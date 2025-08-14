using InventorySystem.Helpers;
using InventorySystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Helpers;


namespace InventorySystem
{
    public class ProductControl : UserControl
    {
        private Label lblTitle;

        // UI Components
        private Panel mainContainer, formContainer, dataContainer;
        private MaterialCard formCard, searchCard, dataCard;
        private TextBox txtBarcode, txtName, txtPrice, txtQuantity, txtSearch;
        private ComboBox cmbCategory;
        private Button btnAddProduct;
        private DataGridView dgvProducts;
        private Button btnRefillStock;
        private Button btnUploadImage;
        private PictureBox picProductImage;
        private string selectedImagePath = "";



        // Product list
        public List<Product> Products { get; set; } = new List<Product>();

        // Backup full list for search filtering
        private List<Product> fullProductList = new List<Product>();

        // Flag to avoid recursive or re-entrant updates
        private bool isRefreshing = false;

        public ProductControl()
        {
            InitializeUI();
            this.DoubleBuffered = true;

            // ✅ Load from SQLite DB after UI is ready
            Products = InventorySystem.Helpers.DatabaseHelper.LoadAllProducts();
            fullProductList = new List<Product>(Products);
            RefreshProductList();

            AdjustLayout();

            if (dataCard != null && dataCard.Height < 300)
                dataCard.Height = 300;

            this.Resize += (s, e) =>
            {
                AdjustLayout();
                if (dataCard != null && dataCard.Height < 300)
                    dataCard.Height = 300;
            };
        }


        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = ColorTranslator.FromHtml("#FAFAFA");

            // Main title
            lblTitle = new Label
            {
                Text = "PRODUCT MANAGEMENT",
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1E293B"),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            // Containers
            mainContainer = new Panel { BackColor = Color.Transparent };
            this.Controls.Add(mainContainer);

            formContainer = new Panel
            {
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            mainContainer.Controls.Add(formContainer);

            dataContainer = new Panel
            {
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            mainContainer.Controls.Add(dataContainer);

            // Form Card
            formCard = new MaterialCard
            {
                Text = "PRODUCT DETAILS",
                BackColor = Color.White,
                ShadowDepth = 8,
                Radius = 12
            };
            formContainer.Controls.Add(formCard);

            // Form labels
            AddFormLabel("BARCODE", 60);
            AddFormLabel("PRODUCT NAME", 130);
            AddFormLabel("PRICE (TZS)", 200);
            AddFormLabel("QUANTITY", 270);
            AddFormLabel("CATEGORY", 340);

            // Form fields
            txtBarcode = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            formCard.Controls.Add(txtBarcode);

            txtName = new TextBox
            {
                Location = new Point(20, 150),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 10)
            };
            formCard.Controls.Add(txtName);

            txtPrice = new TextBox
            {
                Location = new Point(20, 220),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 10)
            };
            formCard.Controls.Add(txtPrice);

            txtQuantity = new TextBox
            {
                Location = new Point(20, 290),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 10)
            };
            formCard.Controls.Add(txtQuantity);

