using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class AdvancedOptionsControl : UserControl
    {
        private readonly Color PrimaryColor = Color.FromArgb(59, 130, 246); // Blue-600
        private readonly Color TextColor = Color.FromArgb(31, 41, 55); // Gray-800
        private readonly Color SecondaryTextColor = Color.FromArgb(107, 114, 128); // Gray-500
        private readonly Color BorderColor = Color.FromArgb(229, 231, 235); // Gray-200
        private readonly Color CardBgColor = Color.FromArgb(249, 250, 251); // Gray-50

        // Controls for the features
        private CheckBox chkAuditTrail;
        private CheckBox chkSwitchUserLockScreen;
        private CheckBox chkTwoFactorAuth;

        public AdvancedOptionsControl()
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
                Text = "🧩 Advanced Options (Optional for Future)",
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                ForeColor = TextColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30),
            };
            mainPanel.Controls.Add(header);

            // --- Audit Trail ---
            var auditTrailCard = CreateCardPanel("🕵️ Audit Trail", 800, 120);
            mainPanel.Controls.Add(auditTrailCard);

            var auditDesc = new Label()
            {
                Text = "Record every user action (delete, sell, change) with timestamp.",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = SecondaryTextColor
            };
            auditTrailCard.Controls.Add(auditDesc);

            chkAuditTrail = new CheckBox()
            {
                Text = "Enable Audit Trail",
                Location = new Point(20, 70),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = TextColor,
                Cursor = Cursors.Hand
            };
            auditTrailCard.Controls.Add(chkAuditTrail);

            // --- Switch User / Lock Screen ---
            var lockScreenCard = CreateCardPanel("🧬 Switch User / Lock Screen", 800, 120);
            mainPanel.Controls.Add(lockScreenCard);

            var lockDesc = new Label()
            {
                Text = "Temporarily lock UI when stepping away.",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = SecondaryTextColor
            };
            lockScreenCard.Controls.Add(lockDesc);

            chkSwitchUserLockScreen = new CheckBox()
            {
                Text = "Enable Lock Screen",
                Location = new Point(20, 70),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = TextColor,
                Cursor = Cursors.Hand
            };
            lockScreenCard.Controls.Add(chkSwitchUserLockScreen);

            // --- Two-Factor Authentication ---
            var twoFactorCard = CreateCardPanel("🔑 Two-Factor Authentication (2FA)", 800, 150);
            mainPanel.Controls.Add(twoFactorCard);

            var twoFactorDesc = new Label()
            {
                Text = "Add PIN or OTP for login (especially for cloud mode).",
                Location = new Point(20, 40),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = SecondaryTextColor
            };
            twoFactorCard.Controls.Add(twoFactorDesc);

            chkTwoFactorAuth = new CheckBox()
            {
                Text = "Enable Two-Factor Authentication",
                Location = new Point(20, 70),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10),
                ForeColor = TextColor,
                Cursor = Cursors.Hand
            };
            twoFactorCard.Controls.Add(chkTwoFactorAuth);

            // Optional: Add Save button to apply changes in the future
            var btnSave = new Button()
            {
                Text = "Save Settings",
                Location = new Point(650, 10),
                Size = new Size(130, 36),
                BackColor = PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(37, 99, 235); // Blue-700
            btnSave.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216); // Blue-800
            btnSave.Click += (s, e) =>
            {
                // Placeholder for future save logic
                MessageBox.Show("Settings saved (not implemented).", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            mainPanel.Controls.Add(btnSave);
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
    }
}
