namespace TidalDiscord
    {
    partial class TidalDiscordForm
        {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TidalDiscordForm));
            timer1 = new System.Windows.Forms.Timer(components);
            notifyIcon1 = new NotifyIcon(components);
            pictureBox1 = new PictureBox();
            darkLabel1 = new DarkUI.Controls.DarkLabel();
            darkLabel2 = new DarkUI.Controls.DarkLabel();
            SongLabel = new DarkUI.Controls.DarkLabel();
            lblDiscordStatus = new DarkUI.Controls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // timer1
            // 
            timer1.Tick += UpdatePresence;
            // 
            // notifyIcon1
            // 
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            notifyIcon1.Click += IconClick;
            // 
            // pictureBox1
            // 
            pictureBox1.AccessibleRole = AccessibleRole.None;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(6, 9);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(91, 87);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // darkLabel1
            // 
            darkLabel1.AutoSize = true;
            darkLabel1.Font = new Font("Segoe UI", 20F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            darkLabel1.ForeColor = Color.FromArgb(220, 220, 220);
            darkLabel1.ImageAlign = ContentAlignment.TopLeft;
            darkLabel1.Location = new Point(98, 0);
            darkLabel1.Name = "darkLabel1";
            darkLabel1.Size = new Size(415, 37);
            darkLabel1.TabIndex = 1;
            darkLabel1.Text = "TIDAL Rich Presence on Discord";
            // 
            // darkLabel2
            // 
            darkLabel2.Font = new Font("Segoe UI", 8F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            darkLabel2.ForeColor = Color.Silver;
            darkLabel2.Location = new Point(12, 105);
            darkLabel2.Name = "darkLabel2";
            darkLabel2.Size = new Size(500, 15);
            darkLabel2.TabIndex = 4;
            darkLabel2.Text = "Developed 2024 by @ricardag @CamelCaseName";
            darkLabel2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SongLabel
            // 
            SongLabel.AutoEllipsis = true;
            SongLabel.ForeColor = Color.FromArgb(220, 220, 220);
            SongLabel.Location = new Point(110, 55);
            SongLabel.Name = "SongLabel";
            SongLabel.Size = new Size(402, 15);
            SongLabel.TabIndex = 2;
            SongLabel.Text = "nothing";
            // 
            // lblDiscordStatus
            // 
            lblDiscordStatus.AutoEllipsis = true;
            lblDiscordStatus.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point);
            lblDiscordStatus.ForeColor = Color.FromArgb(220, 220, 220);
            lblDiscordStatus.Location = new Point(110, 76);
            lblDiscordStatus.Name = "lblDiscordStatus";
            lblDiscordStatus.Size = new Size(402, 15);
            lblDiscordStatus.TabIndex = 5;
            lblDiscordStatus.Text = "Discord status";
            // 
            // TidalDiscordForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(525, 133);
            Controls.Add(lblDiscordStatus);
            Controls.Add(darkLabel2);
            Controls.Add(SongLabel);
            Controls.Add(darkLabel1);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "TidalDiscordForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TIDAL Rich Presence on Discord";
            FormClosing += Form1_Closing;
            Load += Form1_Load;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private NotifyIcon notifyIcon1;
        private PictureBox pictureBox1;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkLabel SongLabel;
        private DarkUI.Controls.DarkLabel lblDiscordStatus;
        }
    }
