using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class SystemPreferencesControl : UserControl
    {
        // Color Palette
        private readonly Color _primaryColor = Color.FromArgb(59, 130, 246);
        private readonly Color _textColor = Color.FromArgb(31, 41, 55);
        private readonly Color _secondaryTextColor = Color.FromArgb(107, 114, 128);
        private readonly Color _borderColor = Color.FromArgb(229, 231, 235);
        private readonly Color _cardBgColor = Color.FromArgb(249, 250, 251);

        // UI Controls
        private CheckBox _chkSoundEffects;
        private DateTimePicker _dtpSaleStart;
        private DateTimePicker _dtpSaleEnd;
        private CheckBox _chkNetworkSync;
        private CheckBox _chkDarkMode;
        private CheckBox _chkAutoBackup;
        private RadioButton _rdoLocalBackup;
        private RadioButton _rdoCloudBackup;
        private Panel _backupOptionsPanel;
        private FlowLayoutPanel _mainPanel;

        public SystemPreferencesControl()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main Container Setup
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10);
            this.Padding = new Padding(20);

            _mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };
            this.Controls.Add(_mainPanel);

            AddHeader();
            AddSoundEffectsCard();
            AddSalePeriodCard();
            AddNetworkSyncCard();
            AddAutoBackupCard();
            AddDarkModeCard();
        }

        private void AddHeader()
        {
            var header = new Label
            {
                Text = "⚙️ System Preferences",
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30)
            };
            _mainPanel.Controls.Add(header);
        }

        private void AddSoundEffectsCard()
        {
            var card = CreateCardPanel("Sound Effects", 800, 80);
            _mainPanel.Controls.Add(card);

            _chkSoundEffects = new CheckBox
            {
                Text = "Enable Sound Effects",
                Location = new Point(20, 25),
                AutoSize = true,
                Checked = true,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 11)
            };
            card.Controls.Add(_chkSoundEffects);
        }

        private void AddSalePeriodCard()
        {
            var card = CreateCardPanel("Sale Period", 800, 110);
            _mainPanel.Controls.Add(card);

            var lblSalePeriod = new Label
            {
                Text = "Allowed Selling Hours:",
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = _textColor,
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold)
            };
            card.Controls.Add(lblSalePeriod);

            var lblFrom = new Label
            {
                Text = "From:",
                Location = new Point(30, 60),
                AutoSize = true,
                ForeColor = _secondaryTextColor,
                Font = new Font("Segoe UI", 10)
            };
            card.Controls.Add(lblFrom);

            _dtpSaleStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location = new Point(80, 55),
                Width = 120,
                Value = DateTime.Today.AddHours(8)
            };
            card.Controls.Add(_dtpSaleStart);

            var lblTo = new Label
            {
                Text = "To:",
                Location = new Point(230, 60),
                AutoSize = true,
                ForeColor = _secondaryTextColor,
                Font = new Font("Segoe UI", 10)
            };
            card.Controls.Add(lblTo);

            _dtpSaleEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Location = new Point(260, 55),
                Width = 120,
                Value = DateTime.Today.AddHours(22)
            };
            card.Controls.Add(_dtpSaleEnd);
        }

        private void AddNetworkSyncCard()
        {
            var card = CreateCardPanel("Network Synchronization", 800, 80);
            _mainPanel.Controls.Add(card);

            _chkNetworkSync = new CheckBox
            {
                Text = "Enable Network Sync (Cloud Synchronization)",
                Location = new Point(20, 25),
                AutoSize = true,
                Checked = false,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 11)
            };
            card.Controls.Add(_chkNetworkSync);
        }

        private void AddAutoBackupCard()
        {
            var card = CreateCardPanel("Automatic Backup", 800, 140);
            _mainPanel.Controls.Add(card);

            _chkAutoBackup = new CheckBox
            {
                Text = "Enable Automatic Backup",
                Location = new Point(20, 20),
                AutoSize = true,
                Checked = false,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 11)
            };
            _chkAutoBackup.CheckedChanged += AutoBackup_CheckedChanged;
            card.Controls.Add(_chkAutoBackup);

            _backupOptionsPanel = new Panel
            {
                Location = new Point(40, 55),
                Size = new Size(300, 70),
                Visible = false
            };
            card.Controls.Add(_backupOptionsPanel);

            _rdoLocalBackup = new RadioButton
            {
                Text = "Local Backup",
                Location = new Point(0, 10),
                AutoSize = true,
                Checked = true,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 10)
            };
            _backupOptionsPanel.Controls.Add(_rdoLocalBackup);

            _rdoCloudBackup = new RadioButton
            {
                Text = "Cloud Backup",
                Location = new Point(140, 10),
                AutoSize = true,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 10)
            };
            _backupOptionsPanel.Controls.Add(_rdoCloudBackup);
        }

        private void AddDarkModeCard()
        {
            var card = CreateCardPanel("Appearance", 800, 80);
            _mainPanel.Controls.Add(card);

            _chkDarkMode = new CheckBox
            {
                Text = "Enable Dark Mode",
                Location = new Point(20, 25),
                AutoSize = true,
                Checked = false,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 11)
            };
            _chkDarkMode.CheckedChanged += DarkMode_CheckedChanged;
            card.Controls.Add(_chkDarkMode);
        }

        private Panel CreateCardPanel(string title, int width, int height)
        {
            var panel = new Panel
            {
                Width = width,
                Height = height,
                BackColor = _cardBgColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 20)
            };

            panel.Paint += (sender, e) =>
            {
                using var pen = new Pen(_borderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(10, 5),
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true
            };
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private void AutoBackup_CheckedChanged(object sender, EventArgs e)
        {
            _backupOptionsPanel.Visible = _chkAutoBackup.Checked;
        }

        private void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            bool dark = _chkDarkMode.Checked;
            this.BackColor = dark ? Color.FromArgb(30, 30, 30) : Color.White;
            _mainPanel.BackColor = this.BackColor;

            foreach (Control c in _mainPanel.Controls)
            {
                if (c is Panel p)
                {
                    p.BackColor = dark ? Color.FromArgb(45, 45, 48) : _cardBgColor;
                    foreach (Control child in p.Controls)
                    {
                        child.ForeColor = dark ? Color.White : _textColor;
                        if (child is DateTimePicker dtp)
                        {
                            dtp.CalendarForeColor = dark ? Color.White : Color.Black;
                            dtp.CalendarMonthBackground = dark ? Color.FromArgb(45, 45, 48) : Color.White;
                        }
                    }
                }
                else
                {
                    c.ForeColor = dark ? Color.White : _textColor;
                }
            }
        }

        // Property Accessors
        public bool SoundEffectsEnabled => _chkSoundEffects?.Checked ?? false;
        public TimeSpan SaleStartTime => _dtpSaleStart?.Value.TimeOfDay ?? TimeSpan.Zero;
        public TimeSpan SaleEndTime => _dtpSaleEnd?.Value.TimeOfDay ?? TimeSpan.Zero;
        public bool NetworkSyncEnabled => _chkNetworkSync?.Checked ?? false;
        public bool AutoBackupEnabled => _chkAutoBackup?.Checked ?? false;
        public bool BackupToLocal => _rdoLocalBackup?.Checked ?? false;
        public bool BackupToCloud => _rdoCloudBackup?.Checked ?? false;
        public bool DarkModeEnabled => _chkDarkMode?.Checked ?? false;
    }
}