using InventorySystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;

namespace InventorySystem
{

    public class SellControl : UserControl
    {
        private Label lblTitle, lblPrompt, lblProductInfo, lblSellResult, lblCartTotal, lblDailySummary;
        private TextBox txtSellBarcode;
        private Button btnSell, btnUndo, btnReceipt;
        private Panel cardPanel;
        private DataGridView dgvProducts; // Added DataGridView for product list
        private PictureBox picProductPreview;
        private MainForm mainForm;


        private List<Product> Cart = new List<Product>();
        private Stack<Product> SoldHistory = new Stack<Product>();

        private decimal dailyTotal = 0;
        private int dailyCount = 0;

        public List<Product> Products { get; set; } = new List<Product>();

        private bool isRefreshing = false; // NEW
        private List<Product> fullProductList = new List<Product>(); // NEW

        public SellControl(MainForm form)
        {
            mainForm = form; // now you can access imageFolderPath
            InitializeUI();
            RefreshProductList();
            LoadTodaySalesSummary(); // <- ✅ THIS IS WHERE TO PASTE
            UpdateDailySummary();    // <- ✅ THIS TOO
        }


        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColorTranslator.FromHtml("#F8FAFC");

            int leftOffset = 226;
            int topOffset = 76;

            var mainContainer = new Panel
            {
                BackColor = Color.Transparent,
                Location = new Point(leftOffset, topOffset),
                Size = new Size(1020, 520)

            };
            this.Controls.Add(mainContainer);

            lblTitle = new Label
            {
                Text = "POINT OF SALE",
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1E293B"),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            mainContainer.Controls.Add(lblTitle);

            cardPanel = new Panel
            {
                Location = new Point(0, 50),
                Size = new Size(700, 430),
                BackColor = Color.White,
                Padding = new Padding(30)
            };
            mainContainer.Controls.Add(cardPanel);

            cardPanel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, cardPanel.ClientRectangle,
                    Color.FromArgb(30, 0, 0, 0), ButtonBorderStyle.Solid);
            };

            lblPrompt = new Label
            {
                Text = "SCAN OR ENTER PRODUCT BARCODE",
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#64748B"),
                AutoSize = true,
                Location = new Point(30, 10)
            };
            cardPanel.Controls.Add(lblPrompt);

