﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using System.Drawing.Imaging;

namespace _8beatMap
{
    public partial class Form1 : Form
    {
        System.Resources.ResourceManager DialogResMgr = new System.Resources.ResourceManager("_8beatMap.Dialogs", System.Reflection.Assembly.GetEntryAssembly());
        System.Resources.ResourceManager NotetypeNamesResMgr = new System.Resources.ResourceManager("_8beatMap.NotetypeNames", System.Reflection.Assembly.GetEntryAssembly());
        
        public Notedata.Chart chart = new Notedata.Chart(32 * 48, 120, "New Chart");
        private int TickHeight = 10;
        private int IconWidth = 20;
        private int IconHeight = 10;
        private double CurrentTick = 0;
        private double LastTick = 0;
        
        private DateTime PlaybackVideoTickStartTime = DateTime.UtcNow;


        private System.Timers.Timer playTimer = new System.Timers.Timer(1000/120d);
        public bool IsPlaying
        {
            get { return playTimer.Enabled; }
        }


        private double lastTickForSmoothing = 0;
        private DateTime lastTickChange = DateTime.UtcNow;
        
        private double getSmoothedPlayTickTime(double rawtick)
        {
            //rawtick = getAveragedPlayTickTime(rawtick);

            TimeSpan timeDelta = DateTime.UtcNow - lastTickChange;
            double interpTick = lastTickForSmoothing + chart.ConvertTimeToTicks(timeDelta);
            double absdiff = Math.Abs(rawtick - interpTick);

            if (!this.IsPlaying || timeDelta > TimeSpan.FromMilliseconds(1000) || absdiff > 0.75)
            {
                lastTickForSmoothing = rawtick;
                lastTickChange = DateTime.UtcNow;
                return lastTickForSmoothing;
            }


            if (absdiff > 1) absdiff = 1;
            int tenAbsdiff = (int)(absdiff * 10);

            if (absdiff > 0.2)
            {
                lastTickForSmoothing = (interpTick * (10 - tenAbsdiff) + rawtick) / (11 - tenAbsdiff);
                lastTickChange = DateTime.UtcNow;

                return lastTickForSmoothing;
            }
            else
            {
                return interpTick;
            }
        }


        private double[] prevPlayTicks = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        private double getAveragedPlayTickTime(double rawtick)
        {
            //return getSmoothedPlayTickTime(rawtick);
            rawtick = (rawtick + getSmoothedPlayTickTime(rawtick) * 2) / 3d;

            double avgTickDelta = 0;
            int numentries = 0;
            for (int i = 1; i < prevPlayTicks.Length; i++)
            {
                if (prevPlayTicks[i] >= 0)
                {
                    avgTickDelta += prevPlayTicks[i - 1] - prevPlayTicks[i];
                    numentries++;
                }
            }
            if (numentries > 0)
                avgTickDelta /= numentries;
            else
                avgTickDelta = 0;

            double averagedTick = prevPlayTicks[0] + avgTickDelta;

            if (Math.Abs(rawtick - averagedTick) > 2 | !this.IsPlaying) // averaged tick is too different to raw tick or playtimer isn't enabled -- reset all to default state
            {
                prevPlayTicks[0] = rawtick;
                for (int i = 1; i < prevPlayTicks.Length; i++)
                {
                    prevPlayTicks[i] = -1;
                }
            }
            else
            {
                for (int i = prevPlayTicks.Length - 1; i > 0; --i)
                {
                    prevPlayTicks[i] = prevPlayTicks[i - 1];
                }
                if (numentries > 0)
                    prevPlayTicks[0] = (averagedTick * 2 + rawtick) / 3; // add some portion of rawtick to help avoid drifting
                else
                    prevPlayTicks[0] = rawtick; // if there was no valid average made, it's necessary to just use the raw value provided
            }

            if (prevPlayTicks[0] < 0) prevPlayTicks[0] = 0;
            return (prevPlayTicks[0] + rawtick) / 2;
        }


        GameCloneRenderer_OGL OGLrenderer = null;

        public bool ShowTypeIdsOnNotes = false;


