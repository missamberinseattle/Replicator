namespace Replicator
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.RootsListBox = new System.Windows.Forms.ListBox();
            this.BtnAddRoot = new System.Windows.Forms.Button();
            this.BtnDeleteRoot = new System.Windows.Forms.Button();
            this.BtnEndSync = new System.Windows.Forms.Button();
            this.BtnStartSync = new System.Windows.Forms.Button();
            this.QueueTimer = new System.Windows.Forms.Timer(this.components);
            this.LoggerPump = new System.Windows.Forms.Timer(this.components);
            this.LoggerView = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.PbOpStatus = new System.Windows.Forms.ToolStripProgressBar();
            this.LabelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatustQueueSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.CmbLogLevel = new System.Windows.Forms.ComboBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Roots";
            // 
            // RootsListBox
            // 
            this.RootsListBox.FormattingEnabled = true;
            this.RootsListBox.Items.AddRange(
                new object[] {
                    Utility.GetProfilePath("Dropbox"),
                    Utility.GetProfilePath("OneDrive")
                });
            this.RootsListBox.Location = new System.Drawing.Point(12, 22);
            this.RootsListBox.Name = "RootsListBox";
            this.RootsListBox.Size = new System.Drawing.Size(161, 95);
            this.RootsListBox.TabIndex = 1;
            // 
            // BtnAddRoot
            // 
            this.BtnAddRoot.Location = new System.Drawing.Point(187, 24);
            this.BtnAddRoot.Name = "BtnAddRoot";
            this.BtnAddRoot.Size = new System.Drawing.Size(78, 25);
            this.BtnAddRoot.TabIndex = 2;
            this.BtnAddRoot.Text = "&Add Root...";
            this.BtnAddRoot.UseVisualStyleBackColor = true;
            // 
            // BtnDeleteRoot
            // 
            this.BtnDeleteRoot.Location = new System.Drawing.Point(187, 55);
            this.BtnDeleteRoot.Name = "BtnDeleteRoot";
            this.BtnDeleteRoot.Size = new System.Drawing.Size(78, 25);
            this.BtnDeleteRoot.TabIndex = 3;
            this.BtnDeleteRoot.Text = "&Delete Root";
            this.BtnDeleteRoot.UseVisualStyleBackColor = true;
            // 
            // BtnEndSync
            // 
            this.BtnEndSync.Enabled = false;
            this.BtnEndSync.Location = new System.Drawing.Point(311, 55);
            this.BtnEndSync.Name = "BtnEndSync";
            this.BtnEndSync.Size = new System.Drawing.Size(78, 25);
            this.BtnEndSync.TabIndex = 5;
            this.BtnEndSync.Text = "&End Sync";
            this.BtnEndSync.UseVisualStyleBackColor = true;
            this.BtnEndSync.Click += new System.EventHandler(this.BtnEndSync_Click);
            // 
            // BtnStartSync
            // 
            this.BtnStartSync.Location = new System.Drawing.Point(311, 24);
            this.BtnStartSync.Name = "BtnStartSync";
            this.BtnStartSync.Size = new System.Drawing.Size(78, 25);
            this.BtnStartSync.TabIndex = 4;
            this.BtnStartSync.Text = "&Start Sync";
            this.BtnStartSync.UseVisualStyleBackColor = true;
            this.BtnStartSync.Click += new System.EventHandler(this.BtnStartSync_Click);
            // 
            // QueueTimer
            // 
            this.QueueTimer.Interval = 15000;
            this.QueueTimer.Tick += new System.EventHandler(this.QueueTimer_Tick);
            // 
            // LoggerPump
            // 
            this.LoggerPump.Enabled = true;
            this.LoggerPump.Interval = 500;
            this.LoggerPump.Tick += new System.EventHandler(this.LoggerPump_Tick);
            // 
            // LoggerView
            // 
            this.LoggerView.Location = new System.Drawing.Point(13, 128);
            this.LoggerView.Name = "LoggerView";
            this.LoggerView.ReadOnly = true;
            this.LoggerView.Size = new System.Drawing.Size(467, 248);
            this.LoggerView.TabIndex = 8;
            this.LoggerView.Text = "";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PbOpStatus,
            this.LabelStatus,
            this.StatustQueueSize});
            this.statusStrip1.Location = new System.Drawing.Point(0, 386);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(493, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // PbOpStatus
            // 
            this.PbOpStatus.Name = "PbOpStatus";
            this.PbOpStatus.Size = new System.Drawing.Size(300, 16);
            // 
            // LabelStatus
            // 
            this.LabelStatus.Name = "LabelStatus";
            this.LabelStatus.Size = new System.Drawing.Size(163, 17);
            this.LabelStatus.Spring = true;
            // 
            // StatustQueueSize
            // 
            this.StatustQueueSize.Name = "StatustQueueSize";
            this.StatustQueueSize.Size = new System.Drawing.Size(13, 17);
            this.StatustQueueSize.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(180, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Log Level:";
            // 
            // CmbLogLevel
            // 
            this.CmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbLogLevel.FormattingEnabled = true;
            this.CmbLogLevel.Items.AddRange(new object[] {
            "Verbose",
            "Info",
            "Warning",
            "Error",
            "Off"});
            this.CmbLogLevel.Location = new System.Drawing.Point(243, 101);
            this.CmbLogLevel.Name = "CmbLogLevel";
            this.CmbLogLevel.Size = new System.Drawing.Size(121, 21);
            this.CmbLogLevel.TabIndex = 11;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(493, 408);
            this.Controls.Add(this.CmbLogLevel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.LoggerView);
            this.Controls.Add(this.BtnEndSync);
            this.Controls.Add(this.BtnStartSync);
            this.Controls.Add(this.BtnDeleteRoot);
            this.Controls.Add(this.BtnAddRoot);
            this.Controls.Add(this.RootsListBox);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Replicator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox RootsListBox;
        private System.Windows.Forms.Button BtnEndSync;
        private System.Windows.Forms.Button BtnStartSync;
        private System.Windows.Forms.Button BtnDeleteRoot;
        private System.Windows.Forms.Button BtnAddRoot;
        private System.Windows.Forms.Timer QueueTimer;
        private System.Windows.Forms.Timer LoggerPump;
        private System.Windows.Forms.RichTextBox LoggerView;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar PbOpStatus;
        private System.Windows.Forms.ToolStripStatusLabel LabelStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CmbLogLevel;
        private System.Windows.Forms.ToolStripStatusLabel StatustQueueSize;
    }
}