            txtSellBarcode = new TextBox
            {
                Width = 370,
                Height = 40,
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Enter barcode here...",
                Location = new Point(30, 40)
            };
            txtSellBarcode.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    SellProduct();
                }
            };
            cardPanel.Controls.Add(txtSellBarcode);

            btnSell = new Button
            {
                Text = "SELL",
                Width = 90,
                Height = 40,
                BackColor = ColorTranslator.FromHtml("#D97706"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(txtSellBarcode.Right + 10, txtSellBarcode.Top)
            };
            btnSell.FlatAppearance.BorderSize = 0;
            btnSell.Click += (s, e) => SellProduct();
            cardPanel.Controls.Add(btnSell);

            btnUndo = new Button
            {
                Text = "⏪ UNDO",
                Width = 100,
                Height = 40,
                BackColor = ColorTranslator.FromHtml("#F43F5E"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(btnSell.Right + 10, btnSell.Top)
            };
            btnUndo.FlatAppearance.BorderSize = 0;
            btnUndo.Click += (s, e) => UndoSale();
            cardPanel.Controls.Add(btnUndo);

            lblProductInfo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                ForeColor = ColorTranslator.FromHtml("#334155"),
                Location = new Point(30, 100)
            };
            cardPanel.Controls.Add(lblProductInfo);

            lblSellResult = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 12),
                Location = new Point(30, 140)
            };
            cardPanel.Controls.Add(lblSellResult);

            picProductPreview = new PictureBox
            {
                Size = new Size(260, 430), // Stretch to match SellControl height
                Location = new Point(cardPanel.Right + 20, cardPanel.Top), // Align top with cardPanel
                BorderStyle = BorderStyle.None, // Remove border
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            mainContainer.Controls.Add(picProductPreview);



            lblCartTotal = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#0284C7"),
                Location = new Point(30, 180),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblCartTotal);

            btnReceipt = new Button
            {
                Text = "🧾 RECEIPT",
                Width = 130,
                Height = 45,
                BackColor = ColorTranslator.FromHtml("#10B981"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(30, 220)
            };
            btnReceipt.FlatAppearance.BorderSize = 0;
            btnReceipt.Click += (s, e) => ShowReceipt();
            cardPanel.Controls.Add(btnReceipt);

            lblDailySummary = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = ColorTranslator.FromHtml("#475569"),
                Location = new Point(30, 280)
            };
            cardPanel.Controls.Add(lblDailySummary);

            txtSellBarcode.TextChanged += TxtSellBarcode_TextChanged;

            // --- Add the DataGridView to display products ---
            dgvProducts = new DataGridView
            {
                Location = new Point(30, 320),
                Size = new Size(640, 100),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            cardPanel.Controls.Add(dgvProducts);

            UpdateCartTotal();
            UpdateDailySummary();
        }

        public void RefreshProductList()
        {
            if (dgvProducts == null) // safety check
                return;

            isRefreshing = true;

            // Reload fresh products from database
            Products = InventorySystem.Helpers.DatabaseHelper.LoadAllProducts();

            fullProductList = new List<Product>(Products); // sync full list

            dgvProducts.DataSource = null;
            dgvProducts.DataSource = fullProductList.ToList();

            if (dgvProducts.Columns.Count > 0)
            {
                FormatGrid();
            }

            dgvProducts.PerformLayout();
            dgvProducts.Refresh();

            isRefreshing = false;
        }

        private void FormatGrid()
        {
            if (dgvProducts.Columns["Barcode"] != null)
                dgvProducts.Columns["Barcode"].HeaderText = "Barcode";

            if (dgvProducts.Columns["Name"] != null)
                dgvProducts.Columns["Name"].HeaderText = "Product Name";

            if (dgvProducts.Columns["Price"] != null)
            {
                dgvProducts.Columns["Price"].HeaderText = "Price (TZS)";
                dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N2";
            }

            if (dgvProducts.Columns["Quantity"] != null)
                dgvProducts.Columns["Quantity"].HeaderText = "Stock";

            if (dgvProducts.Columns["Category"] != null)
                dgvProducts.Columns["Category"].HeaderText = "Category";

            if (dgvProducts.Columns["ImagePath"] != null)
                dgvProducts.Columns["ImagePath"].Visible = false; // 👈 Hide image path
        }


        private void SellProduct()
        {
            string inputBarcode = txtSellBarcode.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(inputBarcode))
                return;

            RefreshProductList();

            var product = Products.FirstOrDefault(p => (p.Barcode ?? "").Trim().ToLower() == inputBarcode);

            if (product != null && product.Quantity > 0)
            {
                // Reduce quantity in memory and DB
                product.Quantity--;
                InventorySystem.Helpers.DatabaseHelper.UpdateProductQuantity(product.Barcode, product.Quantity);

                // Save to Sales table
                InventorySystem.Helpers.DatabaseHelper.SaveSale(
                    product.Barcode,
                    product.Name,
                    1, // Sold one item
                    product.Price
                );

                var cartItem = Cart.FirstOrDefault(p => (p.Barcode ?? "").Trim().ToLower() == inputBarcode);
                if (cartItem == null)
                {
                    Cart.Add(new Product
                    {
                        Barcode = product.Barcode,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = 1
                    });
                }
                else
                {
                    cartItem.Quantity++;
                }

                SoldHistory.Push(new Product
                {
                    Barcode = product.Barcode,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1
                });

                dailyTotal += product.Price;
                dailyCount++;

                lblSellResult.Text = $"✅ Sold: {product.Name}";
                lblSellResult.ForeColor = ColorTranslator.FromHtml("#059669");
                FlashSuccess();

                UpdateCartTotal();
                UpdateDailySummary();
            }
            else if (product != null)
            {
                lblSellResult.Text = $"⚠️ Out of stock: {product.Name}";
                lblSellResult.ForeColor = ColorTranslator.FromHtml("#D97706");
            }
            else
            {
                lblSellResult.Text = "❌ Product not found";
                lblSellResult.ForeColor = ColorTranslator.FromHtml("#DC2626");
            }

            lblSellResult.Left = (cardPanel.Width - lblSellResult.Width) / 2;
            txtSellBarcode.SelectAll();
            txtSellBarcode.Focus();
        }


        private void UndoSale()
        {
            if (SoldHistory.Any())
            {
                var lastSold = SoldHistory.Pop();

                // Restore quantity to inventory
                var product = Products.FirstOrDefault(p => p.Barcode == lastSold.Barcode);
                if (product != null)
                {
                    product.Quantity += 1;
                    InventorySystem.Helpers.DatabaseHelper.UpdateProductQuantity(product.Barcode, product.Quantity);
                }

                // Update cart
                var cartItem = Cart.FirstOrDefault(p => p.Barcode == lastSold.Barcode);
                if (cartItem != null)
                {
                    cartItem.Quantity--;
                    if (cartItem.Quantity <= 0)
                        Cart.Remove(cartItem);
                }

                // Delete sale from database (most recent match by barcode)
                InventorySystem.Helpers.DatabaseHelper.DeleteLastSaleByBarcode(lastSold.Barcode);

                dailyTotal -= lastSold.Price;
                dailyCount--;

                lblSellResult.Text = $"⏪ Undone: {lastSold.Name} (Sale deleted)";
                lblSellResult.ForeColor = ColorTranslator.FromHtml("#FB923C");
                UpdateCartTotal();
                UpdateDailySummary();
            }
            else
            {
                lblSellResult.Text = "⚠️ Nothing to undo";
                lblSellResult.ForeColor = ColorTranslator.FromHtml("#D97706");
            }

            lblSellResult.Left = (cardPanel.Width - lblSellResult.Width) / 2;
            txtSellBarcode.Focus();
        }


        private void ShowReceipt()
        {
            if (!Cart.Any())
            {
                MessageBox.Show("No products in cart.", "Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Generate receipt string
            string receipt = "🧾 RECEIPT\n----------------------\n";
            foreach (var item in Cart)
            {
                receipt += $"{item.Name} x{item.Quantity} - {item.Price * item.Quantity:N2} TZS\n";
            }
            receipt += "----------------------\n";
            decimal total = Cart.Sum(p => p.Price * p.Quantity);
            receipt += $"Total: {total:N2} TZS\n";
            receipt += $"Date: {DateTime.Now}\n";

            DialogResult res = MessageBox.Show(receipt, "Receipt Preview", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (res == DialogResult.OK)
            {
                GeneratePdfReceipt(receipt);

                Cart.Clear();
                SoldHistory.Clear();

                UpdateCartTotal();
                lblSellResult.Text = "✅ Sale completed, cart cleared";
                lblSellResult.ForeColor = ColorTranslator.FromHtml("#059669");
                UpdateDailySummary();
            }

            txtSellBarcode.Focus();
        }

        private void GeneratePdfReceipt(string receiptText)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (sender, args) =>
            {
                args.Graphics.DrawString(receiptText,
                    new Font("Segoe UI", 12),
                    Brushes.Black,
                    new RectangleF(10, 10, args.PageBounds.Width - 20, args.PageBounds.Height - 20));
            };

            try
            {
                PrintPreviewDialog preview = new PrintPreviewDialog
                {
                    Document = pd,
                    Width = 600,
                    Height = 800
                };
                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing receipt: {ex.Message}");
            }
        }

        private void TxtSellBarcode_TextChanged(object sender, EventArgs e)
        {
            string barcode = txtSellBarcode.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(barcode))
            {
                lblProductInfo.Text = "";
                picProductPreview.Image = null; // Clear preview
                return;
            }

            RefreshProductList(); // Ensure latest data

            var product = Products.FirstOrDefault(p => (p.Barcode ?? "").Trim().ToLower() == barcode);

            if (product != null)
            {
                lblProductInfo.Text = $"📦 {product.Name} | Price: TZS {product.Price:N2} | Stock: {product.Quantity}";
                lblProductInfo.ForeColor = ColorTranslator.FromHtml("#2563EB");

                // ✅ Build full image path using MainForm's image folder
                string imageFileName = Path.GetFileName(product.ImagePath ?? "");
                string fullPath = Path.Combine(mainForm.ImageFolderPath, imageFileName);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                        {
                            picProductPreview.Image = Image.FromStream(fs);
                        }
                    }
                    catch
                    {
                        picProductPreview.Image = null;
                    }
                }
                else
                {
                    picProductPreview.Image = null;
                }
            }
            else
            {
                lblProductInfo.Text = "⚠️ Product not found";
                lblProductInfo.ForeColor = ColorTranslator.FromHtml("#DC2626");
                picProductPreview.Image = null;
            }

            lblProductInfo.Left = Math.Max(0, (cardPanel.Width - lblProductInfo.Width) / 2);
        }



        private void UpdateCartTotal()
        {
            decimal total = Cart.Sum(p => p.Price * p.Quantity);
            lblCartTotal.Text = $"🛒 Cart Total: {total:N2} TZS";
        }

        private void UpdateDailySummary()
        {
            lblDailySummary.Text = $"💰 Today's Sales: {dailyCount} item(s), {dailyTotal:N2} TZS";
        }

        private async void FlashSuccess()
        {
            var original = cardPanel.BackColor;
            cardPanel.BackColor = ColorTranslator.FromHtml("#D1FAE5");
            await Task.Delay(300);
            cardPanel.BackColor = original;
        }
        private void LoadTodaySalesSummary()
        {
            var todaySales = InventorySystem.Helpers.DatabaseHelper.GetTodaySales();
            dailyTotal = todaySales.Sum(s => s.TotalAmount);
            dailyCount = todaySales.Sum(s => s.QuantitySold);
        }

    }
}