        Image GetChartImage(double startTick, int tickHeight, int iconWidth, int iconHeight, Color BgCol, bool NoGrid, int Width, int Height, float BarNumSize = 9f, float NodeIdSize = 9f, System.Drawing.Text.TextRenderingHint TextAAMode = System.Drawing.Text.TextRenderingHint.SystemDefault, bool DrawBarNumsAfter = false, float ShiftYTicks = 0.5f)
        {
            Image Bmp = new Bitmap(Width, Height);
            Graphics Grfx = Graphics.FromImage(Bmp);

            int width = Bmp.Width;
            int height = Bmp.Height;


            //float FontScale = 72 / Grfx.DpiY; // divide by dpi for height in inches, times 72 for points
            Font BarNumFont = new Font("Arial", BarNumSize, GraphicsUnit.Pixel);
            Font NodeTypeFont = new Font("Arial", NodeIdSize, GraphicsUnit.Pixel);
            Grfx.TextRenderingHint = TextAAMode;

            Grfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            Grfx.Clear(skin.UIColours[UIColours.UIColourDefs.Chart_BG.TypeName]);
            Grfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;


            Brush LaneLineBrush = new SolidBrush(skin.UIColours[UIColours.UIColourDefs.Chart_LaneLine.TypeName]);
            Brush BarLineBrush = new SolidBrush(skin.UIColours[UIColours.UIColourDefs.Chart_BarLine.TypeName]);
            Brush BarTextBrush = new SolidBrush(skin.UIColours[UIColours.UIColourDefs.Chart_BarText.TypeName]);
            Brush QuarterLineBrush = new SolidBrush(skin.UIColours[UIColours.UIColourDefs.Chart_QuarterLine.TypeName]);
            Brush EigthLineBrush = new SolidBrush(skin.UIColours[UIColours.UIColourDefs.Chart_EigthLine.TypeName]);

            
            for (int i = 0; i < 8; i++)
            {
                Color col = skin.UIColours[UIColours.UIColourDefs.Chart_BG_Lane1.TypeName.Replace("1", (i + 1).ToString())];
                if (col.A > 0) Grfx.FillRectangle(new SolidBrush(skin.UIColours[UIColours.UIColourDefs.Chart_BG_Lane1.TypeName.Replace("1", (i+1).ToString())]), i * width / 8, 0, width / 8, height);
                if (i > 0 & !NoGrid) Grfx.FillRectangle(LaneLineBrush, i * width / 8, 0, 1, height);
            }
            


            float laneWidth = width / 8;
            int halfIconWidth = iconWidth / 2;
            int halfIconHeight = iconHeight / 2;

            int iconXOffset = 0;

            if ((laneWidth - iconWidth) % 2 == 1) iconXOffset = 1; // fix spacing if odd number of padding pixels (impossible for even number)

            for (int i = (int)startTick - 24; i < startTick + height / tickHeight; i++)
            {
                if (i >= chart.Length) break;
                if (i < 0) i = 0;

                Notedata.TimeSigChange timesig = chart.GetTimeSigForTick(i);

                if ((i - timesig.StartTick) % (timesig.Numerator * 48 / timesig.Denominator) == 0)
                {
                    Grfx.FillRectangle(BarLineBrush, 0, height - (float)(i - startTick + ShiftYTicks) * tickHeight - 2, width, 1);
                }

                for (int j = 0; j < 8; j++)
                {
                    NoteTypes.NoteTypeDef Type = chart.FindVisualNoteType(i, j);

                    if (!chart.Ticks[i].Notes[j].IsSwipeEnd)
                    {
                        Point swipeEndPoint = chart.Ticks[i].Notes[j].SwipeEndPoint;

                        if (swipeEndPoint.X > i)
                            Grfx.DrawLine(new Pen(skin.EditorNoteColours[NoteTypes.NoteTypeDefs.ExtendHoldMid.TypeName][0], iconWidth / 3), (float)(j + 0.5) * laneWidth, height - (float)(i - startTick + ShiftYTicks + 0.5) * tickHeight - 2, (float)(swipeEndPoint.Y + 0.5) * laneWidth + iconXOffset, height - (float)(swipeEndPoint.X - startTick + ShiftYTicks + 0.5) * tickHeight - 2);
                            
                    }


                    int iconX = (int)((j + 0.5) * laneWidth + iconXOffset - halfIconWidth);
                    int iconY = (int)Math.Ceiling(height - (i - startTick + 1.0 + ShiftYTicks) * tickHeight - 2);

                    Color backColor = skin.EditorNoteColours[Type.TypeName][0];
                    Color iconColor = skin.EditorNoteColours[Type.TypeName][1];

                    if (backColor.A > 0)
                        Grfx.FillRectangle(new SolidBrush(backColor), iconX, iconY, iconWidth, iconHeight);

                    if (iconColor.A > 0)
                    {
                        if (Type.IconType == NoteTypes.IconType.LeftArrow)
                            Grfx.FillPolygon(new SolidBrush(iconColor), new Point[] { new Point(iconX + iconWidth - 1, iconY + 0), new Point(iconX + iconWidth - 1, iconY + iconHeight - 1), new Point(iconX + 0, iconY + halfIconHeight) });
                        else if (Type.IconType == NoteTypes.IconType.RightArrow)
                            Grfx.FillPolygon(new SolidBrush(iconColor), new Point[] { new Point(iconX + 0, iconY + 0), new Point(iconX + 0, iconY + iconHeight - 1), new Point(iconX + iconWidth - 1, iconY + halfIconHeight) });
                        else if (Type.IconType == NoteTypes.IconType.UpArrow)
                            Grfx.FillPolygon(new SolidBrush(iconColor), new Point[] { new Point(iconX + halfIconWidth, iconY + 0), new Point(iconX + iconWidth - 1, iconY + iconHeight - 1), new Point(iconX + 0, iconY + iconHeight - 1) });
                        else if (Type.IconType == NoteTypes.IconType.HalfSplit)
                            Grfx.FillPolygon(new SolidBrush(iconColor), new Point[] { new Point(iconX + iconWidth - 1, iconY + 0), new Point(iconX + iconWidth - 1, iconY + iconHeight - 1), new Point(iconX + 0, iconY + iconHeight - 1) });
                    }

                    if (ShowTypeIdsOnNotes)
                    {
                        int typeId = chart.Ticks[i].Notes[j].NoteType.TypeId;
                        if (typeId != 0)
                        {
                            string typeStr = typeId.ToString();
                            Grfx.DrawString(typeStr, NodeTypeFont, Brushes.White, iconX + halfIconWidth - typeStr.Length * 3.5f, iconY);
                        }
                    }
                }

                if (!NoGrid)
                {
                    if ((i - timesig.StartTick) % (timesig.Numerator * 48 / timesig.Denominator) == 0) // bars
                    {
                        Grfx.FillRectangle(BarLineBrush, 0, height - (float)(i - startTick + ShiftYTicks) * tickHeight - 3, width, 3);
                        if (!DrawBarNumsAfter)
                        {
                            Grfx.DrawString((timesig.StartBar + (i - timesig.StartTick) / (timesig.Numerator * 48 / timesig.Denominator) + 1).ToString(), BarNumFont, BarTextBrush, 0, height - (float)(i - startTick + ShiftYTicks) * tickHeight - 4 - (int)Math.Round(BarNumSize));
                            if (ShowTypeIdsOnNotes) Grfx.DrawString(i.ToString(), BarNumFont, BarTextBrush, width - (int)(BarNumSize*3.5), height - (float)(i - startTick + ShiftYTicks) * tickHeight - 4 - (int)Math.Round(BarNumSize));
                        }
                    }
                    else if ((i - timesig.StartTick) % (48 / timesig.Denominator) == 0) // notes of denominator length -- 48 = one whole note (four quarters)
                    {
                        Grfx.FillRectangle(QuarterLineBrush, 0, height - (float)(i - startTick + ShiftYTicks) * tickHeight - 2, width, 1);
                    }
                    else if ((i - timesig.StartTick) % (24 / timesig.Denominator) == 0) // notes of half denominator length
                    {
                        Grfx.FillRectangle(EigthLineBrush, 0, height - (float)(i - startTick + ShiftYTicks) * tickHeight - 2, width, 1);
                    }
                }
            }

            if (DrawBarNumsAfter)
            {
                for (int i = (int)startTick - 24; i < startTick + height / tickHeight; i++)
                {
                    if (i >= chart.Length) break;
                    if (i < 0) i = 0;

                    Notedata.TimeSigChange timesig = chart.GetTimeSigForTick(i);

                    if ((i - timesig.StartTick) % (timesig.Numerator * 48 / timesig.Denominator) == 0)
                    {
                        // draw bar number after all notes to avoid rendering issue when over holds
                        Grfx.DrawString((timesig.StartBar + (i - timesig.StartTick) / (timesig.Numerator * 48 / timesig.Denominator) + 1).ToString(), BarNumFont, BarTextBrush, 0, height - (float)(i - startTick + ShiftYTicks) * tickHeight - 4 - (int)Math.Round(BarNumSize));
                        if (ShowTypeIdsOnNotes) Grfx.DrawString(i.ToString(), BarNumFont, BarTextBrush, width - (int)(BarNumSize * 3.5), height - (float)(i - startTick + ShiftYTicks) * tickHeight - 4 - (int)Math.Round(BarNumSize));
                    }
                }
            }

            Grfx.Dispose();
            return Bmp;
        }        