            cmbCategory = new ComboBox
            {
                Location = new Point(20, 360),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            // Category + Add Category panel
            Panel categoryRow = new Panel
            {
                Location = new Point(20, 380), // just above the button row
                Size = new Size(360, 35),
                BackColor = Color.Transparent
            };

            // Category dropdown
            cmbCategory = new ComboBox
            {
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cmbCategory.Items.Clear();
            var categories = DatabaseHelper.LoadAllCategories();
            cmbCategory.Items.AddRange(categories.ToArray());

            // Add category button
            Button btnAddCategory = new Button
            {
                Text = "+",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(35, 30),
                BackColor = ColorTranslator.FromHtml("#D1FAE5"),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddCategory.FlatAppearance.BorderSize = 0;
            btnAddCategory.Click += BtnAddCategory_Click;

            // Add to category row
            categoryRow.Controls.Add(cmbCategory);
            cmbCategory.Location = new Point(0, 0);
            categoryRow.Controls.Add(btnAddCategory);
            btnAddCategory.Location = new Point(cmbCategory.Right + 5, 0);

            // Add category row to formCard
            formCard.Controls.Add(categoryRow);



            // Add Product Button
            btnAddProduct = new Button
            {
                Text = "ADD PRODUCT",
                Location = new Point(20, 430),
                Size = new Size(360, 45),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = ColorTranslator.FromHtml("#7C3AED"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddProduct.FlatAppearance.BorderSize = 0;
            btnAddProduct.Click += BtnAddProduct_Click;
            btnAddProduct.MouseEnter += (s, e) => btnAddProduct.BackColor = ColorTranslator.FromHtml("#1D4ED8");
            btnAddProduct.MouseLeave += (s, e) => btnAddProduct.BackColor = ColorTranslator.FromHtml("#7C3AED");
           // formCard.Controls.Add(btnAddProduct);

            // Refill Stock Button
            btnRefillStock = new Button
            {
                Text = "REFILL STOCK",
                Location = new Point(20, 490), // Position under Add Product button (430 + 45 + 15 margin)
                Size = new Size(360, 45),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = ColorTranslator.FromHtml("#059669"), // Green color
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefillStock.FlatAppearance.BorderSize = 0;
            btnRefillStock.Click += BtnRefillStock_Click;  // This wires the click event to your handler method
            btnRefillStock.MouseEnter += (s, e) => btnRefillStock.BackColor = ColorTranslator.FromHtml("#047857");
            btnRefillStock.MouseLeave += (s, e) => btnRefillStock.BackColor = ColorTranslator.FromHtml("#059669");
            //formCard.Controls.Add(btnRefillStock);


            // Data Section Cards
            searchCard = new MaterialCard
            {
                Text = "SEARCH PRODUCTS",
                BackColor = Color.White,
                ShadowDepth = 4,
                Radius = 8,
                Size = new Size(dataContainer.Width, 100),
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            dataContainer.Controls.Add(searchCard);

            var lblSearch = new Label
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#334155"),
                Location = new Point(20, 15),
                AutoSize = true
            };
            searchCard.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(20, 40),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Search by name or barcode..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            searchCard.Controls.Add(txtSearch);

            dataCard = new MaterialCard
            {
                BackColor = Color.White,
                ShadowDepth = 4,
                Radius = 8,
                Location = new Point(0, searchCard.Bottom + 10),
                Size = new Size(dataContainer.Width, dataContainer.Height - searchCard.Height - 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                MinimumSize = new Size(0, 350) // Ensures enough space for table
            };
            dataContainer.Controls.Add(dataCard);

            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 10),
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvProducts.RowTemplate.Height = 40;
            dgvProducts.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProducts.ScrollBars = ScrollBars.Both;

            dgvProducts.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorTranslator.FromHtml("#F8FAFC"),
                ForeColor = ColorTranslator.FromHtml("#64748B"),
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                Padding = new Padding(10, 8, 10, 8)
            };

            dgvProducts.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = ColorTranslator.FromHtml("#334155"),
                SelectionBackColor = ColorTranslator.FromHtml("#EDE9FE"),
                SelectionForeColor = ColorTranslator.FromHtml("#7C3AED"),
                Padding = new Padding(10, 8, 10, 8)
            };

            dgvProducts.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorTranslator.FromHtml("#F8FAFC")
            };

            dgvProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProducts.GridColor = ColorTranslator.FromHtml("#F1F5F9");

            dataCard.Controls.Add(dgvProducts);


            // Initially sync full list and refresh display
            fullProductList = new List<Product>(Products);
            RefreshProductList();

            btnUploadImage = new Button
            {
                Text = "📷 Upload Product Image",
                Location = new Point(20, 500),
                Size = new Size(360, 40),
                BackColor = ColorTranslator.FromHtml("#2563EB"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnUploadImage.Click += BtnUploadImage_Click;
            //formCard.Controls.Add(btnUploadImage);
            // ⚡ Group buttons horizontally in a panel
            Panel buttonRow = new Panel
            {
                Location = new Point(20, 430),
                Size = new Size(360, 45),
                BackColor = Color.Transparent
            };

            // Adjust button sizes
            btnAddProduct.Size = new Size(110, 40);
            btnRefillStock.Size = new Size(110, 40);
            btnUploadImage.Size = new Size(130, 40); // longer for icon/text

            // Align buttons in row
            btnAddProduct.Location = new Point(0, 0);
            btnRefillStock.Location = new Point(120, 0);
            btnUploadImage.Location = new Point(240, 0);

            // Add to the panel
            buttonRow.Controls.Add(btnAddProduct);
            buttonRow.Controls.Add(btnRefillStock);
            buttonRow.Controls.Add(btnUploadImage);

            // Add the button row to the formCard
            formCard.Controls.Add(buttonRow);

           
            //formCard.Controls.Add(picProductImage);

        }
        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Enter new category name:", "Add Category", "");

            if (!string.IsNullOrWhiteSpace(input))
            {
                try
                {
                    DatabaseHelper.AddCategory(input);
                    LoadCategories();
                    cmbCategory.SelectedItem = input; // auto-select
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding category: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AdjustLayout()
        {
            int leftMargin = 227;
            int topMargin = 76;

            if (lblTitle != null)
                lblTitle.Location = new Point(leftMargin, topMargin);

            int mainContainerTop = topMargin + lblTitle.Height + 10;
            int mainContainerLeft = leftMargin;
            int mainContainerRightMargin = 30;
            int mainContainerBottomMargin = 30;

            if (mainContainer != null)
                mainContainer.Location = new Point(mainContainerLeft, mainContainerTop);

            if (mainContainer != null)
                mainContainer.Size = new Size(this.Width - mainContainerLeft - mainContainerRightMargin,
                                              this.Height - mainContainerTop - mainContainerBottomMargin);

            if (formContainer != null && mainContainer != null)
                formContainer.Size = new Size(400, mainContainer.Height);

            if (dataContainer != null && mainContainer != null)
                dataContainer.Location = new Point(formContainer.Width, 0);

            if (dataContainer != null && mainContainer != null)
                dataContainer.Size = new Size(mainContainer.Width - formContainer.Width, mainContainer.Height);

            if (formCard != null && formContainer != null)
                formCard.Size = formContainer.Size;

            if (searchCard != null && dataContainer != null)
                searchCard.Size = new Size(dataContainer.Width, 80);

            if (txtSearch != null && searchCard != null)
                txtSearch.Size = new Size(searchCard.Width - 150, 30);

            if (dataCard != null && dataContainer != null)
            {
                dataCard.Location = new Point(0, searchCard.Height);
                dataCard.Size = new Size(dataContainer.Width, dataContainer.Height - searchCard.Height);
            }
        }

        private void AddFormLabel(string text, int yPos)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#64748B"),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            formCard.Controls.Add(label);
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            if (ValidateProduct())
            {
                var newProduct = new Product
                {
                    Barcode = txtBarcode.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    Price = decimal.Parse(txtPrice.Text),
                    Quantity = int.Parse(txtQuantity.Text),
                    Category = cmbCategory.SelectedItem.ToString(),
                    ImagePath = selectedImagePath // Include the image path
                };

                try
                {
                    DatabaseHelper.SaveProduct(newProduct);
                    Products.Add(newProduct);
                    fullProductList.Add(newProduct);

                    MessageBox.Show("Product saved successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearForm();
                    RefreshProductList();

                    // Reset image selection
                    selectedImagePath = "";
                   // picProductImage.Image = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save product: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            var categories = DatabaseHelper.LoadAllCategories();
            cmbCategory.Items.AddRange(categories.ToArray());
        }

        private void BtnUploadImage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Please enter a barcode first", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get or verify storage location
            string storageRoot;
            try
            {
                storageRoot = ImageStorageHelper.GetImageSaveFolder();
                if (storageRoot == null)
                {
                    MessageBox.Show("Image storage location is not configured", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to access image storage: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Product Image",
                Filter = "Image Files (*.jpg;*.png)|*.jpg;*.png"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string sourcePath = openFileDialog.FileName;
                    string destPath = ImageStorageHelper.GetProductImagePath(txtBarcode.Text, sourcePath);

                    // Copy the image
                    File.Copy(sourcePath, destPath, true);

                    // Display the image
                    //picProductImage.Image = Image.FromFile(destPath);

                    // Store relative path in database
                    selectedImagePath = Path.GetRelativePath(storageRoot, destPath);
                    DatabaseHelper.UpdateProductImagePath(txtBarcode.Text.Trim().ToLower(), selectedImagePath);

                    // Update local product
                    var product = Products.FirstOrDefault(p => p.Barcode == txtBarcode.Text.Trim().ToLower());
                    if (product != null)
                        product.ImagePath = selectedImagePath;

                    // Show success with path
                    MessageBox.Show($"Image saved to:\n{destPath}", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }





        private bool ValidateProduct()
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Please enter a barcode", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Focus();
                return false;
            }

            if (Products.Any(p => p.Barcode == txtBarcode.Text.Trim()))
            {
                MessageBox.Show("Product with this barcode already exists", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out _))
            {
                MessageBox.Show("Please enter a valid price", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out _))
            {
                MessageBox.Show("Please enter a valid quantity", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Focus();
                return false;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a category", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            txtBarcode.Text = "";
            txtName.Text = "";
            txtPrice.Text = "";
            txtQuantity.Text = "";
            cmbCategory.SelectedIndex = -1;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (isRefreshing)
                return;

            isRefreshing = true;

            string search = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(search))
            {
                // Reset to full list
                dgvProducts.DataSource = null;
                dgvProducts.DataSource = fullProductList.ToList(); // Use a copy to avoid reference issues
            }
            else
            {
                // Filter safely on a separate list
                var filtered = fullProductList
                    .Where(p => p.Name.ToLower().Contains(search) || p.Barcode.ToLower().Contains(search))
                    .ToList();

                dgvProducts.DataSource = null;
                dgvProducts.DataSource = filtered;
            }

            FormatGrid();

            isRefreshing = false;
        }

        public void RefreshProductList()
        {
            if (Products == null)
                Products = new List<Product>(); // Make sure it's never null

            if (dgvProducts == null)
                return; // Don’t crash if the grid hasn’t loaded yet

            isRefreshing = true;

            fullProductList = new List<Product>(Products); // Store full list (for filtering/searching)

            dgvProducts.DataSource = null;
            dgvProducts.DataSource = fullProductList.ToList(); // Bind the fresh copy

            if (dgvProducts.Columns.Count > 0)
            {
                FormatGrid(); // Style your DataGridView if needed
            }

            dgvProducts.PerformLayout();
            dgvProducts.Refresh();

            isRefreshing = false;
        }


        private void FormatGrid()
        {
            if (dgvProducts?.Columns == null || dgvProducts.Columns.Count == 0)
                return;

            var columns = dgvProducts.Columns.Cast<DataGridViewColumn>().ToList();

            foreach (var column in columns)
            {
                try
                {
                    if (column != null)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        column.MinimumWidth = 120;
                        column.SortMode = DataGridViewColumnSortMode.Automatic;
                    }
                }
                catch
                {
                    continue;
                }
            }

            var priceCol = dgvProducts.Columns["Price"];
            if (priceCol != null)
            {
                priceCol.DefaultCellStyle.Format = "N2";
                priceCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                priceCol.HeaderText = "PRICE (TZS)";
            }

            var qtyCol = dgvProducts.Columns["Quantity"];
            if (qtyCol != null)
            {
                qtyCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            try
            {
                var colBarcode = dgvProducts.Columns["Barcode"];
                var colName = dgvProducts.Columns["Name"];
                var colCategory = dgvProducts.Columns["Category"];

                if (colBarcode != null)
                {
                    colBarcode.DisplayIndex = 0;
                    colBarcode.HeaderText = "BARCODE";
                }

                if (colName != null)
                {
                    colName.DisplayIndex = 1;
                    colName.HeaderText = "PRODUCT NAME";
                }

                if (priceCol != null) priceCol.DisplayIndex = 2;
                if (qtyCol != null) qtyCol.DisplayIndex = 3;
                if (colCategory != null)
                {
                    colCategory.DisplayIndex = 4;
                    colCategory.HeaderText = "CATEGORY";
                }
            }

            catch
            {
                // ignore missing columns
            }
            var imagePathCol = dgvProducts.Columns["ImagePath"];
            if (imagePathCol != null)
            {
                imagePathCol.Visible = false;
            }

        }

        private void BtnRefillStock_Click(object sender, EventArgs e)
        {
            string barcode = txtBarcode.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(barcode))
            {
                MessageBox.Show("Enter the barcode of the product to update", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ FIXED: Reload products list from database
            Products = DatabaseHelper.LoadAllProducts();

            var product = Products.FirstOrDefault(p => p.Barcode == barcode);
            if (product == null)
            {
                MessageBox.Show("No product found with this barcode", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int addQty = 0;
            if (!string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                if (!int.TryParse(txtQuantity.Text, out addQty) || addQty < 0)
                {
                    MessageBox.Show("Enter a valid quantity to add (or leave blank if not refilling)", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtQuantity.Focus();
                    return;
                }
            }

            decimal? newPrice = null;
            if (!string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                decimal priceInput;
                if (!decimal.TryParse(txtPrice.Text, out priceInput) || priceInput <= 0)
                {
                    MessageBox.Show("Enter a valid new price (or leave blank to keep current)", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPrice.Focus();
                    return;
                }
                else
                {
                    if (priceInput != product.Price)
                        newPrice = priceInput;
                }
            }

            try
            {
                DatabaseHelper.UpdateProductFlexible(
                    barcode,
                    quantityChange: addQty > 0 ? addQty : (int?)null,
                    newPrice: newPrice
                );

                // Update in-memory product
                product.Quantity += addQty;
                if (newPrice.HasValue)
                    product.Price = newPrice.Value;

                MessageBox.Show($"✅ Product updated successfully.\n📦 New quantity: {product.Quantity}\n💰 New price: TZS {product.Price:N2}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ✅ Refresh again after update (optional but good)
                Products = DatabaseHelper.LoadAllProducts();
                RefreshProductList();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error updating product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        public class MaterialCard : Panel
        {
            public string Text { get; set; }
            public int ShadowDepth { get; set; } = 2;
            public int Radius { get; set; } = 8;

            public MaterialCard()
            {
                this.BackColor = Color.White;
                this.Padding = new Padding(10);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                DrawShadow(e);

                var cardRect = new Rectangle(0, 0, this.Width - ShadowDepth - 1, this.Height - ShadowDepth - 1);
                using (var path = GetRoundRectPath(cardRect, Radius))
                {
                    using (var brush = new SolidBrush(this.BackColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    e.Graphics.DrawPath(new Pen(ColorTranslator.FromHtml("#E2E8F0")), path);
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    using (var font = new Font("Segoe UI Semibold", 12, FontStyle.Bold))
                    using (var brush = new SolidBrush(ColorTranslator.FromHtml("#334155")))
                    {
                        e.Graphics.DrawString(Text, font, brush, 10, 10);
                    }
                }
            }

            private void DrawShadow(PaintEventArgs e)
            {
                for (int i = 0; i < ShadowDepth; i++)
                {
                    var shadowRect = new Rectangle(i, i, this.Width - (i * 2) - 1, this.Height - (i * 2) - 1);
                    using (var path = GetRoundRectPath(shadowRect, Radius))
                    using (var brush = new SolidBrush(Color.FromArgb(5 * (i + 1), Color.Black)))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                }
            }

            private GraphicsPath GetRoundRectPath(Rectangle rect, int radius)
            {
                var path = new GraphicsPath();
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}
