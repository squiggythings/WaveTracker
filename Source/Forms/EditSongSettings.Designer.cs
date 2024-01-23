namespace WaveTracker.Forms
{
    partial class EditSongSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any res being used.
        /// </summary>
        /// <param name="disposing">true if managed res should be disposed; otherwise, false.</param>
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
            this.title = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.author = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.copyright = new System.Windows.Forms.TextBox();
            this.frameLength = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.songSpeed = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.hertzLabel = new System.Windows.Forms.Label();
            this.samplesLabel = new System.Windows.Forms.Label();
            this.Reset = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.tickRate = new System.Windows.Forms.TrackBar();
            this.quantizeChannelAmplitude = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.frameLength)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tickRate)).BeginInit();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.title.Location = new System.Drawing.Point(88, 22);
            this.title.MaxLength = 64;
            this.title.Name = "title";
            this.title.PlaceholderText = "(title)";
            this.title.Size = new System.Drawing.Size(266, 23);
            this.title.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Title";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Location = new System.Drawing.Point(6, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Author";
            // 
            // author
            // 
            this.author.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.author.Location = new System.Drawing.Point(88, 51);
            this.author.MaxLength = 64;
            this.author.Name = "author";
            this.author.PlaceholderText = "(author)";
            this.author.Size = new System.Drawing.Size(266, 23);
            this.author.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label3.Location = new System.Drawing.Point(6, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Copyright";
            // 
            // copyright
            // 
            this.copyright.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.copyright.Location = new System.Drawing.Point(88, 80);
            this.copyright.MaxLength = 64;
            this.copyright.Name = "copyright";
            this.copyright.PlaceholderText = "(copyright)";
            this.copyright.Size = new System.Drawing.Size(266, 23);
            this.copyright.TabIndex = 2;
            // 
            // frameLength
            // 
            this.frameLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.frameLength.Location = new System.Drawing.Point(147, 165);
            this.frameLength.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.frameLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.frameLength.Name = "frameLength";
            this.frameLength.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.frameLength.Size = new System.Drawing.Size(89, 23);
            this.frameLength.TabIndex = 4;
            this.frameLength.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.copyright);
            this.groupBox1.Controls.Add(this.author);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.title);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 115);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Song Information";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label4.Location = new System.Drawing.Point(12, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Frame Length (rows)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label5.Location = new System.Drawing.Point(12, 138);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(129, 15);
            this.label5.TabIndex = 3;
            this.label5.Text = "Song Speed (ticks/row)";
            // 
            // songSpeed
            // 
            this.songSpeed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.songSpeed.Location = new System.Drawing.Point(147, 136);
            this.songSpeed.MaxLength = 64;
            this.songSpeed.Name = "songSpeed";
            this.songSpeed.PlaceholderText = "ex: \"3\" \"3 4\" ";
            this.songSpeed.Size = new System.Drawing.Size(89, 23);
            this.songSpeed.TabIndex = 3;
            this.songSpeed.TextChanged += new System.EventHandler(this.songSpeed_TextChanged);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(324, 249);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(243, 249);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(387, 231);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.frameLength);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.songSpeed);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(379, 203);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.hertzLabel);
            this.tabPage2.Controls.Add(this.samplesLabel);
            this.tabPage2.Controls.Add(this.Reset);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.tickRate);
            this.tabPage2.Controls.Add(this.quantizeChannelAmplitude);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(379, 203);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Advanced";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // hertzLabel
            // 
            this.hertzLabel.AutoSize = true;
            this.hertzLabel.Location = new System.Drawing.Point(168, 16);
            this.hertzLabel.Name = "hertzLabel";
            this.hertzLabel.Size = new System.Drawing.Size(36, 15);
            this.hertzLabel.TabIndex = 6;
            this.hertzLabel.Text = "60 Hz";
            this.hertzLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // samplesLabel
            // 
            this.samplesLabel.AutoSize = true;
            this.samplesLabel.Location = new System.Drawing.Point(132, 82);
            this.samplesLabel.Name = "samplesLabel";
            this.samplesLabel.Size = new System.Drawing.Size(103, 15);
            this.samplesLabel.TabIndex = 5;
            this.samplesLabel.Text = "(531 samples/tick)";
            this.samplesLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.samplesLabel.Click += new System.EventHandler(this.samplesLabel_Click);
            // 
            // Reset
            // 
            this.Reset.Location = new System.Drawing.Point(286, 12);
            this.Reset.Name = "Reset";
            this.Reset.Size = new System.Drawing.Size(87, 23);
            this.Reset.TabIndex = 4;
            this.Reset.Text = "Default";
            this.Reset.UseVisualStyleBackColor = true;
            this.Reset.Click += new System.EventHandler(this.Reset_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 15);
            this.label7.TabIndex = 3;
            this.label7.Text = "Tick Rate:";
            // 
            // tickRate
            // 
            this.tickRate.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tickRate.Location = new System.Drawing.Point(6, 34);
            this.tickRate.Maximum = 240;
            this.tickRate.Minimum = 2;
            this.tickRate.Name = "tickRate";
            this.tickRate.Size = new System.Drawing.Size(367, 45);
            this.tickRate.TabIndex = 1;
            this.tickRate.TickFrequency = 10;
            this.tickRate.Value = 60;
            this.tickRate.Scroll += new System.EventHandler(this.tickRate_Scroll);
            this.tickRate.ValueChanged += new System.EventHandler(this.tickRate_ValueChanged);
            // 
            // quantizeChannelAmplitude
            // 
            this.quantizeChannelAmplitude.AutoSize = true;
            this.quantizeChannelAmplitude.Location = new System.Drawing.Point(8, 128);
            this.quantizeChannelAmplitude.Name = "quantizeChannelAmplitude";
            this.quantizeChannelAmplitude.Size = new System.Drawing.Size(184, 19);
            this.quantizeChannelAmplitude.TabIndex = 0;
            this.quantizeChannelAmplitude.Text = "Quantize Channel Amplitudes";
            this.quantizeChannelAmplitude.UseVisualStyleBackColor = true;
            this.quantizeChannelAmplitude.CheckedChanged += new System.EventHandler(this.quantizeChannelAmplitude_CheckedChanged);
            // 
            // EditSongSettings
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(411, 278);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditSongSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Song Settings";
            ((System.ComponentModel.ISupportInitialize)(this.frameLength)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tickRate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TextBox title;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox author;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox copyright;
        public System.Windows.Forms.NumericUpDown frameLength;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox songSpeed;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button Reset;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.TrackBar tickRate;
        public System.Windows.Forms.CheckBox quantizeChannelAmplitude;
        private System.Windows.Forms.Label hertzLabel;
        private System.Windows.Forms.Label samplesLabel;
    }
}