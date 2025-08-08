using InventorySystem.Helpers;
using System;
using System.Windows.Forms;

namespace InventorySystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Initialize database before app launch
            DatabaseHelper.InitializeDatabase();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ✅ Launch the application using custom context
            Application.Run(new MyAppContext());
        }
    }
}