        private void SetCurrTick(double tick)
        {
            if (tick < 0) tick = 0;
            if (tick >= chart.Length) tick = chart.Length - 1;

            CurrentTick = getAveragedPlayTickTime(tick);
            if (CurrentTick < 0) CurrentTick = 0;
            if (CurrentTick >= chart.Length) CurrentTick = chart.Length - 1;


            TimeSpan ctickTime = chart.ConvertTicksToTime(tick);


            // this is a mess, but by using clock times for video playback I can help smooth out any remaining jitter
            // it almost works...
            TimeSpan currvidtime = DateTime.UtcNow - PlaybackVideoTickStartTime;

            if (Math.Abs((ctickTime - currvidtime).Ticks) > TimeSpan.FromMilliseconds(25).Ticks)
            {
                PlaybackVideoTickStartTime = DateTime.UtcNow - ctickTime;
                currvidtime = ctickTime;
            }


            if (Sound.MusicReader != null &&
                    (Sound.MusicReader.CurrentTime < ctickTime - TimeSpan.FromMilliseconds(MusicDelayMs + 10) |
                    Sound.MusicReader.CurrentTime > ctickTime - TimeSpan.FromMilliseconds(MusicDelayMs - 10)))
                try {
                    if (ctickTime < TimeSpan.FromMilliseconds(MusicDelayMs))
                        Sound.MusicReader.CurrentTime = TimeSpan.FromMilliseconds(0);
                    else
                        Sound.MusicReader.CurrentTime = ctickTime - TimeSpan.FromMilliseconds(MusicDelayMs);
                }
                catch
                { }

            ChartScrollBar.Value = (int)(chart.Length * TickHeight - CurrentTick * TickHeight);
        }



        int VideoDelayMs = 110;
        int GameCloneOffsetMs = -20;
        
        public void UpdateChart()
        {
            double tick = CurrentTick;
            if (this.IsPlaying)
            {
                // derive ticks from time if in playback (if not use current tick)
                tick = chart.ConvertTimeToTicks(DateTime.UtcNow - PlaybackVideoTickStartTime);
                tick -= chart.ConvertTimeToTicks(TimeSpan.FromMilliseconds(VideoDelayMs));
                //tick -= chart.ConvertTimeToTicks(TimeSpan.FromMilliseconds(MusicDelayMs));
            }

            pictureBox1.Image.Dispose();
            pictureBox1.Image = GetChartImage(tick, TickHeight, IconWidth, IconHeight, SystemColors.ControlLight, false, pictureBox1.Width, pictureBox1.Height);
        }

        public void UpdateGameCloneChart()
        {
            if (OGLrenderer == null)
                return;

            double tick = CurrentTick;
            if (this.IsPlaying)
            {
                // derive ticks from time if in playback (if not use current tick)
                tick = chart.ConvertTimeToTicks(DateTime.UtcNow - PlaybackVideoTickStartTime);
                tick -= chart.ConvertTimeToTicks(TimeSpan.FromMilliseconds(VideoDelayMs+GameCloneOffsetMs));
                //tick -= chart.ConvertTimeToTicks(TimeSpan.FromMilliseconds(MusicDelayMs));
            }

            OGLrenderer.currentTick = tick;
            OGLrenderer.numTicksVisible = (int)chart.ConvertTimeToTicks(TimeSpan.FromMilliseconds(700));
        }


        private int ConvertXCoordToNote(int X)
        {
            return ((X - pictureBox1.Location.X) / (pictureBox1.Width/8));
        }

        private double ConvertYCoordToTick(int Y)
        {
            return (pictureBox1.Location.Y + pictureBox1.Height - Y - 2 + CurrentTick%1 - TickHeight/2) / TickHeight + CurrentTick;
        }





        private void ResizeScrollbar()
        {
            ChartScrollBar.Minimum = 0;
            ChartScrollBar.Maximum = (int)(chart.Length * TickHeight + IconHeight / 2 + 110);

            int newval = (int)(chart.Length * TickHeight - CurrentTick * TickHeight);
            if (newval < ChartScrollBar.Minimum) newval = ChartScrollBar.Minimum;
            else if (newval > ChartScrollBar.Maximum) newval = ChartScrollBar.Maximum;
            ChartScrollBar.Value = newval;
        }


        private void ResizeChart(int NewLen)
        {
            chart.Length = NewLen;
            ResizeScrollbar();
            SetCurrTick(CurrentTick);

            Notedata.TimeSigChange lastticktimesig = chart.GetTimeSigForTick(chart.Length - 1);
            //ResizeBox.Value = chart.Length / 48;
            ResizeBox.Value = lastticktimesig.StartBar + (chart.Length - lastticktimesig.StartTick) / (lastticktimesig.Numerator * 48 / lastticktimesig.Denominator); // bar of last timesig change + ticks left in chart / ticks in a bar

            UpdateChart();
        }

