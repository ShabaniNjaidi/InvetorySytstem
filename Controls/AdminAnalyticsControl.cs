using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace InventorySystem.Controls
{
    public class AdminAnalyticsControl : UserControl
    {
        // Colors matching your theme
        private readonly Color PrimaryColor = Color.FromArgb(59, 130, 246); // Blue-600
        private readonly Color TextColor = Color.FromArgb(31, 41, 55); // Gray-800
        private readonly Color SecondaryTextColor = Color.FromArgb(107, 114, 128); // Gray-500
        private readonly Color BorderColor = Color.FromArgb(229, 231, 235); // Gray-200
        private readonly Color CardBgColor = Color.FromArgb(249, 250, 251); // Gray-50

        // Controls for admin profile
        private TextBox txtShopName, txtPhone, txtEmail;
        private PictureBox picLogo;
        private Button btnUploadLogo, btnSaveProfile;

        // Data usage label
        private Label lblDataUsage;

        // Version info and update button
        private Label lblVersionInfo;
        private Button btnCheckUpdate;

        // Contact dev buttons
        private Button btnEmailDev, btnWhatsAppDev;

        public AdminAnalyticsControl()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10);
            this.Padding = new Padding(20);

            var mainPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10),
            };
            this.Controls.Add(mainPanel);

            var header = new Label()
            {
                Text = "🧠 Administration & Analytics",
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                ForeColor = TextColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30),
            };
            mainPanel.Controls.Add(header);

            // --- Admin Profile Card ---
            var profileCard = CreateCardPanel("Edit Admin Profile", 800, 280);
            mainPanel.Controls.Add(profileCard);

            // Logo picture box
            picLogo = new PictureBox()
            {
                Size = new Size(120, 120),
                Location = new Point(20, 40),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke,
                Image = CreateDefaultLogo(),
            };
            profileCard.Controls.Add(picLogo);

            btnUploadLogo = CreateSecondaryButton("Upload Logo", 20, 170, 120);
            btnUploadLogo.Click += BtnUploadLogo_Click;
            profileCard.Controls.Add(btnUploadLogo);

            int leftX = 170;
            int labelWidth = 100;
            int inputWidth = 350;
            int inputHeight = 34;
            int spacingY = 50;

            // Shop Name
            AddLabelAndTextbox(profileCard, "Shop Name:", leftX, 40, labelWidth, inputWidth, inputHeight, out txtShopName);

            // Phone
            AddLabelAndTextbox(profileCard, "Phone:", leftX, 40 + spacingY, labelWidth, inputWidth, inputHeight, out txtPhone);

            // Email
            AddLabelAndTextbox(profileCard, "Email:", leftX, 40 + spacingY * 2, labelWidth, inputWidth, inputHeight, out txtEmail);

            btnSaveProfile = CreatePrimaryButton("Save Profile", leftX + labelWidth, 40 + spacingY * 3 + 10, 150);
            btnSaveProfile.Click += BtnSaveProfile_Click;
            profileCard.Controls.Add(btnSaveProfile);

            // --- Data Usage Card ---
            var dataUsageCard = CreateCardPanel("Data Usage Monitor", 800, 100);
            mainPanel.Controls.Add(dataUsageCard);

            lblDataUsage = new Label()
            {
                Text = "Data synced/uploaded: 0 MB",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = TextColor
            };
            dataUsageCard.Controls.Add(lblDataUsage);

            // --- App Version Card ---
            var versionCard = CreateCardPanel("App Version Info", 800, 100);
            mainPanel.Controls.Add(versionCard);

            lblVersionInfo = new Label()
            {
                Text = "Version: 1.0.0 (Build 100)",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = TextColor
            };
            versionCard.Controls.Add(lblVersionInfo);

            btnCheckUpdate = CreatePrimaryButton("Check for Updates", 300, 35, 160);
            btnCheckUpdate.Click += BtnCheckUpdate_Click;
            versionCard.Controls.Add(btnCheckUpdate);

            // --- Contact Developer Card ---
            var contactCard = CreateCardPanel("Contact Developer / Feedback", 800, 120);
            mainPanel.Controls.Add(contactCard);

            var lblContactDesc = new Label()
            {
                Text = "Report bugs or request features via:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = TextColor
            };
            contactCard.Controls.Add(lblContactDesc);

            btnEmailDev = CreatePrimaryButton("Email", 20, 60, 120);
            btnEmailDev.Click += BtnEmailDev_Click;
            contactCard.Controls.Add(btnEmailDev);

            btnWhatsAppDev = CreatePrimaryButton("WhatsApp", 160, 60, 120);
            btnWhatsAppDev.Click += BtnWhatsAppDev_Click;
            contactCard.Controls.Add(btnWhatsAppDev);
        }

        private void AddLabelAndTextbox(Control container, string labelText, int x, int y, int labelWidth, int inputWidth, int inputHeight, out TextBox textBox)
        {
            var lbl = new Label()
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(labelWidth, inputHeight),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = SecondaryTextColor,
                Font = new Font("Segoe UI", 9.5f)
            };
            container.Controls.Add(lbl);

            textBox = new TextBox()
            {
                Location = new Point(x + labelWidth + 10, y),
                Size = new Size(inputWidth, inputHeight),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            container.Controls.Add(textBox);
        }

        private Panel CreateCardPanel(string title, int width, int height)
        {
            var panel = new Panel()
            {
                Width = width,
                Height = height,
                BackColor = CardBgColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 20)
            };

            panel.Paint += (sender, e) =>
            {
                using var pen = new Pen(BorderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            var titleLabel = new Label()
            {
                Text = title,
                Location = new Point(10, 5),
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = TextColor,
                AutoSize = true
            };
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private Button CreatePrimaryButton(string text, int x, int y, int width)
        {
            var btn = new Button()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 36),
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235); // Blue-700
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216); // Blue-800
            return btn;
        }

        private Button CreateSecondaryButton(string text, int x, int y, int width)
        {
            var btn = new Button()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 32),
                BackColor = Color.White,
                ForeColor = SecondaryTextColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(243, 244, 246); // Gray-100
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(229, 231, 235); // Gray-200
            return btn;
        }

        private Image CreateDefaultLogo()
        {
            Bitmap bmp = new Bitmap(120, 120);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                using var brush = new SolidBrush(SecondaryTextColor);
                g.FillRectangle(brush, 10, 40, 100, 40);
                g.FillEllipse(brush, 40, 10, 40, 40);
            }
            return bmp;
        }

        // Event handlers
        private void BtnUploadLogo_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
            dlg.Title = "Select Shop Logo";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var original = Image.FromFile(dlg.FileName);
                    picLogo.Image = original;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtShopName.Text))
            {
                MessageBox.Show("Please enter the shop name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtShopName.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            // Save logic placeholder
            MessageBox.Show("Admin profile saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCheckUpdate_Click(object sender, EventArgs e)
        {
            // Placeholder: check for updates logic
            MessageBox.Show("Checked for updates. You have the latest version.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEmailDev_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "mailto:support@example.com?subject=App Feedback",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Failed to open email client.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnWhatsAppDev_Click(object sender, EventArgs e)
        {
            try
            {
                // Open WhatsApp Web with chat to developer number (replace number with real one)
                string phone = "1234567890";
                string url = $"https://wa.me/{phone}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("Failed to open WhatsApp.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
