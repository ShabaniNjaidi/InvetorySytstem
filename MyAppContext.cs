using InventorySystem;
using InvetorySytstem;
using System;
using System.Windows.Forms;

namespace InventorySystem
{
    public class MyAppContext : ApplicationContext
    {
        private WelcomeForm? welcomeForm;
        private LoginForm? loginForm;
        private Form? dashboardForm;

        public MyAppContext()
        {
            ShowWelcome();
        }

        private void ShowWelcome()
        {
            welcomeForm = new WelcomeForm();
            welcomeForm.RoleSelected += OnRoleSelected;

            welcomeForm.FormClosed += (s, e) =>
            {
                // Exit only if neither login nor dashboard forms are open
                if (loginForm == null && dashboardForm == null)
                    ExitThread();
            };

            welcomeForm.Show();
        }

        private void OnRoleSelected(object? sender, string selectedRole)
        {
            if (welcomeForm == null) return;

            // Unsubscribe to avoid multiple calls
            welcomeForm.RoleSelected -= OnRoleSelected;

            // Hide WelcomeForm instead of closing it for smooth transition
            welcomeForm.Hide();

            // Pass the selected role string to LoginForm constructor
            ShowLogin(selectedRole);
        }

        private void ShowLogin(string roleFromWelcome)
        {
            // Pass the role to LoginForm constructor (e.g. "admin", "employee")
            loginForm = new LoginForm(roleFromWelcome);

            loginForm.LoginSuccessful += OnLoginSuccessful;

            loginForm.FormClosed += (s, e) =>
            {
                // Exit app if no dashboard open
                if (dashboardForm == null)
                    ExitThread();
            };

            loginForm.Show();
        }

        private void OnLoginSuccessful(object? sender, (string username, string role) args)
        {
            string username = args.username;
            string role = args.role.ToLower();

            // Hide login form to prevent accidental close exit
            loginForm?.Hide();
            loginForm = null;

            try
            {
                ShowDashboard(username, role);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open dashboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void ShowDashboard(string username, string dbRole)
        {
            if (dbRole == "admin")
            {
                // Show MainForm with Owner privileges
                dashboardForm = new MainForm("Owner");
            }
            else if (dbRole == "employee")
            {
                // Show SellForm for employee with limited access
                dashboardForm = new SellForm(new MainForm("Salesperson"));
            }
            else
            {
                throw new Exception($"Unknown role: {dbRole}");
            }

            dashboardForm.FormClosed += (s, e) =>
            {
                // Exit app when dashboard is closed
                ExitThread();
            };

            dashboardForm.Show();
        }
    }
}
