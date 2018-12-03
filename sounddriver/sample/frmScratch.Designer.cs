namespace WindowsFormsApplication1
{
    partial class frmScratch
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
            this.pctWave = new System.Windows.Forms.PictureBox();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnPlay = new System.Windows.Forms.Button();
            this.tmrStreaming = new System.Windows.Forms.Timer(this.components);
            this.btnQue = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pctWave)).BeginInit();
            this.SuspendLayout();
            // 
            // pctWave
            // 
            this.pctWave.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pctWave.Location = new System.Drawing.Point(12, 47);
            this.pctWave.Name = "pctWave";
            this.pctWave.Size = new System.Drawing.Size(748, 126);
            this.pctWave.TabIndex = 15;
            this.pctWave.TabStop = false;
            this.pctWave.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pctWave_MouseDown);
            // 
            // txtFilePath
            // 
            this.txtFilePath.AllowDrop = true;
            this.txtFilePath.Location = new System.Drawing.Point(12, 22);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(283, 19);
            this.txtFilePath.TabIndex = 16;
            this.txtFilePath.Text = "c:/THROAT.WAV";
            this.txtFilePath.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtFilePath_DragDrop);
            this.txtFilePath.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtFilePath_DragEnter);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(725, 17);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(35, 24);
            this.lblInfo.TabIndex = 17;
            this.lblInfo.Text = "label1\r\na\r\n";
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(301, 18);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 23);
            this.btnPlay.TabIndex = 18;
            this.btnPlay.Text = "再生";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // tmrStreaming
            // 
            this.tmrStreaming.Tick += new System.EventHandler(this.tmrStreaming_Tick);
            // 
            // btnQue
            // 
            this.btnQue.Location = new System.Drawing.Point(382, 17);
            this.btnQue.Name = "btnQue";
            this.btnQue.Size = new System.Drawing.Size(49, 23);
            this.btnQue.TabIndex = 19;
            this.btnQue.Text = "QUE";
            this.btnQue.UseVisualStyleBackColor = true;
            this.btnQue.Click += new System.EventHandler(this.btnQue_Click);
            // 
            // frmScratch
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(772, 585);
            this.Controls.Add(this.btnQue);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.pctWave);
            this.Name = "frmScratch";
            this.Text = "frmScratch";
            this.Load += new System.EventHandler(this.frmScratch_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmScratch_FormClosing);
            this.Resize += new System.EventHandler(this.frmScratch_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pctWave)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pctWave;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Timer tmrStreaming;
        private System.Windows.Forms.Button btnQue;
    }
}