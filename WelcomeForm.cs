using InventorySystem;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InventorySystem
{
    public partial class WelcomeForm : Form
    {
        public event EventHandler<string>? RoleSelected;

        private readonly Color PrimaryColor = Color.FromArgb(0, 77, 102);
        private readonly Color SecondaryColor = Color.FromArgb(0, 169, 224);
        private readonly Color AccentColor = Color.FromArgb(255, 183, 0);
        private readonly Color LightText = Color.FromArgb(249, 250, 251);

        private bool allowSafeClose = false;

        private LinearGradientBrush? backgroundGradient;
        private System.Windows.Forms.Timer? fadeTimer;
        private double fadeOpacity = 0;

        private System.Windows.Forms.Timer? closeTimer;
        private double closeOpacity = 1;

        public WelcomeForm()
        {
            InitializeComponent();
            InitializeUI();
            SetupFadeInAnimation();
        }

        private void InitializeUI()
        {
            this.Text = "Inventory Management System";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = PrimaryColor;
            this.DoubleBuffered = true;

            this.Paint += (s, e) =>
            {
                if (backgroundGradient != null)
                    e.Graphics.FillRectangle(backgroundGradient, this.ClientRectangle);
            };

            this.Resize += (s, e) =>
            {
                RecalculateLayout();
                this.Invalidate();
            };

            var mainPanel = new Panel
            {
                Name = "mainPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            this.Controls.Add(mainPanel);

            var logo = new PictureBox
            {
                Name = "logoBox",
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            try
            {
                logo.Image = Image.FromFile("logo.png");
            }
            catch
            {
                Bitmap bmp = new Bitmap(120, 120);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(new SolidBrush(SecondaryColor), 10, 10, 100, 100);
                    g.FillRectangle(Brushes.White, 35, 45, 50, 25);
                    g.FillRectangle(Brushes.White, 45, 25, 30, 15);
                }
                logo.Image = bmp;
            }
            mainPanel.Controls.Add(logo);

            var title = new Label
            {
                Name = "titleLabel",
                Text = "INVENTORY MANAGEMENT",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = LightText,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(title);

            var subtitle = new Label
            {
                Name = "subtitleLabel",
                Text = "Comprehensive Stock Control Solution",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(subtitle);

            var version = new Label
            {
                Name = "versionLabel",
                Text = "v2.5.1",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(version);

            var buttonPanel = new Panel
            {
                Name = "buttonPanel",
                Size = new Size(300, 180),
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(buttonPanel);

            var ownerBtn = CreateModernButton("OWNER PORTAL", SecondaryColor);
            ownerBtn.Click += OnOwnerButtonClick;
            buttonPanel.Controls.Add(ownerBtn);

            var employeeBtn = CreateModernButton("SALESPERSON PORTAL", AccentColor);
            employeeBtn.Top = ownerBtn.Bottom + 20;
            employeeBtn.Click += OnSalespersonButtonClick;
            buttonPanel.Controls.Add(employeeBtn);

            var exitBtn = new Button
            {
                Name = "exitButton",
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                ForeColor = LightText,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12),
                Size = new Size(40, 40),
                Cursor = Cursors.Hand
            };
            exitBtn.FlatAppearance.BorderSize = 0;
            exitBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
            exitBtn.Click += (s, e) => Application.Exit();
            mainPanel.Controls.Add(exitBtn);

            var footer = new Label
            {
                Name = "footerLabel",
                Text = "© 2023 Inventory Solutions. All rights reserved.",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            mainPanel.Controls.Add(footer);

            RecalculateLayout();
        }

        private void OnOwnerButtonClick(object sender, EventArgs e)
        {
            allowSafeClose = true;
            RoleSelected?.Invoke(this, "admin");
        }

        private void OnSalespersonButtonClick(object sender, EventArgs e)
        {
            allowSafeClose = true;
            RoleSelected?.Invoke(this, "employee");
        }

        private Button CreateModernButton(string text, Color bgColor)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(280, 50),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = bgColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 0, 0, 0);
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 25, 25));
            return btn;
        }

        private void RecalculateLayout()
        {
            var mainPanel = this.Controls["mainPanel"];
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            var logo = mainPanel.Controls["logoBox"] as PictureBox;
            logo.Location = new Point(centerX - logo.Width / 2, centerY - 200);

            var title = mainPanel.Controls["titleLabel"] as Label;
            title.Location = new Point(centerX - title.Width / 2, logo.Bottom + 20);

            var subtitle = mainPanel.Controls["subtitleLabel"] as Label;
            subtitle.Location = new Point(centerX - subtitle.Width / 2, title.Bottom + 10);

            var version = mainPanel.Controls["versionLabel"] as Label;
            version.Location = new Point(centerX - version.Width / 2, subtitle.Bottom + 5);

            var buttonPanel = mainPanel.Controls["buttonPanel"] as Panel;
            buttonPanel.Location = new Point(centerX - buttonPanel.Width / 2, centerY + 20);

            var exitBtn = mainPanel.Controls["exitButton"] as Button;
            exitBtn.Location = new Point(this.ClientSize.Width - exitBtn.Width - 20, 20);

            var footer = mainPanel.Controls["footerLabel"] as Label;
            footer.Location = new Point(centerX - footer.Width / 2, this.ClientSize.Height - 40);

            backgroundGradient = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(0, 50, 70),
                PrimaryColor,
                120f);
        }

        private void SetupFadeInAnimation()
        {
            this.Opacity = 0;
            fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 20;
            fadeTimer.Tick += (s, e) =>
            {
                fadeOpacity += 0.05;
                this.Opacity = fadeOpacity;
                if (fadeOpacity >= 1)
                {
                    fadeTimer.Stop();
                    fadeTimer.Dispose();
                }
            };
            fadeTimer.Start();
        }

        public void CloseSafely()
        {
            allowSafeClose = true;
            // Use fade out animation before closing
            if (closeTimer != null && closeTimer.Enabled)
                return; // Already closing

            closeOpacity = this.Opacity;

            closeTimer = new System.Windows.Forms.Timer();
            closeTimer.Interval = 20;
            closeTimer.Tick += (s, e) =>
            {
                closeOpacity -= 0.05;
                if (closeOpacity <= 0)
                {
                    closeTimer.Stop();
                    closeTimer.Dispose();
                    this.Opacity = 0;
                    this.Close();
                }
                else
                {
                    this.Opacity = closeOpacity;
                }
            };
            closeTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowSafeClose && e.CloseReason == CloseReason.UserClosing)
            {
                // Prevent immediate close, fade out instead
                e.Cancel = true;
                CloseSafely();
            }
            base.OnFormClosing(e);
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }
}
