using InventorySystem.Controls;
using InventorySystem.Helpers;
using InventorySystem.Models;
using InvetorySytstem.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace InventorySystem
{
    public partial class MainForm : Form
    {
        private List<Product> products = new List<Product>();
        private Panel navPanel, topPanel, contentPanel, footerPanel;
        private Label titleLabel, footerLabel;
        private Button btnMinimize, btnMaximize, btnClose;
        private bool isMaximized = true;
        private string userRole = "Owner"; // Default
        private Color accentColor = ColorTranslator.FromHtml("#7C3AED");
        public List<Product> Products { get; set; } = new List<Product>();

        public string ImageFolderPath { get; private set; }

        public MainForm(string role = "Owner")
        {
            userRole = role;
            LoadOrPromptImageFolder();
            InitUI();
        }

        private void LoadOrPromptImageFolder()
        {
            var config = ConfigManager.LoadConfig();

            if (config == null || string.IsNullOrEmpty(config.ImageFolderPath) || !Directory.Exists(config.ImageFolderPath))
            {
                string selectedPath = FolderHelper.PromptUserForImageFolder();
                if (string.IsNullOrEmpty(selectedPath))
                {
                    MessageBox.Show(
                        "No image folder selected. Please restart the application.",
                        "Folder Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    // ✅ Changed from Environment.Exit(0) to this.Close()
                    this.Close(); // Gracefully closes ONLY this form
                    return;
                }

                config = new AppConfig { ImageFolderPath = selectedPath };
                ConfigManager.SaveConfig(config);
            }

            ImageFolderPath = config.ImageFolderPath;
        }



        public string GetFullImagePath(string relativeImagePath)
        {
            if (string.IsNullOrEmpty(relativeImagePath))
                return null;

            return Path.Combine(ImageFolderPath, Path.GetFileName(relativeImagePath));
        }

        private void InitUI()
        {
            this.Text = "🚀 Inventory Pro | Ultimate Management System";
            this.MinimumSize = new Size(1366, 768);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColorTranslator.FromHtml("#0F172A");
            this.DoubleBuffered = true;
            this.Bounds = Screen.PrimaryScreen.WorkingArea;

            navPanel = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#0F172A"),
                Dock = DockStyle.Left,
                Width = 240
            };
            this.Controls.Add(navPanel);

            AddBrandHeader();
            AddTopPanel();
            AddFooterPanel();

            contentPanel = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#F8FAFC"),
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 0, 30, 20),
                AutoScroll = true
            };
            this.Controls.Add(contentPanel);

            // Add Navigation Buttons Based on Role
            AddRoleBasedButtons();

            AddUserProfileSection();
            ShowDashboard(null, null);
        }

        private void AddRoleBasedButtons()
        {
            int top = 110;

            if (userRole == "Owner")
            {
                AddNavButton("📊", "Dashboard", top, ShowDashboard); top += 60;
                AddNavButton("📦", "Products", top, ShowProductManagement); top += 60;
                AddNavButton("💳", "Sales", top, ShowSellScreen); top += 60;
                AddNavButton("📈", "Reports", top, ShowReportsControl); top += 60;
                AddNavButton("⚙️", "Settings", top, ShowSettingsControl); top += 60;
            }
            else if (userRole == "Sales")
            {
                AddNavButton("💳", "Sales", top, ShowSellScreen); top += 60;
            }

            AddNavButton("🚪", "Logout", top, (s, e) => this.Close());
        }

        private void AddBrandHeader()
        {
            Panel brandPanel = new Panel
            {
                Width = navPanel.Width,
                Height = 100,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };
            brandPanel.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    brandPanel.ClientRectangle,
                    ColorTranslator.FromHtml("#0F172A"),
                    ColorTranslator.FromHtml("#0F172A"),
                    45f);
                e.Graphics.FillRectangle(brush, brandPanel.ClientRectangle);
            };
            navPanel.Controls.Add(brandPanel);

            PictureBox logoBox = new PictureBox
            {
                Size = new Size(48, 48),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };
            try
            {
                logoBox.Image = Image.FromFile("logo.png");
            }
            catch
            {
                logoBox.Image = CreateVectorIcon();
            }
            brandPanel.Controls.Add(logoBox);

            Label nameLabel = new Label
            {
                Text = "INVENTORY PRO",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(80, 30)
            };
            brandPanel.Controls.Add(nameLabel);
        }

        private Image CreateVectorIcon()
        {
            Bitmap bmp = new Bitmap(48, 48);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawRectangle(pen, 10, 10, 28, 28);
                    g.DrawLine(pen, 10, 20, 38, 20);
                    g.DrawLine(pen, 10, 30, 38, 30);
                }
            }
            return bmp;
        }


        private void AddTopPanel()
        {
            topPanel = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#AFCAD2"),
                Dock = DockStyle.Top,
                Height = 60
            };
            topPanel.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(ColorTranslator.FromHtml("#E2E8F0"), 1),
                    0, topPanel.Height - 1, topPanel.Width, topPanel.Height - 1);
            };
            this.Controls.Add(topPanel);

            btnMinimize = CreateWindowButton("—", this.Width - 120);
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnMaximize = CreateWindowButton("⛶", this.Width - 80);
            btnMaximize.Click += (s, e) => ToggleMaximize();

            btnClose = CreateWindowButton("×", this.Width - 40);
            btnClose.Click += (s, e) => this.Close();
            btnClose.BackColor = ColorTranslator.FromHtml("#FEE2E2");
            btnClose.ForeColor = ColorTranslator.FromHtml("#DC2626");

            topPanel.Controls.Add(btnMinimize);
            topPanel.Controls.Add(btnMaximize);
            topPanel.Controls.Add(btnClose);

            titleLabel = new Label
            {
                Text = "📊  DASHBOARD OVERVIEW",
                ForeColor = ColorTranslator.FromHtml("#1E293B"),
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(260, 18)
            };
            topPanel.Controls.Add(titleLabel);

            topPanel.MouseDown += TopPanel_MouseDown;
            topPanel.MouseMove += TopPanel_MouseMove;
            topPanel.MouseUp += TopPanel_MouseUp;
        }

        private void AddFooterPanel()
        {
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                BackColor = ColorTranslator.FromHtml("#0F172A")
            };
            footerPanel.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(ColorTranslator.FromHtml("#AFCAD2"), 1), 0, 0, footerPanel.Width, 0);
            };
            this.Controls.Add(footerPanel);

            footerLabel = new Label
            {
                Text = $"⚡ v1.0.0 | © {DateTime.Now.Year} Prime Tech Lab Tz | Inventory Pro",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(20, 0, 0, 0)
            };
            footerPanel.Controls.Add(footerLabel);
        }

        private Button CreateWindowButton(string text, int left)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(32, 32),
                Location = new Point(left, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = ColorTranslator.FromHtml("#64748B"),
                Font = new Font("Segoe UI", 12)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#F1F5F9");
            btn.FlatAppearance.MouseDownBackColor = ColorTranslator.FromHtml("#E2E8F0");
            return btn;
        }

        private void ToggleMaximize()
        {
            if (isMaximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.Bounds = new Rectangle((Screen.PrimaryScreen.WorkingArea.Width - 1200) / 2,
                                            (Screen.PrimaryScreen.WorkingArea.Height - 800) / 2,
                                            1200, 800);
                isMaximized = false;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
                isMaximized = true;
            }
        }

        private void AddNavButton(string icon, string text, int top, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = $"  {icon}  {text}",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ColorTranslator.FromHtml("#E2E8F0"),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                Width = navPanel.Width,
                Height = 50,
                Location = new Point(0, top),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Padding = new Padding(20, 0, 0, 0)
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#334155");
            btn.FlatAppearance.MouseDownBackColor = accentColor;

            btn.Click += (s, e) =>
            {
                foreach (Control c in navPanel.Controls)
                {
                    if (c is Button b && b != btn)
                    {
                        b.BackColor = Color.Transparent;
                        b.ForeColor = ColorTranslator.FromHtml("#E2E8F0");
                        b.Font = new Font("Segoe UI", 11);
                    }
                }

                btn.BackColor = accentColor;
                btn.ForeColor = Color.White;
                btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);

                onClick?.Invoke(s, e);
            };

            navPanel.Controls.Add(btn);
        }

        private void AddUserProfileSection()
        {
            Panel profilePanel = new Panel
            {
                Width = navPanel.Width,
                Height = 80,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent
            };
            navPanel.Controls.Add(profilePanel);

            PictureBox profilePic = new PictureBox
            {
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(20, 20),
                BackColor = ColorTranslator.FromHtml("#E2E8F0")
            };
            profilePanel.Controls.Add(profilePic);

            Label userName = new Label
            {
                Text = userRole == "Owner" ? "Admin User" : "Salesperson",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(70, 22)
            };
            profilePanel.Controls.Add(userName);

            Label roleLabel = new Label
            {
                Text = userRole == "Owner" ? "System Administrator" : "Sales Staff",
                Font = new Font("Segoe UI", 8),
                ForeColor = ColorTranslator.FromHtml("#94A3B8"),
                AutoSize = true,
                Location = new Point(70, 40)
            };
            profilePanel.Controls.Add(roleLabel);
        }

        private void ShowDashboard(object sender, EventArgs e)
        {
            titleLabel.Text = "📊  DASHBOARD OVERVIEW";
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(new DashboardControl());
        }

        private void ShowProductManagement(object sender, EventArgs e)
        {
            titleLabel.Text = "📦  PRODUCT MANAGEMENT";
            contentPanel.Controls.Clear();

            var productControl = new ProductControl();
            productControl.Products = products;
            contentPanel.Controls.Add(productControl);
        }

        private void ShowSellScreen(object sender, EventArgs e)
        {
            titleLabel.Text = "💳  SALES TRANSACTIONS";
            contentPanel.Controls.Clear();

            var sellControl = new SellControl(this);
            sellControl.Products = products;
            contentPanel.Controls.Add(sellControl);
        }

        private void ShowReportsControl(object sender, EventArgs e)
        {
            titleLabel.Text = "📈  SALES REPORTS";
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(new ReportsControl());
        }

        private void ShowSettingsControl(object sender, EventArgs e)
        {
            titleLabel.Text = "⚙️  SYSTEM SETTINGS";
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(new SettingsControl() { Dock = DockStyle.Fill });
        }

        // Window Drag Events
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private void TopPanel_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void TopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void TopPanel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
    }
}
