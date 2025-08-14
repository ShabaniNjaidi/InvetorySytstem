using InventorySystem;
using InventorySystem.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InvetorySytstem
{
    public partial class LoginForm : Form
    {
        public event EventHandler<(string username, string role)>? LoginSuccessful;

        private AppConfig appConfig;
        private CheckBox chkRememberMe;

        private readonly Color PrimaryColor = Color.FromArgb(0, 77, 102);
        private readonly Color SecondaryColor = Color.FromArgb(0, 169, 224);
        private readonly Color AccentColor = Color.FromArgb(255, 153, 0);
        private readonly Color CardBackground = Color.FromArgb(245, 245, 245);
        private readonly Color TextColor = Color.FromArgb(31, 41, 55);

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private PictureBox logoPictureBox;

        private bool allowSafeClose = false;
        private System.Windows.Forms.Timer? closeTimer;
        private double closeOpacity = 1;

        private readonly string? expectedRole; // 👈 Holds expected role (admin/employee/etc)

        // ✅ Constructor with optional role filtering
        public LoginForm(string? role = null)
        {
            InitializeComponent();
            expectedRole = role?.ToLower();
            InitializeUI();

            appConfig = ConfigManager.LoadConfig() ?? new AppConfig();

            if (!string.IsNullOrEmpty(appConfig.RememberedUsername))
            {
                txtUsername.Text = appConfig.RememberedUsername;
                chkRememberMe.Checked = true;
            }
        }

        private void InitializeUI()
        {
            this.Text = "Login - Inventory System";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = PrimaryColor;
            this.Font = new Font("Segoe UI", 10);

            logoPictureBox = new PictureBox
            {
                Image = SystemIcons.Application.ToBitmap(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(120, 120),
                Location = new Point((this.Width - 120) / 2, 30),
                BackColor = Color.Transparent
            };
            this.Controls.Add(logoPictureBox);

            var card = new Panel
            {
                Size = new Size(600, 450),
                Location = new Point((this.Width - 600) / 2, 180),
                BackColor = CardBackground,
                Padding = new Padding(20)
            };
            this.Controls.Add(card);

            var lblTitle = new Label
            {
                Text = "Login",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = TextColor,
                AutoSize = true,
                Location = new Point((card.Width - 200) / 2, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblTitle);

            var lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 12),
                ForeColor = TextColor,
                Location = new Point(50, 100),
                AutoSize = true
            };
            card.Controls.Add(lblUsername);

            txtUsername = new TextBox
            {
                Location = new Point(50, 130),
                Size = new Size(500, 40),
                Font = new Font("Segoe UI", 12),
                PlaceholderText = "Enter your username",
                BorderStyle = BorderStyle.FixedSingle
            };
            card.Controls.Add(txtUsername);

            var lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 12),
                ForeColor = TextColor,
                Location = new Point(50, 190),
                AutoSize = true
            };
            card.Controls.Add(lblPassword);

            txtPassword = new TextBox
            {
                Location = new Point(50, 220),
                Size = new Size(500, 40),
                Font = new Font("Segoe UI", 12),
                PlaceholderText = "Enter your password",
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            card.Controls.Add(txtPassword);

            chkRememberMe = new CheckBox
            {
                Text = "Remember Me",
                Location = new Point(50, 280),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = TextColor
            };
            card.Controls.Add(chkRememberMe);

            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(50, 330),
                Size = new Size(500, 50),
                BackColor = AccentColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 173, 51);
            btnLogin.Click += BtnLogin_Click;
            card.Controls.Add(btnLogin);

            var footerLabel = new Label
            {
                Text = "Inventory System v1.0",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point((this.Width - 160) / 2, this.Height - 40)
            };
            this.Controls.Add(footerLabel);

            card.Paint += (sender, e) =>
            {
                using (Pen shadowPen = new Pen(Color.FromArgb(20, 0, 0, 0)))
                {
                    e.Graphics.DrawRectangle(shadowPen, 0, 0, card.Width - 1, card.Height - 1);
                }
                this.ActiveControl = txtUsername;
            };
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var credentials = DatabaseHelper.GetUserCredentials(username, password);

            if (credentials != null)
            {
                int userId = credentials.Value.UserId;
                string detectedRole = credentials.Value.Role;

                SessionData.CurrentUserId = userId;
                SessionData.CurrentUsername = username;
                SessionData.CurrentRole = detectedRole;


                if (detectedRole != null)
                {
                    // 🔐 Validate role match if expectedRole was set (from WelcomeForm)
                    if (!string.IsNullOrEmpty(expectedRole) && !string.Equals(expectedRole, detectedRole, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show($"This user is not authorized as a '{expectedRole}'. Detected role: '{detectedRole}'.", "Role Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (chkRememberMe.Checked)
                        appConfig.RememberedUsername = username;
                    else
                        appConfig.RememberedUsername = null;

                    ConfigManager.SaveConfig(appConfig);

                    MessageBox.Show($"Welcome, {username} ({detectedRole})!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    allowSafeClose = true;
                    LoginSuccessful?.Invoke(this, (username, detectedRole));
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowSafeClose && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                closeOpacity = 1;

                closeTimer = new System.Windows.Forms.Timer();
                closeTimer.Interval = 10;
                closeTimer.Tick += (s, ev) =>
                {
                    closeOpacity -= 0.05;
                    this.Opacity = closeOpacity;
                    if (closeOpacity <= 0)
                    {
                        closeTimer.Stop();
                        Application.Exit();
                    }
                };
                closeTimer.Start();
            }
            base.OnFormClosing(e);
        }

        public void CloseSafely()
        {
            allowSafeClose = true;
            this.Close();
        }
    }
}
