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

        Notedata.Chart chart = new Notedata.Chart(32 * 48, 120);
        private int TickHeight = 10;
        private int IconWidth = 20;
        private int IconHeight = 10;
        private double CurrentTick = 0;
        private int LastTick = 0;


        private Timer playTimer = new Timer() { Interval = 4 };

        WaveOutEvent WaveOut = new WaveOutEvent { DesiredLatency = 100, NumberOfBuffers = 16 };
        WaveFileReader WaveFileReader;

        WaveOutEvent NoteSoundWaveOut = new WaveOutEvent { DesiredLatency = 110, NumberOfBuffers = 4 };
        static NAudio.Wave.SampleProviders.SignalGenerator NoteSoundSig = new NAudio.Wave.SampleProviders.SignalGenerator { Frequency = 1000, Gain = 0.5, Type = NAudio.Wave.SampleProviders.SignalGeneratorType.Square };
        NAudio.Wave.SampleProviders.OffsetSampleProvider NoteSoundTrim;
        NAudio.Wave.SampleProviders.MixingSampleProvider NoteSoundMixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)) { ReadFully = true };


        private struct NoteDataInfo
        {
            public int Tick;
            public int Lane;
            public Notedata.NoteType Type;

            public NoteDataInfo(int Tick, int Lane, Notedata.NoteType Type)
            {
                this.Tick = Tick;
                this.Lane = Lane;
                this.Type = Type;
            }
        }

        private Notedata.NoteType FindVisualNoteType(int tick, int lane)
        {
            if (tick >= chart.Length) return Notedata.NoteType.None;

            if (chart.Ticks[tick].Notes[lane] == Notedata.NoteType.Hold || chart.Ticks[tick].Notes[lane] == Notedata.NoteType.SimulHoldRelease)
            {
                if (tick == 0 || tick == chart.Length - 1) return chart.Ticks[tick].Notes[lane];
                if ((chart.Ticks[tick - 1].Notes[lane] == Notedata.NoteType.Hold ||
                    chart.Ticks[tick - 1].Notes[lane] == Notedata.NoteType.SimulHoldStart ||
                    chart.Ticks[tick - 1].Notes[lane] == Notedata.NoteType.SimulHoldRelease ||
                    chart.Ticks[tick - 1].Notes[lane] == Notedata.NoteType.SwipeLeftStartEnd ||
                    chart.Ticks[tick - 1].Notes[lane] == Notedata.NoteType.SwipeRightStartEnd) &&
                    chart.Ticks[tick + 1].Notes[lane] != Notedata.NoteType.None)
                    return Notedata.NoteType.ExtendHoldMid;
            }
            return chart.Ticks[tick].Notes[lane];
        }


        byte[] swipeEnds;

        private void FixSwipes()
        {
            swipeEnds = new byte[chart.Length * 8];

            for (int i = 0; i < chart.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Notedata.NoteType Type = chart.Ticks[i].Notes[j];

                    if ((Type == Notedata.NoteType.SwipeRightStartEnd | Type == Notedata.NoteType.SwipeRightMid | Type == Notedata.NoteType.SwipeChangeDirL2R) && (swipeEnds[i * 8 + j] == 0))
                    {
                        for (int k = i + 1; k < i + 24; k++)
                        {
                            if (k >= chart.Length) break;
                            int l = j + 1;
                            if (l > 7) break;
                            if (chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeRightStartEnd)
                            {
                                swipeEnds[k * 8 + l] = 1;
                                break;
                            }
                        }
                    }

                    if ((Type == Notedata.NoteType.SwipeLeftStartEnd | Type == Notedata.NoteType.SwipeLeftMid | Type == Notedata.NoteType.SwipeChangeDirR2L) && (swipeEnds[i * 8 + j] == 0))
                    {
                        for (int k = i + 1; k < i + 24; k++)
                        {
                            if (k >= chart.Length) break;
                            int l = j - 1;
                            if (l < 0) break;
                            if (chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeLeftStartEnd)
                            {
                                swipeEnds[k * 8 + l] = 1;
                                break;
                            }
                        }
                    }
                }
            }
        }


        Image GetChartImage(double startTick, int tickHeight, int iconWidth, int iconHeight, Color BgCol, bool NoGrid, Image startImage)
        {
            Image Bmp = startImage;
            Graphics Grfx = Graphics.FromImage(Bmp);

            int width = Bmp.Width;
            int height = Bmp.Height;

            Grfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            Grfx.Clear(BgCol);

            if (!NoGrid)
            {
                for (int i = 1; i < 8; i++)
                {
                    Grfx.FillRectangle(new SolidBrush(Color.LightGray), i * width / 8, 0, 1, height);
                }
            }



            float laneWidth = width / 8;
            float halfIconWidth = iconWidth / 2;
            int halfIconHeight = iconHeight / 2;

            for (int i = (int)startTick - 24; i < startTick + height / tickHeight; i++)
            {
                if (i >= chart.Length) break;
                if (i < 0) i = 0;

                for (int j = 0; j < 8; j++)
                {
                    Color noteCol = Color.LightGray;
                    Color ArrowCol = Color.Transparent;
                    int ArrowDir = 0;

                    Notedata.NoteType Type = FindVisualNoteType(i, j);

                    if ((Type == Notedata.NoteType.SwipeRightStartEnd | Type == Notedata.NoteType.SwipeRightMid | Type == Notedata.NoteType.SwipeChangeDirL2R) && (swipeEnds[i * 8 + j] == 0))
                    {
                        for (int k = i + 1; k < i + 24; k++)
                        {
                            if (k >= chart.Length) break;
                            int l = j + 1;
                            if (l > 7) break;
                            if (chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeRightStartEnd | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeRightMid | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeChangeDirR2L)
                            {
                                Grfx.DrawLine(new Pen(Color.LightGray, iconWidth / 3), (float)(j + 0.5) * laneWidth, height - (float)(i - startTick + 1) * tickHeight - 2, (float)(l + 0.5) * laneWidth, height - (float)(k - startTick + 1) * tickHeight - 2);
                                break;
                            }
                        }
                    }

                    if ((Type == Notedata.NoteType.SwipeLeftStartEnd | Type == Notedata.NoteType.SwipeLeftMid | Type == Notedata.NoteType.SwipeChangeDirR2L) && (swipeEnds[i * 8 + j] == 0))
                    {
                        for (int k = i + 1; k < i + 24; k++)
                        {
                            if (k >= chart.Length) break;
                            int l = j - 1;
                            if (l < 0) break;
                            if (chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeLeftStartEnd | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeLeftMid | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeChangeDirL2R)
                            {
                                Grfx.DrawLine(new Pen(Color.LightGray, iconWidth / 3), (float)(j + 0.5) * laneWidth, height - (float)(i - startTick + 1) * tickHeight - 2, (float)(l + 0.5) * laneWidth, height - (float)(k - startTick + 1) * tickHeight - 2);
                                break;
                            }
                        }
                    }

                    switch (Type)
                    {
                        case Notedata.NoteType.Tap: noteCol = Color.Blue; break;
                        case Notedata.NoteType.Hold: noteCol = Color.LimeGreen; break;
                        case Notedata.NoteType.SimulTap:
                        case Notedata.NoteType.SimulHoldStart:
                        case Notedata.NoteType.SimulHoldRelease: noteCol = Color.DeepPink; break;
                        case Notedata.NoteType.FlickLeft:
                        case Notedata.NoteType.HoldEndFlickLeft: ArrowCol = Color.FromArgb(0x70, 0, 0x78); ArrowDir = -1; break;
                        case Notedata.NoteType.SwipeLeftStartEnd: ArrowCol = Color.DarkViolet; ArrowDir = -1; break;
                        case Notedata.NoteType.SwipeLeftMid:
                        case Notedata.NoteType.SwipeChangeDirR2L: ArrowCol = Color.Violet; ArrowDir = -1; break;
                        case Notedata.NoteType.FlickRight:
                        case Notedata.NoteType.HoldEndFlickRight: ArrowCol = Color.FromArgb(0xcc, 0x88, 0); ArrowDir = 1; break;
                        case Notedata.NoteType.SwipeRightStartEnd: ArrowCol = Color.DarkOrange; ArrowDir = 1; break;
                        case Notedata.NoteType.SwipeRightMid:
                        case Notedata.NoteType.SwipeChangeDirL2R: ArrowCol = Color.Gold; ArrowDir = 1; break;
                        case Notedata.NoteType.ExtendHoldMid: noteCol = Color.LightGray; break;
                    }


                    if (chart.Ticks[i].Notes[j] != Notedata.NoteType.None)
                    {
                        int iconX = (int)((j + 0.5) * laneWidth - halfIconWidth);
                        int iconY = (int)Math.Ceiling(height - (i - startTick + 1.5) * tickHeight - 2);

                        Grfx.FillRectangle(new SolidBrush(noteCol), iconX, iconY, iconWidth, iconHeight);
                        if (ArrowDir == -1)
                            Grfx.FillPolygon(new SolidBrush(ArrowCol), new Point[] { new Point(iconX + iconWidth - 1, iconY + 0), new Point(iconX + iconWidth - 1, iconY + iconHeight - 1), new Point(iconX + 0, iconY + halfIconHeight) });
                        else if (ArrowDir == 1)
                            Grfx.FillPolygon(new SolidBrush(ArrowCol), new Point[] { new Point(iconX + 0, iconY + 0), new Point(iconX + 0, iconY + iconHeight - 1), new Point(iconX + iconWidth - 1, iconY + halfIconHeight) });
                    }
                }

                if (!NoGrid)
                {
                    if (i % 48 == 0)
                    {
                        Grfx.FillRectangle(new SolidBrush(Color.SlateGray), 0, height - (float)(i - startTick + 0.5) * tickHeight - 3, width, 3);
                        //Grfx.DrawString((i / 48 + 1).ToString(), new System.Drawing.Font("Arial", 6.5f), new SolidBrush(Color.DarkSlateGray), 0, height - (float)(i - startTick + 0.5) * tickHeight - 13);
                    }
                    else if (i % 12 == 0)
                    {
                        Grfx.FillRectangle(new SolidBrush(Color.LightSlateGray), 0, height - (float)(i - startTick + 0.5) * tickHeight - 2, width, 1);
                    }
                    else if (i % 6 == 0)
                    {
                        Grfx.FillRectangle(new SolidBrush(Color.LightGray), 0, height - (float)(i - startTick + 0.5) * tickHeight - 2, width, 1);
                    }
                }
            }

            Grfx.Dispose();
            return Bmp;
        }

        PointF GetPointAlongLine(PointF start, PointF end, float distance)
        {
            return new PointF(start.X + (end.X - start.X) * distance, start.Y + (end.Y - start.Y) * distance);
        }

        Image spr_HoldLocus;
        Image spr_SwipeLocus;
        Image spr_TapIcon;
        Image spr_HoldIcon;
        Image spr_SimulIcon;
        Image spr_SwipeRightIcon;
        Image spr_SwipeRightIcon_Simul;
        Image spr_SwipeLeftIcon;
        Image spr_SwipeLeftIcon_Simul;
        Image spr_HitEffect;
        Image spr_Chara1;
        Image spr_Chara2;
        Image spr_Chara3;
        Image spr_Chara4;
        Image spr_Chara5;
        Image spr_Chara6;
        Image spr_Chara7;
        Image spr_Chara8;

        Image GetGameCloneImage(double startTick, int numTicksVisible, Color BgCol, Image startImage)
        {
            Image Bmp = startImage;
            Graphics Grfx = Graphics.FromImage(Bmp);
            Image HoldBmp = (Image)startImage.Clone();
            Graphics HoldGrfx = Graphics.FromImage(HoldBmp);

            int width = Bmp.Width;
            int height = Bmp.Height;
            float scalefactor = (float)width / 1136;

            PointF[] NodeStartLocs = { new PointF(223*scalefactor, 77*scalefactor), new PointF(320*scalefactor, 100*scalefactor), new PointF(419*scalefactor, 114*scalefactor), new PointF(519*scalefactor, 119*scalefactor), new PointF(617*scalefactor, 119*scalefactor), new PointF(717*scalefactor, 114*scalefactor), new PointF(816*scalefactor, 100*scalefactor), new PointF(923*scalefactor, 77*scalefactor) };
            PointF[] NodeEndLocs = { new PointF(75*scalefactor, height-156*scalefactor), new PointF(213*scalefactor, height-120*scalefactor), new PointF(354*scalefactor, height-98*scalefactor), new PointF(497*scalefactor, height-88*scalefactor), new PointF(639*scalefactor, height-88*scalefactor), new PointF(782*scalefactor, height-98*scalefactor), new PointF(923*scalefactor, height-120*scalefactor), new PointF(1061*scalefactor, height-156*scalefactor) };

            int iconSize = (int)(128 * scalefactor);

            Grfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            Grfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            HoldGrfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            HoldGrfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            Grfx.Clear(BgCol);
            HoldGrfx.Clear(Color.Transparent);

            ColorMatrix transpMatrix = new ColorMatrix();
            transpMatrix.Matrix33 = 0.7f;
            ImageAttributes transpAttr = new ImageAttributes();
            transpAttr.SetColorMatrix(transpMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);



            int EffectTime = 1000000;
            int EffectFadeTime = 390000;
            double EffectTicks = ConvertTimeToTicks(new TimeSpan(EffectTime));
            double EffectFadeTicks = ConvertTimeToTicks(new TimeSpan(EffectFadeTime));

            ColorMatrix effectTranspMatrix = new ColorMatrix();
            ImageAttributes effectTranspAttr = new ImageAttributes();


            Grfx.DrawImage(spr_Chara1, NodeEndLocs[0].X - iconSize / 2, NodeEndLocs[0].Y - iconSize / 2, iconSize, iconSize);
            Grfx.DrawImage(spr_Chara2, NodeEndLocs[1].X - iconSize / 2, NodeEndLocs[1].Y - iconSize / 2, iconSize, iconSize);
            Grfx.DrawImage(spr_Chara3, NodeEndLocs[2].X - iconSize / 2, NodeEndLocs[2].Y - iconSize / 2, iconSize, iconSize);
            Grfx.DrawImage(spr_Chara4, NodeEndLocs[3].X - iconSize / 2, NodeEndLocs[3].Y - iconSize / 2, iconSize, iconSize);
            Grfx.DrawImage(spr_Chara5, NodeEndLocs[4].X - iconSize / 2, NodeEndLocs[4].Y - iconSize / 2, iconSize, iconSize);
            Grfx.DrawImage(spr_Chara6, NodeEndLocs[5].X - iconSize / 2, NodeEndLocs[5].Y - iconSize / 2, iconSize, iconSize);
            Grfx.DrawImage(spr_Chara7, NodeEndLocs[6].X - iconSize / 2, NodeEndLocs[6].Y - iconSize / 2, iconSize, iconSize);
            Grfx.DrawImage(spr_Chara8, NodeEndLocs[7].X - iconSize / 2, NodeEndLocs[7].Y - iconSize / 2, iconSize, iconSize);


            //for (int i = (int)startTick - 24; i < startTick + numTicksVisible + 24; i++)
            for (int i = (int)startTick + numTicksVisible + 24; i >= (int)startTick; i--)
            {
                if (i > chart.Length) i = chart.Length;
                if (i < 0) break;

                for (int j = 7; j > -1; j--)
                {
                    Notedata.NoteType Type = FindVisualNoteType(i, j);

                    if ((Type == Notedata.NoteType.SwipeRightStartEnd | Type == Notedata.NoteType.SwipeRightMid | Type == Notedata.NoteType.SwipeChangeDirL2R) && (swipeEnds[i * 8 + j] == 0))
                    {
                        for (int k = i + 1; k < i + 24; k++)
                        {
                            if (k >= chart.Length) break;
                            int l = j + 1;
                            if (l > 7) break;
                            if (chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeRightStartEnd | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeRightMid | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeChangeDirR2L)
                            {
                                /*PointF iPoint = GetPointAlongLine(NodeStartLocs[j], NodeEndLocs[j], (float)(numTicksVisible - i + startTick) / numTicksVisible);
                                PointF kPoint = GetPointAlongLine(NodeStartLocs[l], NodeEndLocs[l], (float)(numTicksVisible - k + startTick) / numTicksVisible);
                                Grfx.DrawImage(spr_SwipeLocus, new PointF[] { iPoint, kPoint, new PointF(iPoint.X, iPoint.Y+iconSize/2) }, new Rectangle(0, 0, spr_SwipeLocus.Width-1, spr_SwipeLocus.Height), GraphicsUnit.Pixel);
                                Grfx.FillPolygon(Brushes.Transparent, new PointF[] { new PointF(iPoint.X, iPoint.Y+iconSize/2 * (float)(numTicksVisible - i + startTick) / numTicksVisible),
                                   new PointF(kPoint.X, kPoint.Y+iconSize/2 * (float)(numTicksVisible - k + startTick) / numTicksVisible),
                                   new PointF(kPoint.X, kPoint.Y+iconSize/2), new PointF(iPoint.X, iPoint.Y+iconSize/2)});*/
                                float iDist = (float)(numTicksVisible - i + startTick) / numTicksVisible;
                                float kDist = (float)(numTicksVisible - k + startTick) / numTicksVisible;
                                float iSize = iconSize / 4 * iDist;
                                float kSize = iconSize / 4 * kDist;
                                PointF iPoint = GetPointAlongLine(NodeStartLocs[j], NodeEndLocs[j], iDist);
                                PointF kPoint = GetPointAlongLine(NodeStartLocs[l], NodeEndLocs[l], kDist);
                                Grfx.DrawImage(spr_SwipeLocus, new PointF[] { new PointF(iPoint.X, iPoint.Y - iSize), new PointF(kPoint.X, kPoint.Y - kSize), new PointF(iPoint.X, iPoint.Y - iSize + iconSize / 2) }, new Rectangle(0, 0, spr_SwipeLocus.Width - 1, spr_SwipeLocus.Height), GraphicsUnit.Pixel, transpAttr);
                                Grfx.FillPolygon(Brushes.Transparent, new PointF[] { new PointF(iPoint.X, iPoint.Y + iSize),
                                   new PointF(kPoint.X, kPoint.Y + kSize),
                                   new PointF(kPoint.X, kPoint.Y-kSize+iconSize/2), new PointF(iPoint.X, iPoint.Y-iSize+iconSize/2)});

                                break;
                            }
                        }
                    }

                    if ((Type == Notedata.NoteType.SwipeLeftStartEnd | Type == Notedata.NoteType.SwipeLeftMid | Type == Notedata.NoteType.SwipeChangeDirR2L) && (swipeEnds[i * 8 + j] == 0))
                    {
                        for (int k = i + 1; k < i + 24; k++)
                        {
                            if (k >= chart.Length) break;
                            int l = j - 1;
                            if (l < 0) break;
                            if (chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeLeftStartEnd | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeLeftMid | chart.Ticks[k].Notes[l] == Notedata.NoteType.SwipeChangeDirL2R)
                            {
                                float iDist = (float)(numTicksVisible - i + startTick) / numTicksVisible;
                                float kDist = (float)(numTicksVisible - k + startTick) / numTicksVisible;
                                float iSize = iconSize / 4 * iDist;
                                float kSize = iconSize / 4 * kDist;
                                PointF iPoint = GetPointAlongLine(NodeStartLocs[j], NodeEndLocs[j], iDist);
                                PointF kPoint = GetPointAlongLine(NodeStartLocs[l], NodeEndLocs[l], kDist);
                                Grfx.DrawImage(spr_SwipeLocus, new PointF[] { new PointF(iPoint.X, iPoint.Y - iSize), new PointF(kPoint.X, kPoint.Y - kSize), new PointF(iPoint.X, iPoint.Y - iSize + iconSize / 2) }, new Rectangle(0, 0, spr_SwipeLocus.Width - 1, spr_SwipeLocus.Height), GraphicsUnit.Pixel, transpAttr);
                                Grfx.FillPolygon(Brushes.Transparent, new PointF[] { new PointF(iPoint.X, iPoint.Y + iSize),
                                   new PointF(kPoint.X, kPoint.Y + kSize),
                                   new PointF(kPoint.X, kPoint.Y-kSize+iconSize/2), new PointF(iPoint.X, iPoint.Y-iSize+iconSize/2)});

                                break;
                            }
                        }
                    }

                    if (Type == Notedata.NoteType.ExtendHoldMid && (i == (int)startTick | FindVisualNoteType(i - 1, j) != Notedata.NoteType.ExtendHoldMid))
                    {
                        int start = i;
                        if (start <= startTick) start = (int)startTick + 1;
                        int end = i;
                        while (FindVisualNoteType(end, j) == Notedata.NoteType.ExtendHoldMid) end++;
                        if (end <= start) continue;

                        float sDist = (float)(numTicksVisible - start + 1 + startTick) / numTicksVisible;
                        float eDist = (float)(numTicksVisible - end + startTick) / numTicksVisible;
                        float sSize = iconSize / 2 * sDist;
                        float eSize = iconSize / 2 * eDist;
                        PointF sPoint = GetPointAlongLine(NodeStartLocs[j], NodeEndLocs[j], sDist);
                        PointF ePoint = GetPointAlongLine(NodeStartLocs[j], NodeEndLocs[j], eDist);
                        HoldGrfx.DrawImage(spr_HoldLocus, new PointF[] { new PointF(sPoint.X + sSize, sPoint.Y), new PointF(sPoint.X + sSize - iconSize, sPoint.Y), new PointF(ePoint.X + eSize, ePoint.Y) }, new Rectangle(0, 0, spr_HoldLocus.Width, spr_HoldLocus.Height - 1), GraphicsUnit.Pixel, transpAttr);
                        HoldGrfx.FillPolygon(Brushes.Transparent, new PointF[] { new PointF(sPoint.X - sSize, sPoint.Y), new PointF(ePoint.X - eSize, ePoint.Y),
                           new PointF(ePoint.X + eSize -iconSize, ePoint.Y), new PointF(sPoint.X  + sSize - iconSize, sPoint.Y)});
                    }

                }
            }
            
            Grfx.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            Grfx.DrawImage(HoldBmp, 0, 0);
            HoldGrfx.Dispose();
            HoldBmp.Dispose();
            

            //for (int i = (int)(startTick - ConvertTimeToTicks(new TimeSpan(1000000))); i <= (int)startTick + numTicksVisible; i++)
            for (int i = (int)startTick + numTicksVisible; i >= (int)(startTick - EffectTicks - EffectFadeTicks - 1); i--)
            {
                if (i > chart.Length) i = chart.Length;
                if (i < 0) break;

                for (int j = 7; j > -1; j--)
                {
                    Notedata.NoteType Type = FindVisualNoteType(i, j);

                    if (Type == Notedata.NoteType.None | Type == Notedata.NoteType.ExtendHoldMid) continue;

                    if (i >= (int)startTick)
                    {
                        Image NoteImg;

                        switch (Type)
                        {
                            case Notedata.NoteType.Tap: NoteImg = spr_TapIcon; break;
                            case Notedata.NoteType.Hold: NoteImg = spr_HoldIcon; break;
                            case Notedata.NoteType.SimulTap:
                            case Notedata.NoteType.SimulHoldStart:
                            case Notedata.NoteType.SimulHoldRelease: NoteImg = spr_SimulIcon; break;
                            case Notedata.NoteType.FlickLeft:
                            case Notedata.NoteType.HoldEndFlickLeft:
                            case Notedata.NoteType.SwipeLeftStartEnd:
                            case Notedata.NoteType.SwipeLeftMid:
                            case Notedata.NoteType.SwipeChangeDirR2L:
                                NoteImg = spr_SwipeLeftIcon;
                                for (int k = 7; k > -1; k--)
                                {
                                    if (chart.Ticks[i].Notes[k] == Notedata.NoteType.SimulTap | chart.Ticks[i].Notes[k] == Notedata.NoteType.SimulHoldStart | chart.Ticks[i].Notes[k] == Notedata.NoteType.SimulHoldRelease)
                                    {
                                        NoteImg = spr_SwipeLeftIcon_Simul;
                                        break;
                                    }
                                }
                                break;
                            case Notedata.NoteType.FlickRight:
                            case Notedata.NoteType.HoldEndFlickRight:
                            case Notedata.NoteType.SwipeRightStartEnd:
                            case Notedata.NoteType.SwipeRightMid:
                            case Notedata.NoteType.SwipeChangeDirL2R:
                                NoteImg = spr_SwipeRightIcon;
                                for (int k = 7; k > -1; k--)
                                {
                                    if (chart.Ticks[i].Notes[k] == Notedata.NoteType.SimulTap | chart.Ticks[i].Notes[k] == Notedata.NoteType.SimulHoldStart | chart.Ticks[i].Notes[k] == Notedata.NoteType.SimulHoldRelease)
                                    {
                                        NoteImg = spr_SwipeRightIcon_Simul;
                                        break;
                                    }
                                }
                                break;
                            default: NoteImg = new Bitmap(1, 1); break;
                        }

                        float icnDist = (float)(numTicksVisible - i + startTick) / numTicksVisible;
                        PointF icnPoint = GetPointAlongLine(NodeStartLocs[j], NodeEndLocs[j], icnDist);
                        float icnSize = iconSize * 1.375f * icnDist;
                        Grfx.DrawImage(NoteImg, icnPoint.X - icnSize / 2, icnPoint.Y - icnSize / 2, icnSize, icnSize);

                    }
                    else if (i >= (int)(startTick - EffectTicks - 1))
                    {
                        int effectSize = (int)(((startTick - i - 1) / EffectTicks + 1) * iconSize * 1.375f);
                        Grfx.DrawImage(spr_HitEffect, NodeEndLocs[j].X-effectSize/2, NodeEndLocs[j].Y-effectSize/2, effectSize, effectSize);
                    }
                    else if (i >= (int)(startTick - EffectTicks - EffectFadeTicks - 1))
                    {
                        int effectSize = (int)(iconSize * 2.75);
                        float effectOpacity = 1 - (float)((startTick - EffectTicks - i - 1) / EffectFadeTicks * 0.8f);

                        effectTranspMatrix.Matrix33 = effectOpacity;
                        effectTranspAttr.SetColorMatrix(effectTranspMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                        Grfx.DrawImage(spr_HitEffect, new Rectangle((int)NodeEndLocs[j].X - effectSize / 2, (int)NodeEndLocs[j].Y - effectSize / 2, effectSize, effectSize), 0, 0, spr_HitEffect.Width, spr_HitEffect.Height, GraphicsUnit.Pixel, effectTranspAttr);
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

            CurrentTick = tick;

            ChartScrollBar.Value = (int)(chart.Length * TickHeight - tick * TickHeight);
        }

        private void UpdateChart()
        {
            pictureBox1.Image = GetChartImage(CurrentTick, TickHeight, IconWidth, IconHeight, SystemColors.ControlLight, false, pictureBox1.Image);
            if(Form2.Visible)
                GameClone.Image = GetGameCloneImage(CurrentTick, 24, Color.Transparent, GameClone.Image);
        }

        private int ConvertXCoordToNote(int X)
        {
            return ((X - pictureBox1.Location.X) / (pictureBox1.Width/8));
        }

        private double ConvertYCoordToTick(int Y)
        {
            return (pictureBox1.Location.Y + pictureBox1.Height - Y - 2 + CurrentTick%1 - TickHeight/2) / TickHeight + CurrentTick;
        }


        private TimeSpan ConvertTicksToTime(double ticks)
        {
            TimeSpan a = TimeSpan.FromSeconds((5 * ticks / chart.BPM));
            return TimeSpan.FromSeconds((5 * ticks / chart.BPM));
        }

        private double ConvertTimeToTicks(TimeSpan time)
        {
            return time.TotalSeconds / (double)(5/BPMbox.Value);
        }


        private void ResizeScrollbar()
        {
            ChartScrollBar.Minimum = 0;
            ChartScrollBar.Maximum = (int)(chart.Length * TickHeight + IconHeight / 2 + 110);
        }


        private void ResizeChart(int NewLen)
        {
            chart.Length = NewLen;
            ResizeScrollbar();
            SetCurrTick(CurrentTick);
            UpdateChart();
        }


        private void StartPlayback()
        {
            playTimer.Enabled = true;
            WaveOut.Play();
        }

        private void StopPlayback()
        {
            playTimer.Enabled = false;
            WaveOut.Pause();
        }


        private void LoadChart(string Path)
        {
            if (Path.Length > 0)
            {
                try {
                    chart = Notedata.ConvertJsonToChart(System.IO.File.ReadAllText(Path));
                }
                catch { MessageBox.Show(DialogResMgr.GetString("ChartLoadError")); }

                if (chart.BPM == 1)
                {
                    MessageBox.Show(DialogResMgr.GetString("ChartLoadNoBPM"));
                    chart.BPM = 120;
                }
                ResizeBox.Value = chart.Length / 48;
                BPMbox.Value = (decimal)chart.BPM;
                ResizeScrollbar();
                SetCurrTick(0);
                FixSwipes();
                UpdateChart();

            }
        }


        private void LoadMusic(string Path)
        {
            if (Path.Length > 0)
            {
                WaveOut.Stop();

                try {
                    WaveFileReader = new WaveFileReader(Path);
                }
                catch { MessageBox.Show(DialogResMgr.GetString("MusicLoadError")); return; }
                
                WaveOut.Init(WaveFileReader);
            }
        }


        private void AddNoteTypes()
        {
            NoteTypeSelector.Items.Clear();

            if (System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ja")
                //NoteTypeSelector.DataSource = Enum.GetValues(typeof(Notedata.UserVisibleNoteType_Nihongo));
                foreach (Notedata.UserVisibleNoteType_Nihongo type in Enum.GetValues(typeof(Notedata.UserVisibleNoteType_Nihongo)))
                {
                    NoteTypeSelector.Items.Add(type);
                }
            else
                // NoteTypeSelector.DataSource = Enum.GetValues(typeof(Notedata.UserVisibleNoteType));
                foreach (Notedata.UserVisibleNoteType type in Enum.GetValues(typeof(Notedata.UserVisibleNoteType)))
                {
                    NoteTypeSelector.Items.Add(type);
                }

            NoteTypeSelector.SelectedIndex = 0;
        }

        PictureBox GameClone = new PictureBox{ Image = new Bitmap(853, 480), Size = new Size(853, 480), Location = new Point(0, 0), BackColor = Color.Black };
        Form Form2 = new Form() { ClientSize = new Size(853, 480) };
        public Form1()
        {
            InitializeComponent();



            Form2.Controls.Add(GameClone);
            Form2.Show();

            try
            {
                spr_HoldLocus = Image.FromFile("nodeimg/locus.png");
                spr_SwipeLocus = Image.FromFile("nodeimg/locus2.png");
                spr_TapIcon = Image.FromFile("nodeimg/node_1.png");
                spr_HoldIcon = Image.FromFile("nodeimg/node_2.png");
                spr_SimulIcon = Image.FromFile("nodeimg/node_3.png");
                spr_SwipeRightIcon = Image.FromFile("nodeimg/node_4.png");
                spr_SwipeRightIcon_Simul = Image.FromFile("nodeimg/node_4_3.png");
                spr_SwipeLeftIcon = Image.FromFile("nodeimg/node_6.png");
                spr_SwipeLeftIcon_Simul = Image.FromFile("nodeimg/node_6_3.png");
                spr_HitEffect = Image.FromFile("nodeimg/node_effect.png");
                spr_Chara1 = Image.FromFile("charaimg/1.png");
                spr_Chara2 = Image.FromFile("charaimg/2.png");
                spr_Chara3 = Image.FromFile("charaimg/3.png");
                spr_Chara4 = Image.FromFile("charaimg/4.png");
                spr_Chara5 = Image.FromFile("charaimg/5.png");
                spr_Chara6 = Image.FromFile("charaimg/6.png");
                spr_Chara7 = Image.FromFile("charaimg/7.png");
                spr_Chara8 = Image.FromFile("charaimg/8.png");
            }
            catch
            {
                spr_HoldLocus = new Bitmap(1, 1);
                spr_SwipeLocus = new Bitmap(1, 1);
                spr_TapIcon = new Bitmap(1, 1);
                spr_HoldIcon = new Bitmap(1, 1);
                spr_SimulIcon = new Bitmap(1, 1);
                spr_SwipeRightIcon = new Bitmap(1, 1);
                spr_SwipeRightIcon_Simul = new Bitmap(1, 1);
                spr_SwipeLeftIcon = new Bitmap(1, 1);
                spr_SwipeLeftIcon_Simul = new Bitmap(1, 1);
                spr_HitEffect = new Bitmap(1, 1);
                spr_Chara1 = new Bitmap(1, 1);
                spr_Chara2 = new Bitmap(1, 1);
                spr_Chara3 = new Bitmap(1, 1);
                spr_Chara4 = new Bitmap(1, 1);
                spr_Chara5 = new Bitmap(1, 1);
                spr_Chara6 = new Bitmap(1, 1);
                spr_Chara7 = new Bitmap(1, 1);
                spr_Chara8 = new Bitmap(1, 1);
            }


            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            AddNoteTypes();

            ActiveControl = ZoomLbl;

            NoteSoundWaveOut.Init(NoteSoundMixer);
            NoteSoundWaveOut.Play();

            ResizeScrollbar();
            SetCurrTick(0);
            FixSwipes();
            UpdateChart();

            playTimer.Tick += playtimer_Tick;
        }

        private void ChartScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (PauseOnSeek.Checked) StopPlayback();
            SetCurrTick(chart.Length - e.NewValue / TickHeight);
            UpdateChart();
            if (WaveFileReader != null)
                try { WaveFileReader.CurrentTime = ConvertTicksToTime(CurrentTick); } catch { }
        }

        private void PlayBtn_Click(object sender, EventArgs e)
        {
            if (WaveFileReader != null)
                StartPlayback();
            else
            {
                MessageBox.Show(DialogResMgr.GetString("PlaybackNoMusicError"));
            }
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }


        private void playtimer_Tick(object sender, EventArgs e)
        {
            SetCurrTick(ConvertTimeToTicks(WaveFileReader.CurrentTime));
            UpdateChart();

            if ((int)CurrentTick != LastTick)
            {
                int ltick = LastTick;
                LastTick = (int)CurrentTick;

                if (NoteSoundBox.Checked)
                {
                    for (int i = ltick + 1; i <= CurrentTick; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            Notedata.NoteType note = FindVisualNoteType(i, j);

                            if (note != Notedata.NoteType.None && note != Notedata.NoteType.ExtendHoldMid &&
                                note != Notedata.NoteType.SwipeLeftMid && note != Notedata.NoteType.SwipeRightMid)
                            {
                                NoteSoundTrim = new NAudio.Wave.SampleProviders.OffsetSampleProvider(NoteSoundSig);
                                NoteSoundTrim.Take = TimeSpan.FromMilliseconds(20);
                                NoteSoundMixer.AddMixerInput(NoteSoundTrim);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void BPMbox_ValueChanged(object sender, EventArgs e)
        {
            chart.BPM = (double)BPMbox.Value;
            ResizeScrollbar();
            if (WaveFileReader != null)
            {
                SetCurrTick(ConvertTimeToTicks(WaveFileReader.CurrentTime));
                UpdateChart();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (pictureBox1.Height != Height)
            {
                pictureBox1.Height = ClientSize.Height;
                Image bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.Image = bmp;
                UpdateChart();
            }
        }

        private void ProcessClick(int Tick, int Lane, MouseButtons MouseButton, Notedata.NoteType NewNote)
        {
            Console.WriteLine(Lane + ", " + Tick);

            if (Tick == -1 || Tick >= chart.Length)
                return;

            if (MouseButton == MouseButtons.Left)
            {
                if (chart.Ticks[Tick].Notes[Lane] != NewNote)
                {
                    if (NewNote == Notedata.NoteType.None)
                    {
                        ProcessClick(Tick, Lane, MouseButtons.Right, NewNote);
                        return;
                    }

                    chart.Ticks[Tick].Notes[Lane] = NewNote;
                    FixSwipes();
                    UpdateChart();
                }

            }

            else if (MouseButton == MouseButtons.Right)
            {
                if (chart.Ticks[Tick].Notes[Lane] != Notedata.NoteType.None)
                {
                    chart.Ticks[Tick].Notes[Lane] = Notedata.NoteType.None;
                    FixSwipes();
                    UpdateChart();
                }
            }
        }

        private void Chart_Click(object sender, MouseEventArgs e)
        {
            Control sendCtl = (Control)sender;
            sendCtl.Capture = false;

            int Lane = ConvertXCoordToNote(e.X);
            int Tick = (int)ConvertYCoordToTick(e.Y);

            ProcessClick(Tick, Lane, e.Button, (Notedata.NoteType)NoteTypeSelector.SelectedItem);
        }

        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left | e.Button == MouseButtons.Right)
            {
                int Lane = ConvertXCoordToNote(e.X);
                int Tick = (int)ConvertYCoordToTick(e.Y);

                ProcessClick(Tick, Lane, e.Button, (Notedata.NoteType)NoteTypeSelector.SelectedItem);
            }
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
            ResizeChart((int)ResizeBox.Value * 48);
            FixSwipes();
            UpdateChart();
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
             LoadChart(openFileDialog1.FileName);
        }

        private void SaveChartBtn_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                System.IO.File.WriteAllText(saveFileDialog1.FileName, Notedata.ConvertChartToJson(chart));
        }

        private void OpenMusicButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
                LoadMusic(openFileDialog2.FileName);
        }

        private void ImgSaveBtn_Click(object sender, EventArgs e)
        {
            /* float scaledivX = LaneWidth / 2; // these are the final pixel dimensions of each note in the image
            float scaledivY = TickHeight / 1;
            Bitmap img = new Bitmap((int)(LaneWidth * 8 * 8/scaledivX + 7), (int)(PanelHeight / scaledivY / 2));
            Graphics grfx = Graphics.FromImage(img);
            Bitmap tmpimg = new Bitmap(LaneWidth * 8, PanelHeight);

            ChartPanel.DrawToBitmap(tmpimg, new Rectangle(0, 0, LaneWidth * 8, PanelHeight));
            grfx.DrawImage(tmpimg, 0, -PanelHeight / scaledivY / 2, LaneWidth * 8/scaledivX, PanelHeight / scaledivY);
            grfx.DrawImage(tmpimg, LaneWidth * 8 * 1 / scaledivX + 1, 0, LaneWidth * 8 / scaledivX, PanelHeight / scaledivY);

            ChartPanel2.DrawToBitmap(tmpimg, new Rectangle(0, 0, LaneWidth * 8, PanelHeight));
            grfx.DrawImage(tmpimg, LaneWidth * 8 * 2 / scaledivX + 2, -PanelHeight / scaledivY / 2, LaneWidth * 8 / scaledivX, PanelHeight / scaledivY);
            grfx.DrawImage(tmpimg, LaneWidth * 8 * 3 / scaledivX + 3, 0, LaneWidth * 8 / scaledivX, PanelHeight / scaledivY);

            ChartPanel3.DrawToBitmap(tmpimg, new Rectangle(0, 0, LaneWidth * 8, PanelHeight));
            grfx.DrawImage(tmpimg, LaneWidth * 8 * 4 / scaledivX + 4, -PanelHeight / scaledivY / 2, LaneWidth * 8 / scaledivX, PanelHeight / scaledivY);
            grfx.DrawImage(tmpimg, LaneWidth * 8 * 5 / scaledivX + 5, 0, LaneWidth * 8 / scaledivX, PanelHeight / scaledivY);

            ChartPanel4.DrawToBitmap(tmpimg, new Rectangle(0, 0, LaneWidth * 8, PanelHeight));
            grfx.DrawImage(tmpimg, LaneWidth * 8 * 6 / scaledivX + 6, -PanelHeight / scaledivY / 2, LaneWidth * 8 / scaledivX, PanelHeight / scaledivY);
            grfx.DrawImage(tmpimg, LaneWidth * 8 * 7 / scaledivX + 7, 0, LaneWidth * 8 / scaledivX, PanelHeight / scaledivY);

            img.Save("imgout.png");

            tmpimg.Dispose();
            grfx.Dispose();
            img.Dispose(); */
        }

        private void NoteShiftBtn_Click(object sender, EventArgs e)
        {
            if (NoteShiftBox.Value > 0)
            {
                List<Notedata.Tick> NewTicks = chart.Ticks.ToList();
                NewTicks.RemoveRange(chart.Length - (int)NoteShiftBox.Value, (int)NoteShiftBox.Value);
                NewTicks.InsertRange(0, new Notedata.Tick[(int)NoteShiftBox.Value]);
                chart.Ticks = NewTicks.ToArray();

                ResizeScrollbar();
                FixSwipes();
                UpdateChart();
            }

            else if (NoteShiftBox.Value < 0)
            {
                List<Notedata.Tick> NewTicks = chart.Ticks.ToList();
                NewTicks.RemoveRange(0, - (int)NoteShiftBox.Value);
                NewTicks.AddRange(new Notedata.Tick[- (int)NoteShiftBox.Value]);
                chart.Ticks = NewTicks.ToArray();

                ResizeScrollbar();
                FixSwipes();
                UpdateChart();
            }

            NoteShiftBox.Value = 0;


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.ApplicationExitCall)
                return;

            else
            {
                if (MessageBox.Show(DialogResMgr.GetString("ExitMessage"), DialogResMgr.GetString("ExitCaption"), MessageBoxButtons.YesNo) == DialogResult.No)
                    e.Cancel = true;
            }
        }

        private void NoteCountButton_Click(object sender, EventArgs e)
        {
            int NoteCount = 0;

            for (int i = 0; i < chart.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Notedata.NoteType NoteType = FindVisualNoteType(i, j);
                    if (NoteType != Notedata.NoteType.None && NoteType != Notedata.NoteType.ExtendHoldMid &&
                        NoteType != Notedata.NoteType.SwipeLeftMid && NoteType != Notedata.NoteType.SwipeRightMid)
                        NoteCount++;
                }
            }

            MessageBox.Show(String.Format(DialogResMgr.GetString("NoteCountMessage"), NoteCount));
        }

        private void AutoSimulBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chart.Length; i++)
            {
                int SimulNum_Tap = 0;
                int SimulNum_Hold = 0;

                for (int j = 0; j < 8; j++)
                {
                    // taps get drawn as simulnotes when swipes or flicks are present, but holds don't
                    Notedata.NoteType NoteType = FindVisualNoteType(i, j);
                    if (NoteType != Notedata.NoteType.None && NoteType != Notedata.NoteType.ExtendHoldMid &&
                        NoteType != Notedata.NoteType.SwipeLeftMid && NoteType != Notedata.NoteType.SwipeRightMid)
                        SimulNum_Tap++;

                    if (NoteType == Notedata.NoteType.Tap || NoteType == Notedata.NoteType.SimulTap ||
                        NoteType == Notedata.NoteType.Hold || NoteType == Notedata.NoteType.SimulHoldStart || NoteType == Notedata.NoteType.SimulHoldRelease)
                        SimulNum_Hold++;
                }

                if (SimulNum_Tap > 1)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Notedata.NoteType NoteType = FindVisualNoteType(i, j);
                        if (NoteType == Notedata.NoteType.Tap)
                        {
                            chart.Ticks[i].Notes[j] = Notedata.NoteType.SimulTap;

                            UpdateChart();
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Notedata.NoteType NoteType = FindVisualNoteType(i, j);
                        if (NoteType == Notedata.NoteType.SimulTap)
                        {
                            chart.Ticks[i].Notes[j] = Notedata.NoteType.Tap;

                            UpdateChart();
                        }
                    }
                }

                if (SimulNum_Hold > 1)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Notedata.NoteType NoteType = FindVisualNoteType(i, j);
                        if (NoteType == Notedata.NoteType.Hold || NoteType == Notedata.NoteType.SimulHoldStart
                        || NoteType == Notedata.NoteType.SimulHoldRelease)
                        {
                            if (i + 1 < chart.Length && (chart.Ticks[i + 1].Notes[j] == Notedata.NoteType.Hold || chart.Ticks[i + 1].Notes[j] == Notedata.NoteType.SimulHoldRelease))
                                chart.Ticks[i].Notes[j] = Notedata.NoteType.SimulHoldStart;
                            else
                                chart.Ticks[i].Notes[j] = Notedata.NoteType.SimulHoldRelease;

                            UpdateChart();
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < 8; j++)
                    {
                        Notedata.NoteType NoteType = FindVisualNoteType(i, j);
                        if (NoteType == Notedata.NoteType.SimulHoldStart || NoteType == Notedata.NoteType.SimulHoldRelease)
                        {
                            chart.Ticks[i].Notes[j] = Notedata.NoteType.Hold;

                            UpdateChart();
                        }
                    }
                }
            }
        }

        private void LangChangeBtn_Click(object sender, EventArgs e)
        {
            if (System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "ja")
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en");
            else
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("ja");


            SuspendLayout();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
            resources.ApplyResources(this, "$this");
            foreach (Control Ctrl in Controls)
                resources.ApplyResources(Ctrl, Ctrl.Name);
            ResumeLayout();

            AddNoteTypes();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                char key = e.KeyChar;
                if (Char.IsDigit(key))
                {
                    NoteTypeSelector.SelectedItem = (Notedata.UserVisibleNoteType)Enum.Parse(typeof(Notedata.NoteShortcutKeys), "_" + key);
                    NoteTypeSelector.SelectedItem = (Notedata.UserVisibleNoteType_Nihongo)Enum.Parse(typeof(Notedata.NoteShortcutKeys), "_" + key);
                }
                else
                {
                    NoteTypeSelector.SelectedItem = (Notedata.UserVisibleNoteType)Enum.Parse(typeof(Notedata.NoteShortcutKeys), key.ToString().ToUpper());
                    NoteTypeSelector.SelectedItem = (Notedata.UserVisibleNoteType_Nihongo)Enum.Parse(typeof(Notedata.NoteShortcutKeys), key.ToString().ToUpper());
                }
            }
            catch { }
        }
    }
}
