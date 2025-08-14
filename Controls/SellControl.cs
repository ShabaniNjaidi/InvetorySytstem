using InventorySystem.Helpers;
using InventorySystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorySystem
{
    public static class SessionData
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }
        public static string CurrentRole { get; set; }
    }

    public class CartItem
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
    }

    public class SellControl : UserControl
    {
        // UI
        private Label lblTitle, lblPrompt, lblProductInfo, lblSellResult, lblCartTotal, lblDailySummary;
        private TextBox txtSellBarcode;
        private Button btnSell, btnUndo, btnReceipt, btnCheckout;
        private Panel cardPanel;
        private DataGridView dgvProducts;
        private DataGridView dgvCart;
        private PictureBox picProductPreview;
        private NumericUpDown nudQuantity;
        private TextBox txtAmountPaid;
        private TextBox txtCustomerName;

        private ComboBox cmbPaymentMethod;
        private Label lblPaymentRef;
        private TextBox txtPaymentRef;
        private CheckBox chkOpenReceipt;

        public List<Product> Products { get; set; } = new List<Product>();

        private MainForm mainForm;

        private BindingList<Product> productsBinding = new BindingList<Product>();
        private BindingList<CartItem> cartBinding = new BindingList<CartItem>();
        private Dictionary<string, Product> productCache = new Dictionary<string, Product>();
        private Stack<CartItem> soldHistory = new Stack<CartItem>();

        private decimal dailyTotal = 0;
        private int dailyCount = 0;

        private System.Windows.Forms.Timer searchDebounceTimer;
        private bool isProcessing = false;

        public SellControl(MainForm form)
        {
            mainForm = form;
            InitializeUI();
            LoadProductsCache();
            LoadTodaySalesSummary();
            UpdateDailySummary();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColorTranslator.FromHtml("#F8FAFC");

            const int rightShift = 208;
            const int topShift = 76;
            const int picAdditionalRightShift = 114;

            cardPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12 + rightShift, 12 + topShift, 12, 12)
            };
            this.Controls.Add(cardPanel);

            lblTitle = new Label
            {
                Text = "POINT OF SALE",
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1E293B"),
                AutoSize = true,
                Location = new Point(10 + rightShift, 10 + topShift)
            };
            cardPanel.Controls.Add(lblTitle);

            lblPrompt = new Label
            {
                Text = "SCAN OR ENTER PRODUCT BARCODE",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorTranslator.FromHtml("#64748B"),
                AutoSize = true,
                Location = new Point(12 + rightShift, 52 + topShift)
            };
            cardPanel.Controls.Add(lblPrompt);

            txtSellBarcode = new TextBox
            {
                Width = 420,
                Height = 38,
                Font = new Font("Segoe UI", 12),
                Location = new Point(12 + rightShift, 74 + topShift),
                PlaceholderText = "Enter or scan barcode and press ENTER..."
            };
            txtSellBarcode.KeyDown += TxtSellBarcode_KeyDown;
            txtSellBarcode.TextChanged += TxtSellBarcode_TextChanged;
            cardPanel.Controls.Add(txtSellBarcode);

            nudQuantity = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1000,
                Value = 1,
                Location = new Point(txtSellBarcode.Right + 10, txtSellBarcode.Top),
                Width = 70
            };
            cardPanel.Controls.Add(nudQuantity);

            btnSell = new Button
            {
                Text = "SELL",
                Width = 90,
                Height = 38,
                BackColor = ColorTranslator.FromHtml("#D97706"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(nudQuantity.Right + 10, txtSellBarcode.Top)
            };
            btnSell.FlatAppearance.BorderSize = 0;
            btnSell.Click += (s, e) => SellProduct();
            cardPanel.Controls.Add(btnSell);

            btnUndo = new Button
            {
                Text = "⏪ UNDO",
                Width = 100,
                Height = 38,
                BackColor = ColorTranslator.FromHtml("#F43F5E"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(btnSell.Right + 8, btnSell.Top)
            };
            btnUndo.FlatAppearance.BorderSize = 0;
            btnUndo.Click += (s, e) => UndoSale();
            cardPanel.Controls.Add(btnUndo);

            lblProductInfo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = ColorTranslator.FromHtml("#334155"),
                Location = new Point(12 + rightShift, 120 + topShift)
            };
            cardPanel.Controls.Add(lblProductInfo);

            lblSellResult = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11),
                Location = new Point(12 + rightShift, 150 + topShift)
            };
            cardPanel.Controls.Add(lblSellResult);

            picProductPreview = new PictureBox
            {
                Size = new Size(220, 220),
                Location = new Point(cardPanel.Width - 240 - rightShift + picAdditionalRightShift, 74 + topShift),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            cardPanel.Controls.Add(picProductPreview);

            dgvProducts = new DataGridView
            {
                Location = new Point(12 + rightShift, 190 + topShift),
                Size = new Size(640, 220),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvProducts.CellClick += DgvProducts_CellClick;
            cardPanel.Controls.Add(dgvProducts);

            dgvCart = new DataGridView
            {
                Location = new Point(12 + rightShift, dgvProducts.Bottom + 8),
                Size = new Size(640, 140),
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgvCart.CellEndEdit += DgvCart_CellEndEdit;
            cardPanel.Controls.Add(dgvCart);

            lblCartTotal = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#0284C7"),
                Location = new Point(660 + rightShift, dgvProducts.Bottom + 10),
                AutoSize = true
            };
            cardPanel.Controls.Add(lblCartTotal);

            txtAmountPaid = new TextBox
            {
                Location = new Point(660 + rightShift, lblCartTotal.Bottom + 10),
                Width = 120,
                PlaceholderText = "Amount Paid"
            };
            // Add formatting to amount paid field
            txtAmountPaid.Leave += TxtAmountPaid_Leave;
            txtAmountPaid.Enter += TxtAmountPaid_Enter;
            cardPanel.Controls.Add(txtAmountPaid);

            cmbPaymentMethod = new ComboBox
            {
                Location = new Point(660 + rightShift, txtAmountPaid.Bottom + 8),
                Width = 120,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPaymentMethod.Items.AddRange(new object[] { "Cash", "MobileMoney", "Card" });
            cmbPaymentMethod.SelectedIndex = 0;
            cmbPaymentMethod.SelectedIndexChanged += (s, e) => TogglePaymentRefVisibility();
            cardPanel.Controls.Add(cmbPaymentMethod);

            lblPaymentRef = new Label
            {
                Text = "Ref #",
                AutoSize = true,
                Location = new Point(660 + rightShift, cmbPaymentMethod.Bottom + 8)
            };
            cardPanel.Controls.Add(lblPaymentRef);

            txtPaymentRef = new TextBox
            {
                Location = new Point(660 + rightShift, lblPaymentRef.Bottom + 2),
                Width = 200,
                PlaceholderText = "Txn Ref / Last 4"
            };
            cardPanel.Controls.Add(txtPaymentRef);

            TogglePaymentRefVisibility();

            btnCheckout = new Button
            {
                Text = "CHECKOUT",
                Width = 120,
                Height = 38,
                BackColor = ColorTranslator.FromHtml("#10B981"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(660 + rightShift, txtPaymentRef.Bottom + 8)
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += async (s, e) => await Checkout();
            cardPanel.Controls.Add(btnCheckout);

            btnReceipt = new Button
            {
                Text = "🧾 RECEIPT",
                Width = 120,
                Height = 38,
                BackColor = ColorTranslator.FromHtml("#2563EB"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Location = new Point(660 + rightShift, btnCheckout.Bottom + 8)
            };
            btnReceipt.FlatAppearance.BorderSize = 0;
            btnReceipt.Click += (s, e) => PrintReceiptDirect();
            cardPanel.Controls.Add(btnReceipt);

            txtCustomerName = new TextBox
            {
                Width = 200,
                Location = new Point(830 + rightShift, 300 + 228),
                PlaceholderText = "Enter Customer Name"
            };
            cardPanel.Controls.Add(txtCustomerName);

            chkOpenReceipt = new CheckBox
            {
                Text = "Open receipt after save",
                AutoSize = true,
                Checked = true,
                Location = new Point(txtCustomerName.Left, txtCustomerName.Bottom + 8)
            };
            cardPanel.Controls.Add(chkOpenReceipt);

            lblDailySummary = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = ColorTranslator.FromHtml("#475569"),
                Location = new Point(12 + rightShift, dgvCart.Bottom + 10)
            };
            cardPanel.Controls.Add(lblDailySummary);

            dgvProducts.DataSource = productsBinding;
            dgvCart.DataSource = cartBinding;

            FormatProductGrid();
            FormatCartGrid();

            searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 350 };
            searchDebounceTimer.Tick += (s, e) =>
            {
                searchDebounceTimer.Stop();
                SearchBarcode(txtSellBarcode.Text.Trim());
            };

            UpdateCartTotal();
            UpdateDailySummary();
        }

        private void TxtAmountPaid_Enter(object sender, EventArgs e)
        {
            // Remove commas when entering the field for editing
            txtAmountPaid.Text = txtAmountPaid.Text.Replace(",", "");
        }

        private void TxtAmountPaid_Leave(object sender, EventArgs e)
        {
            // Format with commas when leaving the field
            if (decimal.TryParse(txtAmountPaid.Text, out decimal value))
            {
                txtAmountPaid.Text = value.ToString("N0");
            }
        }

        private void ClearInputFields()
        {
            txtAmountPaid.Text = "";
            txtPaymentRef.Text = "";
            txtCustomerName.Text = "";
            cmbPaymentMethod.SelectedIndex = 0;
            TogglePaymentRefVisibility();
        }

        private void TogglePaymentRefVisibility()
        {
            bool needsRef = (cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash") != "Cash";
            lblPaymentRef.Visible = needsRef;
            txtPaymentRef.Visible = needsRef;
        }

        private void PrintReceiptDirect()
        {
            try
            {
                var receiptItems = new List<ReceiptItem>();

                foreach (DataGridViewRow row in dgvCart.Rows)
                {
                    if (row.IsNewRow) continue;

                    string name = row.Cells["Name"]?.Value?.ToString() ?? "Unknown";
                    int qty = Convert.ToInt32(row.Cells["Quantity"]?.Value ?? 1);
                    decimal price = Convert.ToDecimal(row.Cells["Price"]?.Value ?? 0);

                    receiptItems.Add(new ReceiptItem { Name = name, Qty = qty, Price = price });
                }

                int currentUserId = GetCurrentUserId();
                var shopInfo = DatabaseHelper.GetUserShopInfo(currentUserId);

                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Receipts");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string fullPath = Path.Combine(folderPath, fileName);
                string customerName = txtCustomerName.Text.Trim();

                ReceiptGenerator.GenerateReceipt(
                    receiptItems,
                    vatRate: 0,
                    filePath: fullPath,
                    shopName: shopInfo?.ShopName ?? "Unknown Shop",
                    tagline: shopInfo?.Tagline ?? "",
                    address: shopInfo?.Address ?? "",
                    contactInfo: shopInfo?.ContactInfo ?? "",
                    customerName: customerName
                );

                if (chkOpenReceipt.Checked)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = fullPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating receipt: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetCurrentUserId()
        {
            return SessionData.CurrentUserId;
        }

        private void FormatProductGrid()
        {
            dgvProducts.Columns.Clear();
            dgvProducts.AutoGenerateColumns = false;

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barcode", HeaderText = "Barcode", DataPropertyName = "Barcode" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", DataPropertyName = "Name" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price (TZS)", DataPropertyName = "Price", DefaultCellStyle = { Format = "N2" } });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Stock", DataPropertyName = "Quantity" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category", DataPropertyName = "Category" });

            // Highlight low stock items
            dgvProducts.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == dgvProducts.Columns["Quantity"].Index && e.RowIndex >= 0)
                {
                    var qty = Convert.ToInt32(e.Value);
                    if (qty <= 0)
                    {
                        e.CellStyle.BackColor = Color.LightPink;
                        e.CellStyle.ForeColor = Color.DarkRed;
                    }
                    else if (qty < 5)
                    {
                        e.CellStyle.BackColor = Color.LightYellow;
                    }
                }
            };
        }

        private void FormatCartGrid()
        {
            dgvCart.Columns.Clear();
            dgvCart.AutoGenerateColumns = false;

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barcode", HeaderText = "Barcode", DataPropertyName = "Barcode", ReadOnly = true });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", DataPropertyName = "Name", ReadOnly = true });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Price", DataPropertyName = "Price", ReadOnly = true, DefaultCellStyle = { Format = "N2" } });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity", DataPropertyName = "Quantity" });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "SubTotal", HeaderText = "Subtotal", DataPropertyName = "SubTotal", ReadOnly = true, DefaultCellStyle = { Format = "N2" } });
        }

        private void LoadProductsCache()
        {
            productsBinding.Clear();
            productCache.Clear();

            var list = DatabaseHelper.LoadAllProducts() ?? new List<Product>();
            foreach (var p in list)
            {
                productsBinding.Add(p);
                var key = (p.Barcode ?? "").Trim().ToLower();
                if (!productCache.ContainsKey(key))
                    productCache[key] = p;
            }
        }

        private void RefreshProductsFromDb()
        {
            LoadProductsCache();
        }

        private void TxtSellBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SellProduct();
            }
        }

        private void TxtSellBarcode_TextChanged(object sender, EventArgs e)
        {
            searchDebounceTimer.Stop();
            searchDebounceTimer.Start();
        }

        private void SearchBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                lblProductInfo.Text = "";
                picProductPreview.Image = null;
                return;
            }

            var key = barcode.Trim().ToLower();
            if (productCache.TryGetValue(key, out var product))
            {
                ShowProductPreview(product);
                HighlightProductRow(product.Barcode);
                lblProductInfo.Text = $"📦 {product.Name} | Price: TZS {product.Price:N2} | Stock: {product.Quantity}";
                lblProductInfo.ForeColor = ColorTranslator.FromHtml("#2563EB");

                // Enable/disable sell button based on stock
                btnSell.Enabled = product.Quantity > 0;
            }
            else
            {
                lblProductInfo.Text = "⚠️ Product not found";
                lblProductInfo.ForeColor = ColorTranslator.FromHtml("#DC2626");
                picProductPreview.Image = null;
                dgvProducts.ClearSelection();
                btnSell.Enabled = false;
            }
        }

        private void ShowProductPreview(Product product)
        {
            try
            {
                string imageFileName = Path.GetFileName(product.ImagePath ?? "");
                string fullPath = Path.Combine(mainForm.ImageFolderPath, imageFileName);
                if (File.Exists(fullPath))
                {
                    using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        picProductPreview.Image = Image.FromStream(fs);
                    }
                }
                else
                {
                    picProductPreview.Image = null;
                }
            }
            catch
            {
                picProductPreview.Image = null;
            }
        }

        private void HighlightProductRow(string barcode)
        {
            if (string.IsNullOrEmpty(barcode)) return;
            for (int i = 0; i < dgvProducts.Rows.Count; i++)
            {
                var row = dgvProducts.Rows[i];
                var cellVal = (row.Cells[0].Value ?? "").ToString().Trim().ToLower();
                if (cellVal == barcode.Trim().ToLower())
                {
                    dgvProducts.ClearSelection();
                    row.Selected = true;
                    dgvProducts.FirstDisplayedScrollingRowIndex = Math.Max(0, i - 3);
                    return;
                }
            }
            dgvProducts.ClearSelection();
        }

        private void DgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvProducts.Rows[e.RowIndex];
            string barcode = (row.Cells[0].Value ?? "").ToString();
            txtSellBarcode.Text = barcode;
            nudQuantity.Value = 1;
            SearchBarcode(barcode);
        }

        private void DgvCart_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var item = cartBinding[e.RowIndex];
            if (item.Quantity < 1) item.Quantity = 1;
            UpdateCartTotal();
            dgvCart.Refresh();
        }

        private void SellProduct()
        {
            if (isProcessing) return;

            try
            {
                isProcessing = true;
                LockUI(true);

                string barcode = txtSellBarcode.Text.Trim();
                if (string.IsNullOrEmpty(barcode)) return;

                int qty = (int)nudQuantity.Value;
                var key = barcode.ToLower();

                if (!productCache.TryGetValue(key, out var product))
                {
                    RefreshProductsFromDb();
                    productCache.TryGetValue(key, out product);
                }

                if (product == null)
                {
                    ShowStatus("❌ Product not found", ColorTranslator.FromHtml("#DC2626"));
                    return;
                }

                if (product.Quantity <= 0)
                {
                    ShowStatus("❌ Product out of stock", ColorTranslator.FromHtml("#DC2626"));
                    return;
                }

                if (product.Quantity < qty)
                {
                    ShowStatus($"⚠️ Only {product.Quantity} available in stock", ColorTranslator.FromHtml("#D97706"));
                    return;
                }

                if (qty < 1)
                {
                    ShowStatus("⚠️ Quantity must be at least 1", ColorTranslator.FromHtml("#D97706"));
                    return;
                }

                AddToCart(product, qty);

                soldHistory.Push(new CartItem
                {
                    Barcode = product.Barcode,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = qty
                });

                ShowStatus($"✅ Added to cart: {product.Name} x{qty}", ColorTranslator.FromHtml("#059669"));
                FlashSuccess();

                UpdateCartTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selling product: " + ex.Message);
            }
            finally
            {
                isProcessing = false;
                LockUI(false);
                txtSellBarcode.SelectAll();
                txtSellBarcode.Focus();
            }
        }

        private void AddToCart(Product product, int qty)
        {
            var existing = cartBinding.FirstOrDefault(c => (c.Barcode ?? "").Trim().ToLower() == (product.Barcode ?? "").Trim().ToLower());
            if (existing == null)
            {
                cartBinding.Add(new CartItem
                {
                    Barcode = product.Barcode,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = qty
                });
            }
            else
            {
                existing.Quantity += qty;
                dgvCart.Refresh();
            }
        }

        private void UndoSale()
        {
            if (!soldHistory.Any())
            {
                ShowStatus("⚠️ Nothing to undo", ColorTranslator.FromHtml("#D97706"));
                return;
            }

            var last = soldHistory.Pop();

            var cartItem = cartBinding.FirstOrDefault(c => (c.Barcode ?? "").Trim().ToLower() == (last.Barcode ?? "").Trim().ToLower());
            if (cartItem != null)
            {
                cartItem.Quantity -= last.Quantity;
                if (cartItem.Quantity <= 0) cartBinding.Remove(cartItem);
                dgvCart.Refresh();
            }

            ShowStatus($"⏪ Removed from cart: {last.Name} x{last.Quantity}", ColorTranslator.FromHtml("#FB923C"));
            UpdateCartTotal();
        }

        private void UpdateCartTotal()
        {
            decimal total = cartBinding.Sum(c => c.SubTotal);
            lblCartTotal.Text = $"🛒 Cart Total: {total:N2} TZS";
        }

        private void UpdateDailySummary()
        {
            lblDailySummary.Text = $"💰 Today's Sales: {dailyCount} item(s), {dailyTotal:N2} TZS";
        }

        private void LoadTodaySalesSummary()
        {
            var todaySales = DatabaseHelper.GetTodaySales() ?? new List<Sale>();
            try
            {
                dailyTotal = todaySales.Sum(s => s.TotalAmount);
                dailyCount = todaySales.Sum(s => s.QuantitySold);
            }
            catch
            {
                dailyTotal = 0;
                dailyCount = 0;
            }
        }

        private void LockUI(bool lockit)
        {
            btnSell.Enabled = !lockit;
            btnUndo.Enabled = !lockit;
            btnCheckout.Enabled = !lockit;
            btnReceipt.Enabled = !lockit;
            txtSellBarcode.Enabled = !lockit;
            nudQuantity.Enabled = !lockit;
        }

        private async Task Checkout()
        {
            if (!cartBinding.Any())
            {
                MessageBox.Show("Cart is empty.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Parse amount paid with comma formatting
            var paidText = txtAmountPaid.Text.Replace(",", "");
            if (!decimal.TryParse(paidText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal paid))
            {
                MessageBox.Show("Enter a valid amount paid.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (paid < 0)
            {
                MessageBox.Show("Amount paid cannot be negative.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            decimal total = cartBinding.Sum(c => c.SubTotal);
            decimal discount = 0;
            decimal change = 0;

            // Calculate discount if paid is less than total
            if (paid < total)
            {
                discount = total - paid;
                change = 0;
            }
            else
            {
                discount = 0;
                change = paid - total;
            }

            string paymentMethod = (cmbPaymentMethod.SelectedItem?.ToString() ?? "Cash");
            string paymentRef = txtPaymentRef.Text.Trim();
            if (paymentMethod != "Cash" && string.IsNullOrWhiteSpace(paymentRef))
            {
                MessageBox.Show("Provide the payment reference for non-cash payments.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string customerName = txtCustomerName.Text.Trim();
            int userId = GetCurrentUserId();

            var lineItems = cartBinding.Select(c => new DatabaseHelper.SaleItemDto
            {
                Barcode = c.Barcode,
                Name = c.Name,
                Quantity = c.Quantity,
                Price = c.Price
            }).ToList();

            try
            {
                isProcessing = true;
                LockUI(true);

                // Pass discount to the transaction
                long transactionId = await DatabaseHelper.FinalizeTransactionAsync(
                    DateTime.Now, userId, customerName, paymentMethod, paymentRef,
                    total, paid, change, discount, lineItems
                );

                var receiptItems = cartBinding.Select(c => new ReceiptItem
                {
                    Name = c.Name,
                    Qty = c.Quantity,
                    Price = c.Price
                }).ToList();

                var shopInfo = DatabaseHelper.GetUserShopInfo(userId);
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Receipts");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Receipt_{transactionId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string fullPath = Path.Combine(folderPath, fileName);

                ReceiptGenerator.GenerateReceipt(
    receiptItems,
    vatRate: 0,
    filePath: fullPath,
    shopName: shopInfo?.ShopName ?? "Unknown Shop",
    tagline: shopInfo?.Tagline ?? "",
    address: shopInfo?.Address ?? "",
    contactInfo: shopInfo?.ContactInfo ?? "",
    customerName: customerName,
    discount: discount,
    paid: paid,
    change: change,
    paymentMethod: paymentMethod,
    paymentRef: paymentRef
);

                if (chkOpenReceipt.Checked)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = fullPath,
                        UseShellExecute = true
                    });
                }

                int itemsCount = cartBinding.Sum(c => c.Quantity);
                dailyTotal += total;
                dailyCount += itemsCount;

                // Clear cart and input fields
                cartBinding.Clear();
                soldHistory.Clear();
                ClearInputFields();
                UpdateCartTotal();
                UpdateDailySummary();

                RefreshProductsFromDb();

                ShowStatus($"✅ Checkout complete. Change: {change:N2} TZS", ColorTranslator.FromHtml("#059669"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Checkout failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isProcessing = false;
                LockUI(false);
            }
        }

        private string BuildReceiptString(decimal total, decimal paid, decimal change)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("RECEIPT");
            sb.AppendLine("----------------------------");
            foreach (var it in cartBinding)
            {
                sb.AppendLine($"{it.Name} x{it.Quantity} - {it.SubTotal:N2} TZS");
            }
            sb.AppendLine("----------------------------");
            sb.AppendLine($"Total: {total:N2} TZS");
            sb.AppendLine($"Paid: {paid:N2} TZS");
            sb.AppendLine($"Change: {change:N2} TZS");
            sb.AppendLine($"Date: {DateTime.Now}");
            return sb.ToString();
        }

        private void GeneratePdfReceipt(string receiptText)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (sender, args) =>
            {
                args.Graphics.DrawString(receiptText,
                    new Font("Segoe UI", 10),
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

        private async void FlashSuccess()
        {
            var original = cardPanel.BackColor;
            cardPanel.BackColor = ColorTranslator.FromHtml("#D1FAE5");
            await Task.Delay(250);
            cardPanel.BackColor = original;
        }

        private void ShowStatus(string message, Color color)
        {
            lblSellResult.Text = message;
            lblSellResult.ForeColor = color;
        }
    }
}