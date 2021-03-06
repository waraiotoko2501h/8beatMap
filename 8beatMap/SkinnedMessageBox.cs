﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _8beatMap
{
    public static class SkinnedMessageBox
    {
        private class SkinnedMessageBoxForm : Form
        {
            System.Resources.ResourceManager DialogResMgr = new System.Resources.ResourceManager("_8beatMap.Dialogs", System.Reflection.Assembly.GetEntryAssembly());

            private void InitComponentNew()
            {
                this.SuspendLayout();

                this.AutoSize = true;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                this.Width = 350;
                this.Height = 140;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.ShowInTaskbar = false;
                this.ShowIcon = false;
                try
                {
                    this.Icon = Application.OpenForms[0].Icon;
                }
                catch { }
                this.Shown += new EventHandler(SkinnedMessageBoxForm_Shown);
                this.FormClosed += new FormClosedEventHandler(SkinnedMessageBoxForm_FormClosed);

                this.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 8.8f);
                //this.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.FontFamily, System.Drawing.SystemFonts.MessageBoxFont.SizeInPoints);
                this.AutoScaleMode = AutoScaleMode.None;

                this.ResumeLayout(false);
                this.PerformLayout();
            }


            private readonly int btnWidth = 85;
            private readonly int btnHeight = 23;
            private readonly int btnPadding = 12;

            private Button[] buttonrefs = { null, null, null };
            private int numbuttons = 1;

            private Button AddBtn(string text, DialogResult result, int btnNumber)
            {
                Button btn = new Button { Text = text, DialogResult = result, Anchor = AnchorStyles.Bottom | AnchorStyles.Right, Width = btnWidth, Height = btnHeight };
                btn.Location = new Point(ClientSize.Width - btnWidth * (numbuttons-btnNumber) - btnPadding * (numbuttons-btnNumber), ClientSize.Height - btnHeight - btnPadding);
                Controls.Add(btn);
                buttonrefs[btnNumber] = btn;
                return btn;
            }


            public SkinnedMessageBoxForm(Skinning.Skin skin, IWin32Window owner, string message, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultbutton = MessageBoxDefaultButton.Button1, MessageBoxOptions options = 0)
            {
                InitComponentNew();

                //if (owner != null)
                //{
                //    this.Owner = owner;
                //}
                if (owner != null)
                {
                    Owner = Control.FromHandle(owner.Handle).FindForm();
                }
                else
                {
                    Owner = Form.ActiveForm;
                }

                this.Text = caption;

                Padding lblMargin = new Padding(14, 0, 14, 45);
                Point lblLocation = new Point(14, 14);
                Size lblMinSize = new Size(this.ClientSize.Width - lblLocation.X - lblMargin.Right, 0);
                Size lblMaxSize = new Size(600, 0);
                Label MessageLbl = new Label() { Text = message, AutoSize = true, Margin = lblMargin, Location = lblLocation, MinimumSize = lblMinSize, MaximumSize = lblMaxSize};
                Controls.Add(MessageLbl);

                if (buttons == MessageBoxButtons.OK)
                {
                    numbuttons = 1;
                    Button OkBtn = AddBtn(DialogResMgr.GetString("BtnOK"), DialogResult.OK, 0);
                    this.CancelButton = OkBtn;
                    //(not sure why, but this matches official .NET behaviour)
                }
                else if (buttons == MessageBoxButtons.OKCancel)
                {
                    numbuttons = 2;
                    AddBtn(DialogResMgr.GetString("BtnOK"), DialogResult.OK, 0);
                    Button CancelBtn = AddBtn(DialogResMgr.GetString("BtnCancel"), DialogResult.Cancel, 1);
                    this.CancelButton = CancelBtn;
                }
                else if (buttons == MessageBoxButtons.YesNo)
                {
                    numbuttons = 2;
                    AddBtn(DialogResMgr.GetString("BtnYes"), DialogResult.Yes, 0);
                    Button NoBtn = AddBtn(DialogResMgr.GetString("BtnNo"), DialogResult.No, 1);
                    this.CancelButton = NoBtn;
                    //(official .NET doesn't set cancel button, but this seems pretty clear to me...)
                }
                else if (buttons == MessageBoxButtons.RetryCancel)
                {
                    numbuttons = 2;
                    AddBtn(DialogResMgr.GetString("BtnRetry"), DialogResult.Retry, 0);
                    Button CancelBtn = AddBtn(DialogResMgr.GetString("BtnCancel"), DialogResult.Cancel, 1);
                    this.CancelButton = CancelBtn;
                }
                else if (buttons == MessageBoxButtons.YesNoCancel)
                {
                    numbuttons = 3;
                    AddBtn(DialogResMgr.GetString("BtnYes"), DialogResult.Yes, 0);
                    AddBtn(DialogResMgr.GetString("BtnNo"), DialogResult.No, 1);
                    Button CancelBtn = AddBtn(DialogResMgr.GetString("BtnCancel"), DialogResult.Cancel, 2);
                    this.CancelButton = CancelBtn;
                }
                else if (buttons == MessageBoxButtons.AbortRetryIgnore)
                {
                    numbuttons = 3;
                    AddBtn(DialogResMgr.GetString("BtnAbort"), DialogResult.Abort, 0);
                    AddBtn(DialogResMgr.GetString("BtnRetry"), DialogResult.Retry, 1);
                    AddBtn(DialogResMgr.GetString("BtnIgnore"), DialogResult.Ignore, 2);
                    //this.CancelButton = IgnoreBtn;
                    //(official .NET doesn't set cancel button)
                }
                else
                {
                    throw new InvalidEnumArgumentException("buttons", (int)buttons, typeof(MessageBoxButtons));
                }

                if (this.CancelButton == null)
                    this.ControlBox = false;


                if (icon != MessageBoxIcon.None)
                {
                    int iconsize = 32;
                    int iconpadding = 12;
                    int textpadding = -8;

                    PictureBox iconPB = new PictureBox() { Width = iconsize, Height = iconsize, Top = 10, Left = 10, AccessibleName = "Icon" };

                    // there are many duplicated values:
                    // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.messageboxicon?view=netframework-4.7.2
                    if (icon == MessageBoxIcon.Error) // also Hand and Stop
                    {
                        Icon erricon = new Icon(SystemIcons.Error, iconsize, iconsize);
                        iconPB.Image = erricon.ToBitmap();
                        iconPB.AccessibleDescription = "Error";
                        //this.Icon = SystemIcons.Error;
                        System.Media.SystemSounds.Hand.Play();
                    }
                    else if (icon == MessageBoxIcon.Question)
                    {
                        Icon qicon = new Icon(SystemIcons.Question, iconsize, iconsize);
                        iconPB.Image = qicon.ToBitmap();
                        iconPB.AccessibleDescription = "Question";
                        //this.Icon = SystemIcons.Question;
                        System.Media.SystemSounds.Question.Play();
                    }
                    else if (icon == MessageBoxIcon.Warning) // also Exclamation
                    {
                        Icon warnicon = new Icon(SystemIcons.Warning, iconsize, iconsize);
                        iconPB.Image = warnicon.ToBitmap();
                        iconPB.AccessibleDescription = "Warning";
                        //this.Icon = SystemIcons.Warning;
                        textpadding -= 2; // triangle shape looks worse without this
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                    else if (icon == MessageBoxIcon.Information) // also Asterisk
                    {
                        Icon infoicon = new Icon(SystemIcons.Information, iconsize, iconsize);
                        iconPB.Image = infoicon.ToBitmap();
                        iconPB.AccessibleDescription = "Information";
                        //this.Icon = SystemIcons.Information;
                        System.Media.SystemSounds.Asterisk.Play();
                    }
                    else
                    {
                        throw new InvalidEnumArgumentException("icon", (int)icon, typeof(MessageBoxIcon));
                    }

                    MessageLbl.MinimumSize = new Size(MessageLbl.MinimumSize.Width - (iconsize + iconpadding + textpadding), MessageLbl.MinimumSize.Height);
                    MessageLbl.Left += (iconsize + iconpadding + textpadding);
                    Controls.Add(iconPB);
                }

                if (defaultbutton == MessageBoxDefaultButton.Button1)
                {
                    this.AcceptButton = buttonrefs[0];
                    ActiveControl = buttonrefs[0];
                }
                else if (defaultbutton == MessageBoxDefaultButton.Button2)
                {
                    if (buttonrefs[1] == null) throw new ArgumentOutOfRangeException("defaultbutton", "defaultbutton cannot be set to a button number that does not exist");
                    this.AcceptButton = buttonrefs[1];
                    ActiveControl = buttonrefs[1];
                }
                else if (defaultbutton == MessageBoxDefaultButton.Button3)
                {
                    if (buttonrefs[2] == null) throw new ArgumentOutOfRangeException("defaultbutton", "defaultbutton cannot be set to a button number that does not exist");
                    this.AcceptButton = buttonrefs[2];
                    ActiveControl = buttonrefs[2];
                }
                else
                {
                    throw new InvalidEnumArgumentException("defaultbutton", (int)defaultbutton, typeof(MessageBoxDefaultButton));
                }

                
                // MessageBoxOptions are more like flags than concrete settings.
                // For ease of use, we assume default value is whatever winforms gives us in current locale -- but we override if any value is set
                if ((options & MessageBoxOptions.RightAlign) == MessageBoxOptions.RightAlign) MessageLbl.TextAlign = ContentAlignment.TopRight;
                else if (options != 0) MessageLbl.TextAlign = ContentAlignment.TopLeft;

                // RtlReading should be what actually affects layout, and RightAlign only affects the text label
                if ((options & MessageBoxOptions.RtlReading) == MessageBoxOptions.RtlReading)
                {
                    foreach (Control control in this.Controls)
                    {
                        control.Left = this.ClientSize.Width - control.Right;
                        control.RightToLeft = RightToLeft.Yes;
                    }
                    this.RightToLeft = RightToLeft.Yes;
                    this.RightToLeftLayout = true;
                }
                else if (options != 0)
                {
                    foreach (Control control in this.Controls)
                    {
                        control.RightToLeft = RightToLeft.No;
                    }
                    this.RightToLeft = RightToLeft.No;
                    this.RightToLeftLayout = false;
                }


                Skinning.SetBackCol(this, skin.UIColours[UIColours.UIColourDefs.Form_BG.TypeName]);
                Skinning.SetForeCol(this, skin.UIColours[UIColours.UIColourDefs.Form_Text.TypeName]);
                Skinning.SetUIStyle(this, skin.UIStyle);

                //Can use this to compare against official .NET
                //DialogResult test = MessageBox.Show("test test test test test test test test test test test test", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
                //Console.WriteLine(test);

            }

            private void SkinnedMessageBoxForm_Shown(object sender, EventArgs e)
            {
                this.Activate();
            }

            private void SkinnedMessageBoxForm_FormClosed(object sender, FormClosedEventArgs e)
            {
                if (CancelButton != null && (this.DialogResult == DialogResult.None || this.DialogResult == DialogResult.Cancel))
                    this.DialogResult = CancelButton.DialogResult;
            }
        }

        public static Skinning.Skin defaultskin = Skinning.DefaultSkin;

        // no skin, no owner
        public static DialogResult Show(string message, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultbutton = MessageBoxDefaultButton.Button1, MessageBoxOptions options = 0)
        {
            return Show(defaultskin, null, message, caption, buttons, icon, defaultbutton, options);
        }

        // no skin, has owner
        public static DialogResult Show(IWin32Window owner, string message, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultbutton = MessageBoxDefaultButton.Button1, MessageBoxOptions options = 0)
        {
            return Show(defaultskin, owner, message, caption, buttons, icon, defaultbutton, options);
        }

        // has skin, no owner
        public static DialogResult Show(Skinning.Skin skin, string message, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultbutton = MessageBoxDefaultButton.Button1, MessageBoxOptions options = 0)
        {
            return Show(skin, null, message, caption, buttons, icon, defaultbutton, options);
        }

        // has skin, has owner
        public static DialogResult Show(Skinning.Skin skin, IWin32Window owner, string message, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultbutton = MessageBoxDefaultButton.Button1, MessageBoxOptions options = 0)
        {
            // I probably shouldn't do this, but from what I can tell this should be safe enough -- out of thread owner is allowed
            // and it only seems to appear when debugging (???)
            Control.CheckForIllegalCrossThreadCalls = false;
            SkinnedMessageBoxForm mb = new SkinnedMessageBoxForm(skin, owner, message, caption, buttons, icon, defaultbutton, options);
            DialogResult res = mb.ShowDialog();
            Control.CheckForIllegalCrossThreadCalls = true;
            mb.Dispose();
            return res;
        }
    }
}
