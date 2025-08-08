using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class BackupRecoveryControl : UserControl
    {
        // Colors to match your theme
        private readonly Color PrimaryColor = Color.FromArgb(59, 130, 246); // Blue-600
        private readonly Color TextColor = Color.FromArgb(31, 41, 55); // Gray-800
        private readonly Color SecondaryTextColor = Color.FromArgb(107, 114, 128); // Gray-500
        private readonly Color BorderColor = Color.FromArgb(229, 231, 235); // Gray-200
        private readonly Color CardBgColor = Color.FromArgb(249, 250, 251); // Gray-50

        private FlowLayoutPanel mainPanel;

        // Controls
        private Button btnManualBackupUSB;
        private Button btnManualBackupCloud;
        private Button btnRestoreBackup;

        private CheckBox chkScheduledBackup;
        private ComboBox cmbBackupFrequency;

        private CheckBox chkAutoCloudSync;

        private Button btnExportExcel;
        private Button btnExportPDF;

        public BackupRecoveryControl()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10);
            this.Padding = new Padding(20);

            mainPanel = new FlowLayoutPanel()
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
                Text = "🔄 Backup & Recovery",
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                ForeColor = TextColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30),
            };
            mainPanel.Controls.Add(header);

            // --- Manual Backup Card ---
            var manualBackupCard = CreateCardPanel("Manual Backup", 800, 100);
            mainPanel.Controls.Add(manualBackupCard);

            btnManualBackupUSB = CreatePrimaryButton("Backup to USB Drive", 20, 40, 180);
            btnManualBackupUSB.Click += BtnManualBackupUSB_Click;
            manualBackupCard.Controls.Add(btnManualBackupUSB);

            btnManualBackupCloud = CreatePrimaryButton("Backup to Cloud (Google Drive)", 220, 40, 240);
            btnManualBackupCloud.Click += BtnManualBackupCloud_Click;
            manualBackupCard.Controls.Add(btnManualBackupCloud);

            // --- Restore Backup Card ---
            var restoreCard = CreateCardPanel("Restore from Backup", 800, 80);
            mainPanel.Controls.Add(restoreCard);

            btnRestoreBackup = CreatePrimaryButton("Restore Backup", 20, 25, 160);
            btnRestoreBackup.Click += BtnRestoreBackup_Click;
            restoreCard.Controls.Add(btnRestoreBackup);

            // --- Scheduled Backup Card ---
            var scheduledBackupCard = CreateCardPanel("Scheduled Backup", 800, 110);
            mainPanel.Controls.Add(scheduledBackupCard);

            chkScheduledBackup = new CheckBox()
            {
                Text = "Enable Scheduled Backup",
                Location = new Point(20, 25),
                AutoSize = true,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 11),
            };
            chkScheduledBackup.CheckedChanged += ChkScheduledBackup_CheckedChanged;
            scheduledBackupCard.Controls.Add(chkScheduledBackup);

            var lblFrequency = new Label()
            {
                Text = "Backup Frequency:",
                Location = new Point(40, 60),
                AutoSize = true,
                ForeColor = SecondaryTextColor,
                Font = new Font("Segoe UI", 10)
            };
            scheduledBackupCard.Controls.Add(lblFrequency);

            cmbBackupFrequency = new ComboBox()
            {
                Location = new Point(160, 55),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            cmbBackupFrequency.Items.AddRange(new string[] { "Daily", "Weekly", "Monthly" });
            cmbBackupFrequency.SelectedIndex = 0;
            scheduledBackupCard.Controls.Add(cmbBackupFrequency);

            // --- Auto Cloud Sync Card ---
            var cloudSyncCard = CreateCardPanel("Auto Cloud Sync", 800, 80);
            mainPanel.Controls.Add(cloudSyncCard);

            chkAutoCloudSync = new CheckBox()
            {
                Text = "Enable Auto Cloud Sync (Firestore / CloudDB)",
                Location = new Point(20, 25),
                AutoSize = true,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 11),
            };
            cloudSyncCard.Controls.Add(chkAutoCloudSync);

            // --- Export Data Card ---
            var exportCard = CreateCardPanel("Export Data", 800, 100);
            mainPanel.Controls.Add(exportCard);

            btnExportExcel = CreatePrimaryButton("Export to Excel", 20, 40, 160);
            btnExportExcel.Click += BtnExportExcel_Click;
            exportCard.Controls.Add(btnExportExcel);

            btnExportPDF = CreatePrimaryButton("Export to PDF", 200, 40, 160);
            btnExportPDF.Click += BtnExportPDF_Click;
            exportCard.Controls.Add(btnExportPDF);
        }

        private void ChkScheduledBackup_CheckedChanged(object sender, EventArgs e)
        {
            cmbBackupFrequency.Enabled = chkScheduledBackup.Checked;
        }

        // Event handlers - placeholders
        private void BtnManualBackupUSB_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Manual backup to USB initiated.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void BtnManualBackupCloud_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Manual backup to Cloud initiated.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void BtnRestoreBackup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Restore from backup initiated.", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export to Excel initiated.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void BtnExportPDF_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export to PDF initiated.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
