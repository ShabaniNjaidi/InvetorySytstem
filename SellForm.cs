using InventorySystem;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace InvetorySytstem
{
    public partial class SellForm : Form
    {
        private MainForm mainForm;
        private SellControl sellControl;

        public SellForm(MainForm form)
        {
            InitializeComponent(); // This will use the one from SellForm.Designer.cs
            this.mainForm = form;

            this.Text = "Sell - Inventory System";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(800, 600);

            sellControl = new SellControl(mainForm);
            sellControl.Dock = DockStyle.Fill;
            this.Controls.Add(sellControl);
        }
    }
}
