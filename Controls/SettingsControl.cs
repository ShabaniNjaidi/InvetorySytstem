using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class SettingsControl : UserControl
    {
        private Panel headerPanel;
        private Panel contentPanel;
        private FlowLayoutPanel navBarPanel;
        private Panel displayPanel;
        private Panel containerPanel;
        private Button activeNavButton;

        private ProfileManagementControl profileControl;
        private SystemPreferencesControl systemPreferencesControl;
        private BackupRecoveryControl backupControl;
        private AdminAnalyticsControl adminControl;
        private AdvancedOptionsControl advancedControl;

        // Enhanced color palette
        private readonly Color PrimaryColor = Color.FromArgb(59, 130, 246); // Blue-600
        private readonly Color NavBgColor = Color.FromArgb(249, 250, 251); // Gray-50
        private readonly Color HoverColor = Color.FromArgb(243, 244, 246); // Gray-100
        private readonly Color ActiveColor = Color.FromArgb(239, 246, 255); // Blue-50
        private readonly Color TextColor = Color.FromArgb(31, 41, 55); // Gray-800
        private readonly Color SecondaryTextColor = Color.FromArgb(75, 85, 99); // Gray-600
        private readonly Color BorderColor = Color.FromArgb(229, 231, 235); // Gray-200

        public SettingsControl()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Main container
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(260, 10, 20, 20);
            this.AutoScroll = true;
            this.BackColor = Color.White;

            // Container Panel with subtle shadow effect
            containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(containerPanel);

            // Header Panel with improved typography
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(25, 15, 0, 0)
            };

            var headerLabel = new Label
            {
                Text = "⚙️  Settings",
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
                ForeColor = TextColor,
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 5)
            };

            headerPanel.Controls.Add(headerLabel);

            // Main Content Panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            containerPanel.Controls.Add(contentPanel);
            containerPanel.Controls.Add(headerPanel);

            // NavBar Panel with improved styling
            navBarPanel = new FlowLayoutPanel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = NavBgColor,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                WrapContents = false,
                Padding = new Padding(15, 15, 0, 0),
                BorderStyle = BorderStyle.FixedSingle // or BorderStyle.None if you don't want a border
                                                      // Remove BorderColor property here
            };


            contentPanel.Controls.Add(navBarPanel);

            // Display Panel with subtle border
            displayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(25, 20, 25, 20),
                BorderStyle = BorderStyle.None
            };
            contentPanel.Controls.Add(displayPanel);

            // Initialize section controls
            profileControl = new ProfileManagementControl();
            systemPreferencesControl = new SystemPreferencesControl();
            backupControl = new BackupRecoveryControl();
            adminControl = new AdminAnalyticsControl();
            advancedControl = new AdvancedOptionsControl();

            // Create navigation buttons with improved styling
            var btnProfile = CreateNavButton("Profile", "👤", (s, e) => ShowControl(profileControl, (Button)s));
            var btnSystem = CreateNavButton("System", "⚙️", (s, e) => ShowControl(systemPreferencesControl, (Button)s));
            var btnBackup = CreateNavButton("Backup", "🔄", (s, e) => ShowControl(backupControl, (Button)s));
            var btnAdmin = CreateNavButton("Admin", "📊", (s, e) => ShowControl(adminControl, (Button)s));
            var btnAdvanced = CreateNavButton("Advanced", "🔧", (s, e) => ShowControl(advancedControl, (Button)s));

            navBarPanel.Controls.AddRange(new Control[]
            {
                btnProfile, btnSystem, btnBackup, btnAdmin, btnAdvanced
            });

            // Show default view
            ShowControl(profileControl, btnProfile);
        }

        private Button CreateNavButton(string text, string icon, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = $"{icon} {text}",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = SecondaryTextColor,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 36),
                Margin = new Padding(5, 0, 5, 0),
                Cursor = Cursors.Hand,
                Padding = new Padding(5, 0, 5, 0)
            };

            btn.FlatAppearance.BorderSize = 0;
           //tn.FlatAppearance.BorderColor = Color.Transparent;
            btn.FlatAppearance.MouseOverBackColor = HoverColor;
            btn.FlatAppearance.MouseDownBackColor = ActiveColor;

            // Smooth transitions
            btn.MouseEnter += (s, e) => { btn.Font = new Font(btn.Font, FontStyle.Bold); };
            btn.MouseLeave += (s, e) => {
                if (btn != activeNavButton)
                    btn.Font = new Font(btn.Font, FontStyle.Regular);
            };

            btn.Click += onClick;
            return btn;
        }

        private void ShowControl(UserControl controlToShow, Button navButton)
        {
            if (activeNavButton != null)
            {
                activeNavButton.ForeColor = SecondaryTextColor;
                activeNavButton.Font = new Font(activeNavButton.Font, FontStyle.Regular);
                activeNavButton.BackColor = Color.Transparent;
            }

            navButton.ForeColor = PrimaryColor;
            navButton.Font = new Font(navButton.Font, FontStyle.Bold);
            navButton.BackColor = ActiveColor;
            activeNavButton = navButton;

            displayPanel.Controls.Clear();
            controlToShow.Dock = DockStyle.Fill;
            displayPanel.Controls.Add(controlToShow);
        }
    }
}