using InventorySystem.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventorySystem.Controls
{
    public class ProfileManagementControl : UserControl
    {
        // Color palette
        private readonly Color PrimaryColor = Color.FromArgb(59, 130, 246); // Blue-600
        private readonly Color DangerColor = Color.FromArgb(220, 38, 38); // Red-600
        private readonly Color SuccessColor = Color.FromArgb(22, 163, 74); // Green-600
        private readonly Color TextColor = Color.FromArgb(31, 41, 55); // Gray-800
        private readonly Color SecondaryTextColor = Color.FromArgb(107, 114, 128); // Gray-500
        private readonly Color BorderColor = Color.FromArgb(229, 231, 235); // Gray-200
        private readonly Color CardBgColor = Color.FromArgb(249, 250, 251); // Gray-50

        // Controls
        private TextBox txtNewUsername;
        private TextBox txtNewPassword;
        private ListView lvSalespersons;
        private TextBox txtOldPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtFullName;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private ListView lvActivityLog;
        private PictureBox picProfile;
        private Button btnUploadPicture;
        private Button btnSaveProfile;
        private Button btnChangePassword;
        private Button btnLogoutAllDevices;



        public ProfileManagementControl()
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
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20),
            };
            this.Controls.Add(mainPanel);

            // === Header ===
            var header = new Label()
            {
                Text = "🛡️ Account Management",
                Font = new Font("Segoe UI Semibold", 20, FontStyle.Bold),
                ForeColor = TextColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30)
            };
            mainPanel.Controls.Add(header);

            // === Profile Section ===
            var profileCard = CreateCardPanel("Profile", 800, 260);
            mainPanel.Controls.Add(profileCard);

            picProfile = new PictureBox()
            {
                Size = new Size(120, 120),
                Location = new Point(20, 40),
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = CreateDefaultProfileImage(),
                BackColor = Color.Transparent
            };
            profileCard.Controls.Add(picProfile);

            btnUploadPicture = CreateSecondaryButton("Upload Picture", 20, 170, 120);
            btnUploadPicture.Click += BtnUploadPicture_Click;
            profileCard.Controls.Add(btnUploadPicture);

            int editLeft = 170;
            int inputWidth = 300;
            int inputHeight = 34;
            int labelWidth = 100;

            var lblEditProfile = new Label()
            {
                Text = "Personal Information",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                Location = new Point(editLeft, 20),
                AutoSize = true,
                ForeColor = TextColor
            };
            profileCard.Controls.Add(lblEditProfile);

            var fullNameControls = CreateFormInput("Full Name:", editLeft, 60, labelWidth, inputWidth, inputHeight);
            profileCard.Controls.AddRange(fullNameControls);
            txtFullName = (TextBox)fullNameControls[1];

            var phoneControls = CreateFormInput("Phone:", editLeft, 110, labelWidth, inputWidth, inputHeight);
            profileCard.Controls.AddRange(phoneControls);
            txtPhone = (TextBox)phoneControls[1];

            var emailControls = CreateFormInput("Email:", editLeft, 160, labelWidth, inputWidth, inputHeight);
            profileCard.Controls.AddRange(emailControls);
            txtEmail = (TextBox)emailControls[1];

            btnSaveProfile = CreatePrimaryButton("Save Changes", editLeft + labelWidth, 200, 150);
            btnSaveProfile.Click += BtnSaveProfile_Click;
            profileCard.Controls.Add(btnSaveProfile);

            // === Password Section ===
            var passwordCard = CreateCardPanel("Security", 800, 260);
            mainPanel.Controls.Add(passwordCard);

            var lblChangePassword = new Label()
            {
                Text = "Change Password",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = TextColor
            };
            passwordCard.Controls.Add(lblChangePassword);

            var oldPwdControls = CreatePasswordInput("Current Password:", 20, 60, labelWidth, inputWidth, inputHeight);
            passwordCard.Controls.AddRange(oldPwdControls);
            txtOldPassword = (TextBox)oldPwdControls[1];

            var newPwdControls = CreatePasswordInput("New Password:", 20, 110, labelWidth, inputWidth, inputHeight);
            passwordCard.Controls.AddRange(newPwdControls);
            txtNewPassword = (TextBox)newPwdControls[1]; // ✅ uses field

            var confirmPwdControls = CreatePasswordInput("Confirm Password:", 20, 160, labelWidth, inputWidth, inputHeight);
            passwordCard.Controls.AddRange(confirmPwdControls);
            txtConfirmPassword = (TextBox)confirmPwdControls[1];

            btnChangePassword = CreatePrimaryButton("Update Password", 20 + labelWidth, 200, 160);
            btnChangePassword.Click += BtnChangePassword_Click;
            passwordCard.Controls.Add(btnChangePassword);

            // === Salesperson Management Section ===
            var staffCard = CreateCardPanel("Manage Sales Team", 800, 300);
            mainPanel.Controls.Add(staffCard);

            var lblStaffHeader = new Label()
            {
                Text = "Salespersons",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = TextColor
            };
            staffCard.Controls.Add(lblStaffHeader);

            // Username textbox (class-level field)
            txtNewUsername = new TextBox()
            {
                Location = new Point(20, 60),
                Width = 200,
                PlaceholderText = "Username"
            };
            staffCard.Controls.Add(txtNewUsername);

            // Password textbox (class-level field)
            txtNewPassword = new TextBox()
            {
                Location = new Point(240, 60),
                Width = 200,
                UseSystemPasswordChar = true,
                PlaceholderText = "Password"
            };
            staffCard.Controls.Add(txtNewPassword);

            // Add Button
            var btnAddSalesperson = CreatePrimaryButton("Add", 460, 60, 80);
            btnAddSalesperson.Click += (s, e) =>
            {
                string user = txtNewUsername.Text.Trim();
                string pass = txtNewPassword.Text.Trim();
                if (user == "" || pass == "")
                {
                    MessageBox.Show("Please enter both username and password.");
                    return;
                }

                if (DatabaseHelper.AddUser(user, pass, "employee"))
                {
                    MessageBox.Show("Salesperson added.");
                    txtNewUsername.Clear();
                    txtNewPassword.Clear();
                    LoadSalespersonList(); // ✅ Refresh ListView
                }
                else
                {
                    MessageBox.Show("Username already exists.");
                }
            };
            staffCard.Controls.Add(btnAddSalesperson);

            // Salesperson ListView (assigned to field)
            lvSalespersons = new ListView()
            {
                Location = new Point(20, 110),
                Size = new Size(750, 140),
                View = View.Details,
                FullRowSelect = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            lvSalespersons.Columns.Add("Username", 300);
            lvSalespersons.Columns.Add("Role", 100);
            staffCard.Controls.Add(lvSalespersons);

            // Delete Button
            var btnDeleteSalesperson = CreateDangerButton("Remove Selected", 600, 60, 160);
            btnDeleteSalesperson.Click += (s, e) =>
            {
                if (lvSalespersons.SelectedItems.Count > 0)
                {
                    string selectedUser = lvSalespersons.SelectedItems[0].Text;
                    if (DatabaseHelper.DeleteUser(selectedUser))
                    {
                        MessageBox.Show("Salesperson removed.");
                        LoadSalespersonList();
                    }
                }
            };
            staffCard.Controls.Add(btnDeleteSalesperson);

            // Load salespersons
            void LoadSalespersonList()
            {
                lvSalespersons.Items.Clear();
                var users = DatabaseHelper.GetUsersByRole("employee");
                foreach (var user in users)
                {
                    lvSalespersons.Items.Add(new ListViewItem(new[] { user.Username, user.Role }));
                }
            }
            LoadSalespersonList();

            // === Activity Section ===
            var activityCard = CreateCardPanel("Activity", 800, 260);
            mainPanel.Controls.Add(activityCard);

            var lblActivityLog = new Label()
            {
                Text = "Recent Activity",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = TextColor
            };
            activityCard.Controls.Add(lblActivityLog);

            lvActivityLog = new ListView()
            {
                Location = new Point(20, 50),
                Size = new Size(760, 150),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9)
            };
            lvActivityLog.Columns.Add("Date/Time", 180);
            lvActivityLog.Columns.Add("Activity", 400);
            lvActivityLog.Columns.Add("Location", 180);

            lvActivityLog.OwnerDraw = true;
            lvActivityLog.DrawColumnHeader += (sender, e) =>
            {
                e.DrawBackground();
                using var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                using var brush = new SolidBrush(TextColor);
                e.Graphics.DrawString(e.Header.Text, new Font("Segoe UI Semibold", 9), brush, e.Bounds, sf);
            };
            lvActivityLog.DrawItem += (sender, e) => e.DrawDefault = true;
            lvActivityLog.DrawSubItem += (sender, e) => e.DrawDefault = true;

            lvActivityLog.Items.Add(new ListViewItem(new[] { DateTime.Now.ToString("g"), "Logged in", "New York, US" }));
            lvActivityLog.Items.Add(new ListViewItem(new[] { DateTime.Now.AddMinutes(-30).ToString("g"), "Updated profile information", "New York, US" }));
            lvActivityLog.Items.Add(new ListViewItem(new[] { DateTime.Now.AddHours(-2).ToString("g"), "Changed password", "New York, US" }));
            lvActivityLog.Items.Add(new ListViewItem(new[] { DateTime.Now.AddDays(-1).ToString("g"), "Logged in from new device", "London, UK" }));
            activityCard.Controls.Add(lvActivityLog);

            // === Security Actions ===
            var securityCard = CreateCardPanel("Security Actions", 800, 80);
            mainPanel.Controls.Add(securityCard);

            btnLogoutAllDevices = CreateDangerButton("Logout All Devices", 20, 20, 180);
            btnLogoutAllDevices.Click += BtnLogoutAllDevices_Click;
            securityCard.Controls.Add(btnLogoutAllDevices);
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

            return panel;
        }


        private Control[] CreateFormInput(string labelText, int x, int y, int labelWidth, int inputWidth, int inputHeight)
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

            var txt = new TextBox()
            {
                Location = new Point(x + labelWidth + 10, y),
                Size = new Size(inputWidth, inputHeight),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };

            return new Control[] { lbl, txt };
        }

        private Control[] CreatePasswordInput(string labelText, int x, int y, int labelWidth, int inputWidth, int inputHeight)
        {
            var controls = CreateFormInput(labelText, x, y, labelWidth, inputWidth, inputHeight);
            var txtBox = (TextBox)controls[1];
            txtBox.UseSystemPasswordChar = true;
            return controls;
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

        private Button CreateDangerButton(string text, int x, int y, int width)
        {
            var btn = new Button()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 36),
                BackColor = DangerColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(185, 28, 28); // Red-700
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(153, 27, 27); // Red-800
            return btn;
        }

        private Image CreateDefaultProfileImage()
        {
            Bitmap bmp = new Bitmap(120, 120);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                // Draw circle background
                using (var brush = new SolidBrush(Color.FromArgb(229, 231, 235)))
                {
                    g.FillEllipse(brush, 0, 0, 120, 120);
                }

                // Draw user icon
                using (var brush = new SolidBrush(SecondaryTextColor))
                {
                    // Head
                    g.FillEllipse(brush, 40, 30, 40, 40);
                    // Body
                    g.FillRectangle(brush, 50, 70, 20, 30);
                }
            }
            return bmp;
        }

        private void BtnUploadPicture_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
            dlg.Title = "Select Profile Picture";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var original = Image.FromFile(dlg.FileName);
                    // Create a circular image
                    picProfile.Image = CropToCircle(original, Color.White);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Image CropToCircle(Image original, Color bgColor)
        {
            Bitmap circleImage = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(circleImage))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(bgColor);

                using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, original.Width, original.Height);
                    g.SetClip(path);
                    g.DrawImage(original, 0, 0);
                }
            }
            return circleImage;
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                ShowValidationError("Please enter your full name", txtFullName);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                ShowValidationError("Please enter a valid email address", txtEmail);
                return;
            }

            // Simulate save operation
            MessageBox.Show("Profile changes saved successfully", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowValidationError(string message, Control control)
        {
            MessageBox.Show(message, "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOldPassword.Text))
            {
                ShowValidationError("Please enter your current password", txtOldPassword);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNewPassword.Text) || txtNewPassword.Text.Length < 8)
            {
                ShowValidationError("Password must be at least 8 characters", txtNewPassword);
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                ShowValidationError("New passwords do not match", txtConfirmPassword);
                return;
            }

            // Simulate password change
            MessageBox.Show("Password changed successfully", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtOldPassword.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
        }

        private void BtnLogoutAllDevices_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("This will log you out from all devices. Continue?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("All devices have been logged out", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}