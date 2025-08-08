using InventorySystem.Helpers;
using InventorySystem.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InventorySystem
{
    public class DashboardControl : UserControl
    {
        Panel wrapperPanel;
        Panel cardsPanel;
        GroupBox activityBox;
        private Panel activitiesContainer; // To hold activity items
        private System.Windows.Forms.Timer refreshTimer;

        private const int RefreshInterval = 5000; // 5 seconds

        public DashboardControl()
        {
            InitializeUI();
            InitializeRefreshTimer(); // ✅ Add this
        }


        private void LoadActivities()
        {
            if (activitiesContainer != null)
            {
                activitiesContainer.Controls.Clear();
            }
            else
            {
                activitiesContainer = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    AutoScroll = true,
                    Padding = new Padding(20, 15, 20, 20)
                };
                activityBox.Controls.Add(activitiesContainer);
            }

            var activities = DatabaseHelper.LoadRecentActivities(10);
            int y = 20;

            foreach (var act in activities)
            {
                var activityItem = new Panel
                {
                    BackColor = Color.White,
                    Width = activitiesContainer.ClientSize.Width - 40,
                    Height = 60,
                    Location = new Point(20, y),
                    Padding = new Padding(45, 10, 15, 10),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };

                activityItem.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(ColorTranslator.FromHtml("#F1F5F9"), 1))
                    {
                        e.Graphics.DrawLine(pen, 40, activityItem.Height - 1, activityItem.Width, activityItem.Height - 1);
                    }
                };

                var timelineDot = new Panel
                {
                    Size = new Size(10, 10),
                    BackColor = ColorTranslator.FromHtml("#3B82F6"),
                    Location = new Point(20, 25),
                    Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 10, 10, 5, 5))
                };
                activityItem.Controls.Add(timelineDot);

                var activityLabel = new Label
                {
                    Text = act,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = ColorTranslator.FromHtml("#334155"),
                    AutoSize = false,
                    MaximumSize = new Size(activityItem.Width - 120, 0),
                    Location = new Point(40, 10),
                    TextAlign = ContentAlignment.TopLeft,
                    Dock = DockStyle.Fill
                };
                activityItem.Controls.Add(activityLabel);

                using (Graphics g = CreateGraphics())
                {
                    SizeF textSize = g.MeasureString(act, activityLabel.Font, activityLabel.MaximumSize.Width);
                    activityItem.Height = Math.Max(60, (int)textSize.Height + 30);
                }

                var timeLabel = new Label
                {
                    Text = DateTime.Now.ToString("hh:mm tt"),
                    Font = new Font("Segoe UI", 8, FontStyle.Regular),
                    ForeColor = ColorTranslator.FromHtml("#94A3B8"),
                    AutoSize = true,
                    Location = new Point(activityItem.Width - 85, 10),
                    TextAlign = ContentAlignment.TopRight
                };
                activityItem.Controls.Add(timeLabel);

                activityItem.MouseEnter += (s, e) =>
                {
                    activityItem.BackColor = ColorTranslator.FromHtml("#F8FAFC");
                    timelineDot.BackColor = ColorTranslator.FromHtml("#2563EB");
                    activityLabel.Font = new Font(activityLabel.Font, FontStyle.Bold);
                };
                activityItem.MouseLeave += (s, e) =>
                {
                    activityItem.BackColor = Color.White;
                    timelineDot.BackColor = ColorTranslator.FromHtml("#3B82F6");
                    activityLabel.Font = new Font(activityLabel.Font, FontStyle.Regular);
                };

                activitiesContainer.Controls.Add(activityItem);
                y += activityItem.Height + 5;
            }

            var shadowPanel = new Panel
            {
                Height = 15,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent
            };
            shadowPanel.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(0, 15),
                    Color.FromArgb(20, Color.Black),
                    Color.Transparent))
                {
                    e.Graphics.FillRectangle(brush, shadowPanel.ClientRectangle);
                }
            };
            activitiesContainer.Controls.Add(shadowPanel);
        }


        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColorTranslator.FromHtml("#F8FAFC"); // Lighter, airy background

            // Wrapper Panel with modern spacing
            wrapperPanel = new Panel
            {
                Location = new Point(240, 76),
                Width = this.Width - 240,
                Height = this.Height - 76,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Padding = new Padding(30, 0, 30, 30)
            };
            this.Controls.Add(wrapperPanel);

            // Main layout with improved structure
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0),
                RowStyles = {
                    new RowStyle(SizeType.Absolute, 220), // Cards row
                    new RowStyle(SizeType.Percent, 100)   // Activity row
                }
            };
            wrapperPanel.Controls.Add(layout);

            // Cards Panel with horizontal scroll
            cardsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                AutoScroll = true,
                Padding = new Padding(10, 20, 10, 10)
            };
            layout.Controls.Add(cardsPanel, 0, 0);

            // Add metric cards with modern design
            int totalProducts = DatabaseHelper.GetProductCount();
            decimal monthlySales = DatabaseHelper.GetMonthlySales();
            decimal inventoryValue = DatabaseHelper.GetInventoryValue();
            int lowStock = DatabaseHelper.GetLowStockCount();

            AddCard(cardsPanel, 0, "#8B5CF6", "Total Products", totalProducts.ToString(), "📦");
            AddCard(cardsPanel, 1, "#3B82F6", "Monthly Sales", $"TZS {monthlySales:N0}", "💳");
            AddCard(cardsPanel, 2, "#10B981", "Inventory Value", $"TZS {inventoryValue:N0}", "💰");
            AddCard(cardsPanel, 3, "#EF4444", "Low Stock Items", lowStock.ToString(), "⚠️");

            // Activity Box with modern styling
            activityBox = new GroupBox
            {
                Text = "  Recent Activity  ",
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1E293B"),
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20, 25, 20, 20),
                Margin = new Padding(0, 15, 0, 0)
            };
            layout.Controls.Add(activityBox, 0, 1);

            // Modern GroupBox styling
            activityBox.Paint += (s, e) =>
            {
                var box = s as GroupBox;
                e.Graphics.Clear(box.BackColor);

                // Draw modern border
                using (var pen = new Pen(ColorTranslator.FromHtml("#E2E8F0"), 2))
                {
                    e.Graphics.DrawRoundedRectangle(pen, 1, 15, box.Width - 3, box.Height - 17, 8);
                }

                // Draw text with modern typography
                TextRenderer.DrawText(e.Graphics, box.Text, box.Font,
                    new Point(15, 0), box.ForeColor, box.BackColor);

                // Add modern inner glow
                using (var pen = new Pen(Color.FromArgb(15, 59, 130, 246)))
                {
                    e.Graphics.DrawRoundedRectangle(pen, 2, 16, box.Width - 4, box.Height - 18, 8);
                }
            };

            /// Load and display activities with premium list styling
            LoadActivities();

        }

        private void InitializeRefreshTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = RefreshInterval;
            refreshTimer.Tick += RefreshActivities;
            refreshTimer.Start();
        }

        private void RefreshActivities(object sender, EventArgs e)
        {
            LoadActivities();
        }
        private void AddCard(Panel panel, int index, string color, string title, string value, string icon)
        {
            int cardWidth = 200;
            int cardHeight = 110;
            int margin = 20;
            int left = index * (cardWidth + margin) + margin;
            int top = 10;

            Panel card = new Panel
            {
                BackColor = ColorTranslator.FromHtml(color),
                Size = new Size(cardWidth, cardHeight),
                Location = new Point(left, top),
                Padding = new Padding(10, 10, 10, 10),
                Cursor = Cursors.Hand
            };

            // Rounded corners
            card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 10, 10));

            // 📏 Fonts: small but clean
            Font iconFont = new Font("Segoe UI Emoji", 18, FontStyle.Regular);
            Font titleFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            Font valueFont = new Font("Segoe UI", 14.5f, FontStyle.Bold);

            Label iconLabel = new Label
            {
                Text = icon,
                Font = iconFont,
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            card.Controls.Add(iconLabel);

            Label titleLabel = new Label
            {
                Text = title,
                Font = titleFont,
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                AutoSize = true,
                Location = new Point(10, 45)
            };
            card.Controls.Add(titleLabel);

            Label valueLabel = new Label
            {
                Text = value,
                Font = valueFont,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(10, 65)
            };
            card.Controls.Add(valueLabel);

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BackColor = ControlPaint.Light(ColorTranslator.FromHtml(color), 0.1f);
                card.Location = new Point(card.Location.X, card.Location.Y - 2);
            };

            card.MouseLeave += (s, e) =>
            {
                card.BackColor = ColorTranslator.FromHtml(color);
                card.Location = new Point(card.Location.X, card.Location.Y + 2);
            };

            panel.Controls.Add(card);
        }


        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);
    }

    // Extension for rounded rectangles
    public static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, float x, float y, float width, float height, float radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
            path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            graphics.DrawPath(pen, path);
        }
    }
}