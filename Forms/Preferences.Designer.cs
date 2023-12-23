namespace WaveTracker.Forms
{
     partial class Preferences
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.General = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.oscilloscopeDropdown = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.GeneralSettings = new System.Windows.Forms.CheckedListBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.VisualizationMode = new System.Windows.Forms.TabPage();
            this.General.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(397, 329);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(316, 329);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 6;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // General
            // 
            this.General.Controls.Add(this.groupBox3);
            this.General.Controls.Add(this.groupBox2);
            this.General.Controls.Add(this.groupBox1);
            this.General.Location = new System.Drawing.Point(4, 24);
            this.General.Name = "General";
            this.General.Padding = new System.Windows.Forms.Padding(3);
            this.General.Size = new System.Drawing.Size(452, 283);
            this.General.TabIndex = 0;
            this.General.Text = "General";
            this.General.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.trackBar2);
            this.groupBox3.Location = new System.Drawing.Point(269, 70);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(177, 83);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Volume";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(65, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "40 ms";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // trackBar2
            // 
            this.trackBar2.BackColor = System.Drawing.Color.White;
            this.trackBar2.Location = new System.Drawing.Point(6, 22);
            this.trackBar2.Maximum = 100;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(165, 45);
            this.trackBar2.TabIndex = 7;
            this.trackBar2.TickFrequency = 10;
            this.trackBar2.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar2.Value = 1;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.oscilloscopeDropdown);
            this.groupBox2.Location = new System.Drawing.Point(269, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(177, 58);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Oscilloscope";
            // 
            // oscilloscopeDropdown
            // 
            this.oscilloscopeDropdown.FormattingEnabled = true;
            this.oscilloscopeDropdown.Items.AddRange(new object[] {
            "Mono",
            "Stereo split",
            "Stereo overlap"});
            this.oscilloscopeDropdown.Location = new System.Drawing.Point(6, 22);
            this.oscilloscopeDropdown.Name = "oscilloscopeDropdown";
            this.oscilloscopeDropdown.Size = new System.Drawing.Size(165, 23);
            this.oscilloscopeDropdown.TabIndex = 0;
            this.oscilloscopeDropdown.SelectedIndexChanged += new System.EventHandler(this.oscilloscopeDropdown_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DescriptionLabel);
            this.groupBox1.Controls.Add(this.GeneralSettings);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(257, 270);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General Settings";
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.BackColor = System.Drawing.SystemColors.Window;
            this.DescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.DescriptionLabel.Location = new System.Drawing.Point(6, 192);
            this.DescriptionLabel.MaximumSize = new System.Drawing.Size(342, 200);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(242, 73);
            this.DescriptionLabel.TabIndex = 3;
            this.DescriptionLabel.Text = "Description:  today we will be going to the movies";
            this.DescriptionLabel.Click += new System.EventHandler(this.DescriptionLabel_Click);
            // 
            // GeneralSettings
            // 
            this.GeneralSettings.FormattingEnabled = true;
            this.GeneralSettings.Items.AddRange(new object[] {
            "Select channel on double click",
            "Fade volume column",
            "Ignore step when moving",
            "Key repeat",
            "Restore channel state on playback",
            "Show note-off and note-release as text",
            "Show row numbers in hex",
            "Wrap cursor",
            "Wrap cursor across frames",
            "Reflect instrument index changes",
            "Automatically normalize samples",
            "Trim silence on imported samples"});
            this.GeneralSettings.Location = new System.Drawing.Point(6, 23);
            this.GeneralSettings.Name = "GeneralSettings";
            this.GeneralSettings.Size = new System.Drawing.Size(243, 166);
            this.GeneralSettings.TabIndex = 1;
            this.GeneralSettings.ThreeDCheckBoxes = true;
            this.GeneralSettings.SelectedIndexChanged += new System.EventHandler(this.GeneralSettings_SelectedIndexChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.General);
            this.tabControl1.Controls.Add(this.VisualizationMode);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(460, 311);
            this.tabControl1.TabIndex = 3;
            // 
            // VisualizationMode
            // 
            this.VisualizationMode.Location = new System.Drawing.Point(4, 24);
            this.VisualizationMode.Name = "VisualizationMode";
            this.VisualizationMode.Padding = new System.Windows.Forms.Padding(3);
            this.VisualizationMode.Size = new System.Drawing.Size(452, 283);
            this.VisualizationMode.TabIndex = 3;
            this.VisualizationMode.Text = "Visualization Mode";
            this.VisualizationMode.UseVisualStyleBackColor = true;
            // 
            // Preferences
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(484, 360);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Preferences";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Preferences";
            this.General.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabPage General;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.ComboBox oscilloscopeDropdown;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Label DescriptionLabel;
        public System.Windows.Forms.CheckedListBox GeneralSettings;
        private System.Windows.Forms.TabControl tabControl1;
        public System.Windows.Forms.Button buttonCancel;
        public System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TabPage VisualizationMode;
        public System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Label label2;
    }
}