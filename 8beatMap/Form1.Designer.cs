﻿namespace _8beatMap
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ChartScrollBar = new System.Windows.Forms.VScrollBar();
            this.BPMbox = new System.Windows.Forms.NumericUpDown();
            this.BPMLbl = new System.Windows.Forms.Label();
            this.PlayBtn = new System.Windows.Forms.Button();
            this.StopBtn = new System.Windows.Forms.Button();
            this.newplayhead = new System.Windows.Forms.PictureBox();
            this.PauseOnSeek = new System.Windows.Forms.CheckBox();
            this.ZoomBox = new System.Windows.Forms.NumericUpDown();
            this.ZoomLbl = new System.Windows.Forms.Label();
            this.ResizeLbl = new System.Windows.Forms.Label();
            this.ResizeBox = new System.Windows.Forms.NumericUpDown();
            this.ResizeBtn = new System.Windows.Forms.Button();
            this.OpenBtn = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.OpenMusicButton = new System.Windows.Forms.Button();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.SaveChartBtn = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.ImgSaveBtn = new System.Windows.Forms.Button();
            this.NoteTypeSelector = new System.Windows.Forms.ComboBox();
            this.NoteTypeLbl = new System.Windows.Forms.Label();
            this.NoteShiftLbl = new System.Windows.Forms.Label();
            this.NoteShiftBox = new System.Windows.Forms.NumericUpDown();
            this.NoteShiftBtn = new System.Windows.Forms.Button();
            this.NoteSoundBox = new System.Windows.Forms.CheckBox();
            this.NoteCountButton = new System.Windows.Forms.Button();
            this.AutoSimulBtn = new System.Windows.Forms.Button();
            this.LangChangeBtn = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.PreviewWndBtn = new System.Windows.Forms.Button();
            this.CopyLengthLabel = new System.Windows.Forms.Label();
            this.CopyLengthBox = new System.Windows.Forms.NumericUpDown();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.editorToolsTab = new System.Windows.Forms.TabPage();
            this.editorSettingsTab = new System.Windows.Forms.TabPage();
            this.chartSettingsTab = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.BPMbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.newplayhead)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ZoomBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResizeBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteShiftBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CopyLengthBox)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.editorToolsTab.SuspendLayout();
            this.editorSettingsTab.SuspendLayout();
            this.chartSettingsTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ChartScrollBar
            // 
            resources.ApplyResources(this.ChartScrollBar, "ChartScrollBar");
            this.ChartScrollBar.LargeChange = 100;
            this.ChartScrollBar.Name = "ChartScrollBar";
            this.ChartScrollBar.SmallChange = 10;
            this.ChartScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ChartScrollBar_Scroll);
            // 
            // BPMbox
            // 
            this.BPMbox.DecimalPlaces = 1;
            resources.ApplyResources(this.BPMbox, "BPMbox");
            this.BPMbox.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.BPMbox.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.BPMbox.Name = "BPMbox";
            this.BPMbox.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.BPMbox.ValueChanged += new System.EventHandler(this.BPMbox_ValueChanged);
            // 
            // BPMLbl
            // 
            resources.ApplyResources(this.BPMLbl, "BPMLbl");
            this.BPMLbl.Name = "BPMLbl";
            // 
            // PlayBtn
            // 
            resources.ApplyResources(this.PlayBtn, "PlayBtn");
            this.PlayBtn.Name = "PlayBtn";
            this.PlayBtn.UseVisualStyleBackColor = true;
            this.PlayBtn.Click += new System.EventHandler(this.PlayBtn_Click);
            // 
            // StopBtn
            // 
            resources.ApplyResources(this.StopBtn, "StopBtn");
            this.StopBtn.Name = "StopBtn";
            this.StopBtn.UseVisualStyleBackColor = true;
            this.StopBtn.Click += new System.EventHandler(this.StopBtn_Click);
            // 
            // newplayhead
            // 
            resources.ApplyResources(this.newplayhead, "newplayhead");
            this.newplayhead.BackColor = System.Drawing.Color.DarkSlateGray;
            this.newplayhead.Name = "newplayhead";
            this.newplayhead.TabStop = false;
            // 
            // PauseOnSeek
            // 
            resources.ApplyResources(this.PauseOnSeek, "PauseOnSeek");
            this.PauseOnSeek.Name = "PauseOnSeek";
            this.PauseOnSeek.UseVisualStyleBackColor = true;
            // 
            // ZoomBox
            // 
            resources.ApplyResources(this.ZoomBox, "ZoomBox");
            this.ZoomBox.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.ZoomBox.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.ZoomBox.Name = "ZoomBox";
            this.ZoomBox.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.ZoomBox.ValueChanged += new System.EventHandler(this.ZoomBox_ValueChanged);
            // 
            // ZoomLbl
            // 
            resources.ApplyResources(this.ZoomLbl, "ZoomLbl");
            this.ZoomLbl.Name = "ZoomLbl";
            // 
            // ResizeLbl
            // 
            resources.ApplyResources(this.ResizeLbl, "ResizeLbl");
            this.ResizeLbl.Name = "ResizeLbl";
            // 
            // ResizeBox
            // 
            resources.ApplyResources(this.ResizeBox, "ResizeBox");
            this.ResizeBox.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.ResizeBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ResizeBox.Name = "ResizeBox";
            this.ResizeBox.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // ResizeBtn
            // 
            resources.ApplyResources(this.ResizeBtn, "ResizeBtn");
            this.ResizeBtn.Name = "ResizeBtn";
            this.ResizeBtn.UseVisualStyleBackColor = true;
            this.ResizeBtn.Click += new System.EventHandler(this.ResizeBtn_Click);
            // 
            // OpenBtn
            // 
            resources.ApplyResources(this.OpenBtn, "OpenBtn");
            this.OpenBtn.Name = "OpenBtn";
            this.OpenBtn.UseVisualStyleBackColor = true;
            this.OpenBtn.Click += new System.EventHandler(this.OpenBtn_Click);
            // 
            // openFileDialog1
            // 
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // OpenMusicButton
            // 
            resources.ApplyResources(this.OpenMusicButton, "OpenMusicButton");
            this.OpenMusicButton.Name = "OpenMusicButton";
            this.OpenMusicButton.UseVisualStyleBackColor = true;
            this.OpenMusicButton.Click += new System.EventHandler(this.OpenMusicButton_Click);
            // 
            // openFileDialog2
            // 
            resources.ApplyResources(this.openFileDialog2, "openFileDialog2");
            // 
            // SaveChartBtn
            // 
            resources.ApplyResources(this.SaveChartBtn, "SaveChartBtn");
            this.SaveChartBtn.Name = "SaveChartBtn";
            this.SaveChartBtn.UseVisualStyleBackColor = true;
            this.SaveChartBtn.Click += new System.EventHandler(this.SaveChartBtn_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "*.dec.json";
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            // 
            // ImgSaveBtn
            // 
            resources.ApplyResources(this.ImgSaveBtn, "ImgSaveBtn");
            this.ImgSaveBtn.Name = "ImgSaveBtn";
            this.ImgSaveBtn.UseVisualStyleBackColor = true;
            this.ImgSaveBtn.Click += new System.EventHandler(this.ImgSaveBtn_Click);
            // 
            // NoteTypeSelector
            // 
            this.NoteTypeSelector.DisplayMember = "Key";
            this.NoteTypeSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NoteTypeSelector.FormattingEnabled = true;
            resources.ApplyResources(this.NoteTypeSelector, "NoteTypeSelector");
            this.NoteTypeSelector.Name = "NoteTypeSelector";
            this.NoteTypeSelector.ValueMember = "Value";
            // 
            // NoteTypeLbl
            // 
            resources.ApplyResources(this.NoteTypeLbl, "NoteTypeLbl");
            this.NoteTypeLbl.Name = "NoteTypeLbl";
            // 
            // NoteShiftLbl
            // 
            resources.ApplyResources(this.NoteShiftLbl, "NoteShiftLbl");
            this.NoteShiftLbl.Name = "NoteShiftLbl";
            // 
            // NoteShiftBox
            // 
            resources.ApplyResources(this.NoteShiftBox, "NoteShiftBox");
            this.NoteShiftBox.Maximum = new decimal(new int[] {
            48,
            0,
            0,
            0});
            this.NoteShiftBox.Minimum = new decimal(new int[] {
            48,
            0,
            0,
            -2147483648});
            this.NoteShiftBox.Name = "NoteShiftBox";
            // 
            // NoteShiftBtn
            // 
            resources.ApplyResources(this.NoteShiftBtn, "NoteShiftBtn");
            this.NoteShiftBtn.Name = "NoteShiftBtn";
            this.NoteShiftBtn.UseVisualStyleBackColor = true;
            this.NoteShiftBtn.Click += new System.EventHandler(this.NoteShiftBtn_Click);
            // 
            // NoteSoundBox
            // 
            resources.ApplyResources(this.NoteSoundBox, "NoteSoundBox");
            this.NoteSoundBox.Checked = true;
            this.NoteSoundBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.NoteSoundBox.Name = "NoteSoundBox";
            this.NoteSoundBox.UseVisualStyleBackColor = true;
            // 
            // NoteCountButton
            // 
            resources.ApplyResources(this.NoteCountButton, "NoteCountButton");
            this.NoteCountButton.Name = "NoteCountButton";
            this.NoteCountButton.UseVisualStyleBackColor = true;
            this.NoteCountButton.Click += new System.EventHandler(this.NoteCountButton_Click);
            // 
            // AutoSimulBtn
            // 
            resources.ApplyResources(this.AutoSimulBtn, "AutoSimulBtn");
            this.AutoSimulBtn.Name = "AutoSimulBtn";
            this.AutoSimulBtn.UseVisualStyleBackColor = true;
            this.AutoSimulBtn.Click += new System.EventHandler(this.AutoSimulBtn_Click);
            // 
            // LangChangeBtn
            // 
            resources.ApplyResources(this.LangChangeBtn, "LangChangeBtn");
            this.LangChangeBtn.Name = "LangChangeBtn";
            this.LangChangeBtn.UseVisualStyleBackColor = true;
            this.LangChangeBtn.Click += new System.EventHandler(this.LangChangeBtn_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Chart_Click);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Chart_MouseMove);
            this.pictureBox1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Chart_MouseWheel);
            // 
            // PreviewWndBtn
            // 
            resources.ApplyResources(this.PreviewWndBtn, "PreviewWndBtn");
            this.PreviewWndBtn.Name = "PreviewWndBtn";
            this.PreviewWndBtn.UseVisualStyleBackColor = true;
            this.PreviewWndBtn.Click += new System.EventHandler(this.PreviewWndBtn_Click);
            // 
            // CopyLengthLabel
            // 
            resources.ApplyResources(this.CopyLengthLabel, "CopyLengthLabel");
            this.CopyLengthLabel.Name = "CopyLengthLabel";
            // 
            // CopyLengthBox
            // 
            this.CopyLengthBox.DecimalPlaces = 2;
            this.CopyLengthBox.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            resources.ApplyResources(this.CopyLengthBox, "CopyLengthBox");
            this.CopyLengthBox.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.CopyLengthBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.CopyLengthBox.Name = "CopyLengthBox";
            this.CopyLengthBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.editorToolsTab);
            this.tabControl1.Controls.Add(this.editorSettingsTab);
            this.tabControl1.Controls.Add(this.chartSettingsTab);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // editorToolsTab
            // 
            this.editorToolsTab.Controls.Add(this.NoteTypeLbl);
            this.editorToolsTab.Controls.Add(this.PreviewWndBtn);
            this.editorToolsTab.Controls.Add(this.CopyLengthBox);
            this.editorToolsTab.Controls.Add(this.NoteTypeSelector);
            this.editorToolsTab.Controls.Add(this.CopyLengthLabel);
            this.editorToolsTab.Controls.Add(this.NoteCountButton);
            this.editorToolsTab.Controls.Add(this.AutoSimulBtn);
            this.editorToolsTab.Controls.Add(this.NoteShiftBtn);
            this.editorToolsTab.Controls.Add(this.NoteShiftBox);
            this.editorToolsTab.Controls.Add(this.NoteShiftLbl);
            resources.ApplyResources(this.editorToolsTab, "editorToolsTab");
            this.editorToolsTab.Name = "editorToolsTab";
            // 
            // editorSettingsTab
            // 
            this.editorSettingsTab.Controls.Add(this.NoteSoundBox);
            this.editorSettingsTab.Controls.Add(this.PauseOnSeek);
            this.editorSettingsTab.Controls.Add(this.ZoomLbl);
            this.editorSettingsTab.Controls.Add(this.ZoomBox);
            resources.ApplyResources(this.editorSettingsTab, "editorSettingsTab");
            this.editorSettingsTab.Name = "editorSettingsTab";
            // 
            // chartSettingsTab
            // 
            this.chartSettingsTab.Controls.Add(this.BPMLbl);
            this.chartSettingsTab.Controls.Add(this.BPMbox);
            this.chartSettingsTab.Controls.Add(this.ResizeLbl);
            this.chartSettingsTab.Controls.Add(this.ResizeBox);
            this.chartSettingsTab.Controls.Add(this.ResizeBtn);
            resources.ApplyResources(this.chartSettingsTab, "chartSettingsTab");
            this.chartSettingsTab.Name = "chartSettingsTab";
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.OpenMusicButton);
            this.splitContainer1.Panel2.Controls.Add(this.PlayBtn);
            this.splitContainer1.Panel2.Controls.Add(this.LangChangeBtn);
            this.splitContainer1.Panel2.Controls.Add(this.StopBtn);
            this.splitContainer1.Panel2.Controls.Add(this.ImgSaveBtn);
            this.splitContainer1.Panel2.Controls.Add(this.OpenBtn);
            this.splitContainer1.Panel2.Controls.Add(this.SaveChartBtn);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.FontFamily, 8.8f);
            //this.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.FontFamily, System.Drawing.SystemFonts.MessageBoxFont.SizeInPoints);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.newplayhead);
            this.Controls.Add(this.ChartScrollBar);
            this.Controls.Add(this.pictureBox1);
            this.KeyPreview = true;
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.BPMbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.newplayhead)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ZoomBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResizeBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteShiftBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CopyLengthBox)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.editorToolsTab.ResumeLayout(false);
            this.editorToolsTab.PerformLayout();
            this.editorSettingsTab.ResumeLayout(false);
            this.editorSettingsTab.PerformLayout();
            this.chartSettingsTab.ResumeLayout(false);
            this.chartSettingsTab.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        
        private System.Windows.Forms.VScrollBar ChartScrollBar;
        private System.Windows.Forms.NumericUpDown BPMbox;
        private System.Windows.Forms.Label BPMLbl;
        private System.Windows.Forms.Button PlayBtn;
        private System.Windows.Forms.Button StopBtn;
        private System.Windows.Forms.PictureBox newplayhead;
        private System.Windows.Forms.CheckBox PauseOnSeek;
        private System.Windows.Forms.NumericUpDown ZoomBox;
        private System.Windows.Forms.Label ZoomLbl;
        private System.Windows.Forms.Label ResizeLbl;
        private System.Windows.Forms.NumericUpDown ResizeBox;
        private System.Windows.Forms.Button ResizeBtn;
        private System.Windows.Forms.Button OpenBtn;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button OpenMusicButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.Button SaveChartBtn;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button ImgSaveBtn;
        private System.Windows.Forms.ComboBox NoteTypeSelector;
        private System.Windows.Forms.Label NoteTypeLbl;
        private System.Windows.Forms.Label NoteShiftLbl;
        private System.Windows.Forms.NumericUpDown NoteShiftBox;
        private System.Windows.Forms.Button NoteShiftBtn;
        private System.Windows.Forms.CheckBox NoteSoundBox;
        private System.Windows.Forms.Button NoteCountButton;
        private System.Windows.Forms.Button AutoSimulBtn;
        private System.Windows.Forms.Button LangChangeBtn;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button PreviewWndBtn;
        private System.Windows.Forms.Label CopyLengthLabel;
        private System.Windows.Forms.NumericUpDown CopyLengthBox;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage editorToolsTab;
        private System.Windows.Forms.TabPage editorSettingsTab;
        private System.Windows.Forms.TabPage chartSettingsTab;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}