        [System.Runtime.InteropServices.DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint MM_timeBeginPeriod(uint uMilliseconds);
        [System.Runtime.InteropServices.DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint MM_timeEndPeriod(uint uMilliseconds);

        private void StartPlayback()
        {
            try
            {
                MM_timeBeginPeriod(2);
            }
            catch
            { }

            PlaybackVideoTickStartTime = DateTime.UtcNow - chart.ConvertTicksToTime(CurrentTick);
            playTimer.Enabled = true;
            Sound.PlayMusic();
        }

        private void StopPlayback()
        {
            if (playTimer.Enabled) // check this because timeEndPeriod calls should match with timeBeginPeriod calls
            {
                playTimer.Enabled = false;
                
                try
                {
                    MM_timeEndPeriod(2);
                }
                catch
                { }
            }
            Sound.StopMusic();
            UpdateChart();
        }


        private void UpdateWindowTitle()
        {
            if (chart.SongName != "") this.Text = "8beatMap - " + chart.SongName;
            else this.Text = "8beatMap";
        }

        private void LoadChart(string Path)
        {
            StopPlayback();

            if (Path.Length > 0)
            {
                try {
                    chart = Notedata.ConvertJsonToChart(System.IO.File.ReadAllText(Path));

                    chart.FilePath = Path;

                    if (chart.SongName == "")
                    {
                        string name = System.IO.Path.GetFileNameWithoutExtension(Path);
                        if (name.EndsWith(".json")) name = name.Remove(name.Length - 5, 5);
                        if (name.EndsWith(".dec")) name = name.Remove(name.Length - 4, 4);
                        chart.SongName = name;
                    }

                    UpdateWindowTitle();
                }
                catch { SkinnedMessageBox.Show(skin, DialogResMgr.GetString("ChartLoadError"), "", MessageBoxButtons.OK, MessageBoxIcon.Error); }

                if (chart.BPM == 1)
                {
                    SkinnedMessageBox.Show(skin, DialogResMgr.GetString("ChartLoadNoBPM"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    chart.BPM = 120;
                }

                Notedata.TimeSigChange lastticktimesig = chart.GetTimeSigForTick(chart.Length - 1);
                //ResizeBox.Value = chart.Length / 48;
                ResizeBox.Value = lastticktimesig.StartBar + (chart.Length - lastticktimesig.StartTick) / (lastticktimesig.Numerator * 48 / lastticktimesig.Denominator); // bar of last timesig change + ticks left in chart / ticks in a bar

                BPMbox.Value = (decimal)chart.BPM;
                ResizeScrollbar();
                SetCurrTick(0);
                UpdateChart();

            }
        }


        private Dictionary<string, int> currentNoteTypes = new Dictionary<string, int> { };
        private void AddNoteTypes() // to dropdown selector
        {
            int oldindex = 0;
            if (NoteTypeSelector.SelectedIndex > -1) oldindex = NoteTypeSelector.SelectedIndex;

            NoteTypeSelector.Items.Clear();
            currentNoteTypes.Clear();

            foreach (KeyValuePair<string, int> type in NoteTypes.UserVisibleNoteTypes)
            {
                currentNoteTypes.Add(NotetypeNamesResMgr.GetString(type.Key), type.Value);
                NoteTypeSelector.Items.Add(new KeyValuePair<string, int>(NotetypeNamesResMgr.GetString(type.Key), type.Value));
            }
            
            NoteTypeSelector.SelectedIndex = oldindex;
        }


        private void LoadSounds()
        {
            try
            {
                Sound.NoteSoundWave = new Sound.CachedSound(skin.SoundPaths["hit"]);
                Sound.NoteSoundWave_Swipe = new Sound.CachedSound(skin.SoundPaths["swipe"]);
                //NoteSoundMixer.AddMixerInput(NoteSoundWave);
                //NoteSoundMixer.AddMixerInput(NoteSoundWave_Swipe);
                //Sound.SetNoteSoundLatency(95);
            }
            catch
            {
                Sound.NoteSoundWave = null;
                Sound.NoteSoundWave_Swipe = null;
            }
        }

        bool isOpeningPreview = false;
        private void OpenPreviewWindow_Int()
        {
            if (isOpeningPreview) return;
            isOpeningPreview = true;

            int wndWidth = 853;
            int wndHeight = 480;
            int wndX = -99999;
            int wndY = -99999;
            OpenTK.WindowState wndState = OpenTK.WindowState.Normal;

            if (OGLrenderer != null)
            {
                Point wndSize = OGLrenderer.WindowSize;
                Point wndLoc = OGLrenderer.WindowLocation;
                if (wndSize.X > 0 && wndSize.Y > 0)
                {
                    wndX = wndLoc.X;
                    wndY = wndLoc.Y;
                    wndState = OGLrenderer.WindowState;
                    if (wndState != OpenTK.WindowState.Maximized)
                    {
                        wndState = OpenTK.WindowState.Normal;
                        wndWidth = wndSize.X;
                        wndHeight = wndSize.Y;
                    }
                }
                OGLrenderer.Stop();
                OGLrenderer = null;
            }

            OGLrenderer = new GameCloneRenderer_OGL(wndWidth, wndHeight, wndX, wndY, wndState, this, skin, ShowComboNumBox.Checked);
            isOpeningPreview = false;
        }

        private void OpenPreviewWindow()
        {
            // use a thread to avoid blocking UI
            System.Threading.Thread thread = new System.Threading.Thread(OpenPreviewWindow_Int);
            thread.Start();
        }
        

        private Skinning.Skin skin = Skinning.DefaultSkin;
        private void SetSkin(string skin)
        {
            this.skin = Skinning.LoadSkin("skins/" + skin);
            UpdateChart();

            SuspendLayout();
            Skinning.SetBackCol(this, this.skin.UIColours[UIColours.UIColourDefs.Form_BG.TypeName]);
            Skinning.SetForeCol(this, this.skin.UIColours[UIColours.UIColourDefs.Form_Text.TypeName]);
            Skinning.SetUIStyle(this, this.skin.UIStyle);
            if (this.skin.UIStyle == FlatStyle.Flat | this.skin.UIStyle == FlatStyle.Popup) tabControl1.Appearance = TabAppearance.FlatButtons;
            else tabControl1.Appearance = TabAppearance.Normal;
            newplayhead.BackColor = this.skin.UIColours[UIColours.UIColourDefs.Chart_Playhead.TypeName];
            ResumeLayout(false);
            PerformLayout();

            SkinnedMessageBox.defaultskin = this.skin;

            LoadSounds();

            if (this.Visible)
                OpenPreviewWindow();
        }


        public Form1()
        {
            InitializeComponent();
            UpdateWindowTitle();

            this.SuspendLayout();
            this.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 8.8f);
            //this.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.FontFamily, System.Drawing.SystemFonts.MessageBoxFont.SizeInPoints);
            this.AutoScaleMode = AutoScaleMode.None;
            this.ResumeLayout(false);
            this.PerformLayout();

            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            if (!Properties.Settings.Default.AreSettingsInit)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.AreSettingsInit = true;
            }

            DefaultMusicDelayMs = Properties.Settings.Default.DefaultMusicDelay;
            VideoDelayMs = Properties.Settings.Default.VideoDelay;
            GameCloneOffsetMs = Properties.Settings.Default.PreviewTimingOffset;
            Sound.Latency = Properties.Settings.Default.AudioLatency;
            TickHeight = Properties.Settings.Default.ChartZoom;
            NoteSoundBox.Checked = Properties.Settings.Default.NoteSounds;
            PauseOnSeek.Checked = Properties.Settings.Default.PauseOnSeek;
            VolumeBar.Value = Properties.Settings.Default.Volume;
            UseBeepNoteSounds = Properties.Settings.Default.UseBeepNoteSounds;
            ShowComboNumBox.Checked = Properties.Settings.Default.ShowComboInPreview;
            ChangeFormLanguage(Properties.Settings.Default.Language);

            SetSkin(Properties.Settings.Default.Skin);

            AddNoteTypes();
            PopulateSkins();

            MusicDelayMs = (int)AudioDelayBox.Value + DefaultMusicDelayMs;
            Sound.SetVolume(VolumeBar.Value / 100f);
            toolTip1.SetToolTip(VolumeBar, VolumeBar.Value.ToString());

            ZoomBox.Value = TickHeight;
            
            ActiveControl = ZoomLbl;

            Sound.InitWaveOut();

            ResizeScrollbar();
            SetCurrTick(0);
            UpdateChart();

            playTimer.SynchronizingObject = this;
            playTimer.Elapsed += playtimer_Tick;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            OpenPreviewWindow();
        }


        private void ChartScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (PauseOnSeek.Checked) StopPlayback();
            SetCurrTick(chart.Length - e.NewValue / TickHeight);
            UpdateChart();
        }

        private void PlayBtn_Click(object sender, EventArgs e)
        {
            if (Sound.MusicReader != null)
                StartPlayback();
            else
            {
                SkinnedMessageBox.Show(skin, DialogResMgr.GetString("PlaybackNoMusicError"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }
        

        private void LogMissedTick(object sender, EventArgs e)
        {
            //Console.WriteLine("missed playtimer tick");
        }

        static int DefaultMusicDelayMs = 10;
        int MusicDelayMs = DefaultMusicDelayMs;
        bool UseBeepNoteSounds = false;
        DateTime LastSwipeSoundTime = DateTime.UtcNow;
        TimeSpan ConnectedSwipeSoundTimeout = TimeSpan.FromMilliseconds(165);

        private void playtimer_Tick(object sender, EventArgs e)
        {
            playTimer.Elapsed -= playtimer_Tick; //avoid being called again if still running, but don't interrupt timing
            playTimer.Elapsed += LogMissedTick;

            SetCurrTick(chart.ConvertTimeToTicks(Sound.MusicReader.CurrentTime + TimeSpan.FromMilliseconds(MusicDelayMs)));
            UpdateChart(); //(update graphics)

            if ((int)CurrentTick != (int)LastTick)
            {
                int ltick = (int)LastTick;
                LastTick = (int)CurrentTick;

                if ((LastTick - ltick) > 5) //replace the last tick recorded with current tick if time difference is too large
                    ltick = (int)LastTick;

                if (NoteSoundBox.Checked)
                {
                    for (int i = ltick + 1; i <= CurrentTick; i++) //process for all ticks since the last one
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            NoteTypes.NoteTypeDef note = chart.FindVisualNoteType(i, j);

                            if (Sound.NoteSoundWave != null && Sound.NoteSoundWave_Swipe != null && !UseBeepNoteSounds)
                            {
                                if (note.DetectType == NoteTypes.DetectType.Tap | note.DetectType == NoteTypes.DetectType.Hold | note.DetectType == NoteTypes.DetectType.GbsClock)
                                {
                                    //Sound.PlayNoteSound(Sound.NoteSoundWave);
                                    Sound.NoteSoundTrim = new NAudio.Wave.SampleProviders.OffsetSampleProvider(new Sound.CachedSoundSampleProvider(Sound.NoteSoundWave));
                                    //if (MusicDelayMs > 30)
                                    //    Sound.NoteSoundTrim.DelayBy = TimeSpan.FromMilliseconds(MusicDelayMs - 30);
                                    //else
                                    //    Sound.NoteSoundTrim.SkipOver = TimeSpan.FromMilliseconds(30 - MusicDelayMs);
                                    if (DefaultMusicDelayMs > 30)
                                        Sound.NoteSoundTrim.DelayBy = TimeSpan.FromMilliseconds(DefaultMusicDelayMs - 30);
                                    else
                                        Sound.NoteSoundTrim.SkipOver = TimeSpan.FromMilliseconds(30 - DefaultMusicDelayMs);

                                    Sound.PlayNoteSound(Sound.NoteSoundTrim);
                                }

                                else if ((note.DetectType == NoteTypes.DetectType.SwipeEndPoint & !chart.Ticks[i].Notes[j].IsSwipeEnd) ||
                                         note.DetectType == NoteTypes.DetectType.Flick || note.DetectType == NoteTypes.DetectType.HoldEndFlick ||
                                         note.DetectType == NoteTypes.DetectType.GbsFlick ||
                                         (note.DetectType == NoteTypes.DetectType.SwipeDirChange && LastSwipeSoundTime + ConnectedSwipeSoundTimeout < DateTime.UtcNow))
                                {
                                    //Sound.PlayNoteSound(Sound.NoteSoundWave_Swipe);
                                    Sound.NoteSoundTrim = new NAudio.Wave.SampleProviders.OffsetSampleProvider(new Sound.CachedSoundSampleProvider(Sound.NoteSoundWave_Swipe));
                                    //if (MusicDelayMs > 30)
                                    //    Sound.NoteSoundTrim.DelayBy = TimeSpan.FromMilliseconds(MusicDelayMs - 30);
                                    //else
                                    //    Sound.NoteSoundTrim.SkipOver = TimeSpan.FromMilliseconds(30 - MusicDelayMs);
                                    if (DefaultMusicDelayMs > 30)
                                        Sound.NoteSoundTrim.DelayBy = TimeSpan.FromMilliseconds(DefaultMusicDelayMs - 30);
                                    else
                                       Sound.NoteSoundTrim.SkipOver = TimeSpan.FromMilliseconds(30 - DefaultMusicDelayMs);

                                    Sound.PlayNoteSound(Sound.NoteSoundTrim);

                                    LastSwipeSoundTime = DateTime.UtcNow;
                                }
                            }

                            else if (note.NotNode != true & note.DetectType != NoteTypes.DetectType.SwipeMid)
                            {
                                Sound.NoteSoundTrim = new NAudio.Wave.SampleProviders.OffsetSampleProvider(Sound.NoteSoundSig);
                                Sound.NoteSoundTrim.Take = TimeSpan.FromMilliseconds(20);
                                //Sound.NoteSoundTrim.DelayBy = TimeSpan.FromMilliseconds(MusicDelayMs + 5);
                                Sound.NoteSoundTrim.DelayBy = TimeSpan.FromMilliseconds(DefaultMusicDelayMs + 5);
                                Sound.PlayNoteSound(Sound.NoteSoundTrim);
                            }
                        }
                    }
                }
            }

            if (CurrentTick >= chart.Length - 1 || Sound.MusicReader.CurrentTime == Sound.MusicReader.TotalTime)
            {
                StopPlayback();
            }

            playTimer.Elapsed += playtimer_Tick;
            playTimer.Elapsed -= LogMissedTick;
        }

        private void BPMbox_ValueChanged(object sender, EventArgs e)
        {
            chart.BPM = (double)BPMbox.Value;
            ResizeScrollbar();
            if (Sound.MusicReader != null)
            {
                SetCurrTick(chart.ConvertTimeToTicks(Sound.MusicReader.CurrentTime + TimeSpan.FromMilliseconds(MusicDelayMs)));
                UpdateChart();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //SuspendLayout();
            if (pictureBox1.Height != Height)
            {
                if (ClientSize.Height == 0)
                    pictureBox1.Height = 1;
                else
                    pictureBox1.Height = ClientSize.Height;
                Image bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.Image = bmp;
                UpdateChart();
            }
            splitContainer1.Width = ClientSize.Width - (ChartScrollBar.Left + ChartScrollBar.Width) - 1;
            //ResumeLayout();
        }

        private void ProcessClick(int Tick, int Lane, MouseButtons MouseButton, NoteTypes.NoteTypeDef NewNote)
        {
            //Console.WriteLine(Lane + ", " + Tick);

            if (Tick == -1 | Tick >= chart.Length)
                return;

            if (Lane < 0 | Lane > 7)
                return;

            if (MouseButton == MouseButtons.Left)
            {
                if (chart.Ticks[Tick].Notes[Lane].NoteType.TypeId != NewNote.TypeId)
                {
                    if (NewNote.TypeId == NoteTypes.NoteTypeDefs.None.TypeId)
                    {
                        ProcessClick(Tick, Lane, MouseButtons.Right, NewNote);
                        return;
                    }

                    chart.Ticks[Tick].SetNote(NewNote, Lane, ref chart);
                    UpdateChart();
                }

            }

            else if (MouseButton == MouseButtons.Right)
            {
                if (chart.Ticks[Tick].Notes[Lane].NoteType.TypeId != NoteTypes.NoteTypeDefs.None.TypeId)
                {
                    chart.Ticks[Tick].SetNote(NoteTypes.NoteTypeDefs.None, Lane, ref chart);
                    UpdateChart();
                }
            }
        }

        private void Chart_Click(object sender, MouseEventArgs e)
        {
            Control sendCtl = (Control)sender;
            sendCtl.Capture = false;

            pictureBox1.Focus();

            int Lane = ConvertXCoordToNote(e.X);
            int Tick = (int)ConvertYCoordToTick(e.Y);

            ProcessClick(Tick, Lane, e.Button, NoteTypes.NoteTypeDefs.gettypebyid(((KeyValuePair<string, int>)NoteTypeSelector.SelectedItem).Value));
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left | e.Button == MouseButtons.Right)
            {
                int Lane = ConvertXCoordToNote(e.X);
                int Tick = (int)ConvertYCoordToTick(e.Y);

                ProcessClick(Tick, Lane, e.Button, NoteTypes.NoteTypeDefs.gettypebyid(((KeyValuePair<string, int>)NoteTypeSelector.SelectedItem).Value));
            }
        }

        private void Chart_MouseWheel(object sender, MouseEventArgs e)
        {
            SetCurrTick(CurrentTick + e.Delta / 15);
            UpdateChart();
        }


        private void ZoomBox_ValueChanged(object sender, EventArgs e)
        {
            TickHeight = (int)ZoomBox.Value;
            IconHeight = TickHeight;
            ResizeScrollbar();
            UpdateChart();
        }

        private void ResizeBtn_Click(object sender, EventArgs e)
        {
            int tries = 6; // using a fixed number of iterations saves us from having to check somehow, and ensures we won't hang from rounding errors
            int newlen = chart.Length;
            while (tries > 0)
            {
                Notedata.TimeSigChange lastticktimesig = chart.GetTimeSigForTick(newlen);
                while (lastticktimesig.StartBar > (int)ResizeBox.Value) // just skip bars we don't care about
                {
                    lastticktimesig = chart.GetTimeSigForTick(lastticktimesig.StartTick - 1);
                }

                newlen = lastticktimesig.StartTick + ((int)ResizeBox.Value - lastticktimesig.StartBar) * (lastticktimesig.Numerator * 48 / lastticktimesig.Denominator); // tick last timesig change is at + how many extra bars after that * bar length
                tries--;
            }

            //ResizeChart((int)ResizeBox.Value * 48);
            ResizeChart(newlen);
            UpdateChart();
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            if (chart.FilePath != null)
            {
                openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(chart.FilePath);
                openFileDialog1.FileName = System.IO.Path.GetFileName(chart.FilePath);
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
             LoadChart(openFileDialog1.FileName);
        }

        private void SaveChartBtn_Click(object sender, EventArgs e)
        {
            if (chart.FilePath != null)
            {
                saveFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(chart.FilePath);
                saveFileDialog1.FileName = System.IO.Path.GetFileName(chart.FilePath);
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                if (System.IO.File.Exists(saveFileDialog1.FileName))
                {
                    System.IO.File.WriteAllText(saveFileDialog1.FileName + ".tmp", Notedata.ConvertChartToJson_Small(chart));
                    System.IO.File.Replace(saveFileDialog1.FileName + ".tmp", saveFileDialog1.FileName, saveFileDialog1.FileName + ".bak");
                }
                else
                {
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, Notedata.ConvertChartToJson_Small(chart));
                }
        }

        private void OpenMusicButton_Click(object sender, EventArgs e)
        {
            if (Sound.MusicReader != null && Sound.MusicReader.FileName != null)
            {
                openFileDialog2.InitialDirectory = System.IO.Path.GetDirectoryName(Sound.MusicReader.FileName);
                openFileDialog2.FileName = System.IO.Path.GetFileName(Sound.MusicReader.FileName);
            }

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                StopPlayback();
                Sound.LoadMusic(openFileDialog2.FileName, skin);
                SetCurrTick(0);
                UpdateChart();

                MusicDelayMs = -Sound.TryGetMp3FileStartDelay(openFileDialog2.FileName) + DefaultMusicDelayMs;
                AudioDelayBox.Value = MusicDelayMs - DefaultMusicDelayMs;
            }
        }

        private void ImgSaveBtn_Click(object sender, EventArgs e)
        {
            int TicksPerCol = 48 * 8; //8 bars
            int TickHeight = 6;
            int ColWidth = (12 + 5) * 8;
            int ColPadding = 10;
            int NoteHeight = 6;
            int NoteWidth = 12;
            float BarFontSize = 9f;
            float TypeIdFontSize = 6.5f;

            int EdgePaddingX = 12;
            int EdgePaddingY = 12;

            int NumCols = (chart.Ticks.Length - 1) / TicksPerCol + 1;


            int imgWidth = NumCols * (ColWidth + ColPadding) - ColPadding + EdgePaddingX * 2;
            int imgHeight = TicksPerCol * NoteHeight + EdgePaddingY * 2;

            Bitmap img = new Bitmap(imgWidth, imgHeight);
            Graphics grfx = Graphics.FromImage(img);

            grfx.Clear(SystemColors.ControlDark);

            for (int i = 0; i < NumCols; i++)
            {
                Image tempimg = GetChartImage(i * TicksPerCol, TickHeight, NoteWidth, NoteHeight, SystemColors.ControlLight, false, ColWidth, TicksPerCol * NoteHeight + 1, BarFontSize, TypeIdFontSize, System.Drawing.Text.TextRenderingHint.AntiAliasGridFit, true, 0);
                grfx.DrawImage(tempimg, i * (ColWidth + ColPadding) + EdgePaddingX, EdgePaddingY);
                tempimg.Dispose();
            }

            grfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            Font InfoFont = new Font("Arial", 9f, GraphicsUnit.Pixel);
            if (chart.SongName != "") grfx.DrawString("8beatMap - " + chart.SongName + " (" + chart.BPM.ToString() + " BPM)", InfoFont, Brushes.LightGray, EdgePaddingX, imgHeight - 11);
            else grfx.DrawString("8beatMap - ??? (" + chart.BPM.ToString() + " BPM)", InfoFont, Brushes.LightGray, EdgePaddingX, imgHeight - 11);

            img.Save("imgout.png");
            
            grfx.Dispose();
            img.Dispose();
        }

        private void NoteShiftBtn_Click(object sender, EventArgs e)
        {
            if (NoteShiftBox.Value == 0) return;

            chart.ShiftAllNotes((int)NoteShiftBox.Value);

            ResizeScrollbar();
            UpdateChart();

            NoteShiftBox.Value = 0;
        }


        private void SaveConfig()
        {
            try
            {
                Properties.Settings.Default.DefaultMusicDelay = DefaultMusicDelayMs;
                Properties.Settings.Default.VideoDelay = VideoDelayMs;
                Properties.Settings.Default.PreviewTimingOffset = GameCloneOffsetMs;
                Properties.Settings.Default.AudioLatency = Sound.Latency;
                Properties.Settings.Default.Skin = skin.SkinName;
                Properties.Settings.Default.ChartZoom = TickHeight;
                Properties.Settings.Default.NoteSounds = NoteSoundBox.Checked;
                Properties.Settings.Default.PauseOnSeek = PauseOnSeek.Checked;
                Properties.Settings.Default.Volume = VolumeBar.Value;
                Properties.Settings.Default.UseBeepNoteSounds = UseBeepNoteSounds;
                Properties.Settings.Default.ShowComboInPreview = ShowComboNumBox.Checked;
                Properties.Settings.Default.Language = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

                Properties.Settings.Default.Save();
            }
            catch
            {
                SkinnedMessageBox.Show(skin, "Error writing app settings.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (OGLrenderer != null)
                {
                    OGLrenderer.Stop();
                    OGLrenderer = null;
                }

                StopPlayback();

                SaveConfig();
                return;
            }

            if (SkinnedMessageBox.Show(skin, this, DialogResMgr.GetString("ExitMessage"), DialogResMgr.GetString("ExitCaption"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                e.Cancel = true;
            else
            {
                if (OGLrenderer != null)
                {
                    OGLrenderer.Stop();
                    OGLrenderer = null;
                }

                StopPlayback();

                SaveConfig();
            }
        }

        private void NoteCountButton_Click(object sender, EventArgs e)
        {
            SkinnedMessageBox.Show(skin, String.Format(DialogResMgr.GetString("NoteCountMessage"), chart.NoteCount));
        }

        private void ChartDifficultyBtn_Click(object sender, EventArgs e)
        {
            SkinnedMessageBox.Show(skin, String.Format(DialogResMgr.GetString("ChartDifficultyMessage"), chart.AutoDifficultyScore));
        }

        private void AutoSimulBtn_Click(object sender, EventArgs e)
        {
            chart.AutoSetSimulNotes();
            UpdateChart();
        }

        private void ChangeFormLanguage(string culture)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(culture);

            SuspendLayout();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
            resources.ApplyResources(this, "$this");
            this.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 8.8f);
            //this.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.FontFamily, System.Drawing.SystemFonts.MessageBoxFont.SizeInPoints);
            UpdateWindowTitle();
            foreach (Control Ctrl in Controls)
                resources.ApplyResources(Ctrl, Ctrl.Name);

            foreach (Control Ctrl in splitContainer1.Panel1.Controls)
                resources.ApplyResources(Ctrl, Ctrl.Name);
            foreach (Control Ctrl in splitContainer1.Panel2.Controls)
                resources.ApplyResources(Ctrl, Ctrl.Name);

            foreach (TabPage Tab in tabControl1.TabPages)
            {
                resources.ApplyResources(Tab, Tab.Name);
                foreach (Control Ctrl in Tab.Controls)
                    resources.ApplyResources(Ctrl, Ctrl.Name);
            }

            ResumeLayout(false);
            PerformLayout();

            AddNoteTypes();
        }

        private void LangChangeBtn_Click(object sender, EventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ja")
                ChangeFormLanguage("en");
            else
                ChangeFormLanguage("ja");
        }
        
    
    private void Form1_KeyPress(object sender, KeyEventArgs e)
        {
            try
            {
                Keys key = e.KeyCode;
                
                if (ModifierKeys == Keys.Control | ModifierKeys == (Keys.Control | Keys.Shift))
                {
                    if (key == Keys.C)
                    {
                        Notedata.TimeSigChange timesig = chart.GetTimeSigForTick((int)CurrentTick);
                        int copylen = (int)((timesig.Numerator * 48 / timesig.Denominator) * CopyLengthBox.Value);
                        if ((int)CurrentTick + copylen >= chart.Length) copylen = chart.Length - (int)CurrentTick;

                        Notedata.Tick[] copydata = new Notedata.Tick[copylen];

                        for (int i = 0; i < copylen; i++)
                        {
                            copydata[i] = chart.Ticks[(int)CurrentTick + i];
                        }

                        Clipboard.Clear();
                        Clipboard.SetDataObject(copydata, true);

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else if (key == Keys.V)
                    {
                        Type datatype = typeof(Notedata.Tick[]);
                        IDataObject dataobject = Clipboard.GetDataObject();
                        if (dataobject.GetDataPresent(datatype))
                        {
                            Notedata.Tick[] pastedata = (Notedata.Tick[])dataobject.GetData(datatype);

                            int datalen = pastedata.Length;
                            if ((int)CurrentTick + datalen >= chart.Length) datalen = chart.Length - (int)CurrentTick;

                            if (ModifierKeys == Keys.Control)
                            {
                                for (int i = 0; i < datalen; i++)
                                {
                                    chart.Ticks[(int)CurrentTick + i] = pastedata[i];
                                }
                            }
                            else if (ModifierKeys == (Keys.Control | Keys.Shift))
                            {
                                for (int i = 0; i < datalen; i++)
                                {
                                    for (int j = 0; j < 8; j++)
                                    {
                                        NoteTypes.NoteTypeDef note = pastedata[i].Notes[j].NoteType;
                                        switch(note.TypeId)
                                        {
                                            case 13: // FlickLeft
                                                note = NoteTypes.NoteTypeDefs.FlickRight;
                                                break;
                                            case 11: // HoldEndFlickLeft
                                                note = NoteTypes.NoteTypeDefs.HoldEndFlickRight;
                                                break;
                                            case 6: // SwipeLeftStartEnd
                                                note = NoteTypes.NoteTypeDefs.SwipeRightStartEnd;
                                                break;
                                            case 7: // SwipeLeftMid
                                                note = NoteTypes.NoteTypeDefs.SwipeRightMid;
                                                break;
                                            case 14: // SwipeChangeDirR2L
                                                note = NoteTypes.NoteTypeDefs.SwipeChangeDirL2R;
                                                break;
                                            
                                            case 12: // FlickRight
                                                note = NoteTypes.NoteTypeDefs.FlickLeft;
                                                break;
                                            case 10: // HoldEndFlickRight
                                                note = NoteTypes.NoteTypeDefs.HoldEndFlickLeft;
                                                break;
                                            case 4: // SwipeRightStartEnd
                                                note = NoteTypes.NoteTypeDefs.SwipeLeftStartEnd;
                                                break;
                                            case 5: // SwipeRightMid
                                                note = NoteTypes.NoteTypeDefs.SwipeLeftMid;
                                                break;
                                            case 15: // SwipeChangeDirL2R
                                                note = NoteTypes.NoteTypeDefs.SwipeChangeDirR2L;
                                                break;


                                            default:
                                                break;
                                        }

                                        chart.Ticks[(int)CurrentTick + i].Notes[7-j].NoteType = note;
                                    }
                                }
                            }
                            
                            chart.FixNoteInfo();
                            UpdateChart();
                        }

                        e.Handled = true;
                        e.SuppressKeyPress = true;

                    }
                    else if(key == Keys.Q)
                    {
                        AutoSimulBtn_Click(null, null);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else if (key == Keys.D1)
                    {
                        NoteCountButton_Click(null, null);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else if (key == Keys.D)
                    {
                        ChartDifficultyBtn_Click(null, null);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else if (key == Keys.B)
                    {
                        if(tabControl1.SelectedTab != chartSettingsTab) tabControl1.SelectedTab = chartSettingsTab;
                        if (!BPMbox.Focused) BPMbox.Focus();
                        BPMbox.Select(0, BPMbox.Value.ToString("F1").Length);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else if (key == Keys.I)
                    {
                        ChartInfoButton_Click(null, null);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                }
                else switch (key)
                {
                    case Keys.OemQuestion: // toggle showing numbers on keys   question is same as slash in most layouts
                        ShowTypeIdsOnNotes = !ShowTypeIdsOnNotes;
                        UpdateChart();
                        break;

                    case Keys.P: // reopen preview window
                        OpenPreviewWindow();
                        break;
                    
                    case Keys.M:
                        if (OGLrenderer != null)
                            OGLrenderer.clearColor = Color.FromArgb(0, 0, 0, 0);
                        break;
                    case Keys.Oemcomma:
                        if (OGLrenderer != null)
                            OGLrenderer.clearColor = Color.FromArgb(0, 170, 170, 170);
                        break;
                    case Keys.OemPeriod:
                        if (OGLrenderer != null)
                            OGLrenderer.clearColor = Color.FromArgb(0, 255, 255, 255);
                        break;

                    default:
                        NoteTypeSelector.SelectedItem = currentNoteTypes.FirstOrDefault(x => x.Value == NoteTypes.NoteShortcutKeys[key]);
                        break;
                }
            }
            catch { }
        }

        private void PreviewWndBtn_Click(object sender, EventArgs e)
        {
            OpenPreviewWindow();
        }

        private void PopulateSkins()
        {
            SkinSelector.Items.Clear();

            string[] skinlist = System.IO.Directory.GetDirectories("skins");
            foreach (string skinpath in skinlist)
            {
                string skinname = new System.IO.DirectoryInfo(skinpath).Name;
                KeyValuePair<string, string> item = new KeyValuePair<string, string>(skinname, skinname);
                SkinSelector.Items.Add(item);

                if (skin.SkinName == skinname) SkinSelector.SelectedItem = item;
            }
        }

        private void SkinSelector_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SetSkin(((KeyValuePair<string, string>)SkinSelector.SelectedItem).Value);
        }

        private void AudioDelayBox_ValueChanged(object sender, EventArgs e)
        {
            MusicDelayMs = (int)AudioDelayBox.Value + DefaultMusicDelayMs;
        }

        private void VolumeBar_Scroll(object sender, EventArgs e)
        {
            Sound.SetVolume(VolumeBar.Value / 100f);

            //Graphics g = CreateGraphics();
            //int estWidth = (int)(DefaultFont.SizeInPoints * 0.6 * g.DpiX / 72) * VolumeBar.Value.ToString().Length;
            //int estHeight = (int)(DefaultFont.SizeInPoints * g.DpiX / 72);
            //g.Dispose();

            //Point ThumbPos = new Point();
            //ThumbPos.X = 5 + (VolumeBar.Width - 24) * (VolumeBar.Value - VolumeBar.Minimum) / VolumeBar.Maximum - estWidth/2;
            //ThumbPos.Y = VolumeBar.Height - estHeight/4;

            toolTip1.SetToolTip(VolumeBar, VolumeBar.Value.ToString());
            //toolTip1.Show(VolumeBar.Value.ToString(), VolumeBar, ThumbPos, 2000);
        }

        private void NoteShiftBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                NoteShiftBtn_Click(sender, e);
        }

        private void ResizeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ResizeBtn_Click(sender, e);
        }

        private void ChartInfoButton_Click(object sender, EventArgs e)
        {
            ChartInfoDialog chartInfo = new ChartInfoDialog(skin, chart.SongName, chart.Author, Notedata.MakeTimesigChangesString(chart.TimeSigChanges));
            if (chartInfo.ShowDialog() == DialogResult.OK)
            {
                chart.SongName = chartInfo.result[0];
                chart.Author = chartInfo.result[1];
                if (chartInfo.result[2] != "nochange")
                    chart.TimeSigChanges = Notedata.ReadTimesigChangesFromString(chartInfo.result[2]);

                UpdateChart();
            }
            chartInfo.Dispose();

            UpdateWindowTitle();
        }

        private void ShowComboNumBox_CheckedChanged(object sender, EventArgs e)
        {
            OGLrenderer.showcombo = ShowComboNumBox.Checked;
        }
    }
}
