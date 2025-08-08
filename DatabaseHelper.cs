using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Models;

namespace InventorySystem.Helpers
{
    public static class DatabaseHelper
    {
        public static string dbPath = Path.Combine(Application.StartupPath, "inventory.db");

        // New helper method added here:
        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={dbPath};Version=3;");
        }

        public static string GetProductImagePath(string barcode, string sourceImagePath)
        {
            // Create products directory if it doesn't exist
            string productsDir = Path.Combine(Application.StartupPath, "products");
            Directory.CreateDirectory(productsDir);

            // Get file extension from source image
            string extension = Path.GetExtension(sourceImagePath);

            // Create filename from barcode (sanitize it first)
            string cleanBarcode = new string(barcode.Where(char.IsLetterOrDigit).ToArray());
            string fileName = $"product_{cleanBarcode}{extension}";

            // Return full path
            return Path.Combine(productsDir, fileName);
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                // Create Products table with ImagePath
                string productsTable = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        Barcode TEXT PRIMARY KEY,
                        Name TEXT,
                        Price REAL,
                        Quantity INTEGER,
                        Category TEXT,
                        ImagePath TEXT
                    );";
                using (var cmd = new SQLiteCommand(productsTable, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Create Sales table
                string salesTable = @"
                    CREATE TABLE IF NOT EXISTS Sales (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Barcode TEXT,
                        ProductName TEXT,
                        Quantity INTEGER,
                        UnitPrice REAL,
                        Total REAL,
                        SaleDate TEXT
                    );";
                using (var cmd = new SQLiteCommand(salesTable, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void SaveProduct(Product product)
        {
            string cleanBarcode = (product.Barcode ?? "").Trim().ToLower();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string sql = @"
                    INSERT OR REPLACE INTO Products 
                    (Barcode, Name, Price, Quantity, Category, ImagePath) 
                    VALUES (@Barcode, @Name, @Price, @Quantity, @Category, @ImagePath);";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Barcode", cleanBarcode);
                    cmd.Parameters.AddWithValue("@Name", product.Name?.Trim());
                    cmd.Parameters.AddWithValue("@Price", product.Price);
                    cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
                    cmd.Parameters.AddWithValue("@Category", product.Category?.Trim());
                    cmd.Parameters.AddWithValue("@ImagePath", product.ImagePath ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Product> LoadAllProducts()
        {
            var products = new List<Product>();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string sql = "SELECT * FROM Products";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new Product
                        {
                            Barcode = reader["Barcode"].ToString()?.Trim().ToLower(),
                            Name = reader["Name"].ToString()?.Trim(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            Category = reader["Category"].ToString()?.Trim(),
                            ImagePath = reader["ImagePath"].ToString()?.Trim()
                        };

                        products.Add(product);
                    }
                }
            }

            return products;
        }

        public static void UpdateProductImagePath(string barcode, string imagePath)
        {
            string cleanBarcode = barcode?.Trim().ToLower();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string sql = "UPDATE Products SET ImagePath = @ImagePath WHERE Barcode = @Barcode";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ImagePath", imagePath ?? "");
                    cmd.Parameters.AddWithValue("@Barcode", cleanBarcode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool ValidateUser(string username, string password, string role)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                string query = @"
            SELECT COUNT(*) FROM users
            WHERE username = @username COLLATE NOCASE
              AND password = @password COLLATE NOCASE
              AND role = @role COLLATE NOCASE";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@role", role);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    MessageBox.Show($"Login check:\nUsername: {username}\nPassword: {password}\nRole: {role}\nMatch count: {count}");

                    return count > 0;
                }
            }
        }

        public static string? GetUserRole(string username, string password)
        {
            using (var conn = GetConnection())  // <-- changed here to call GetConnection()
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT role FROM users WHERE username = @username AND password = @password", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    var result = cmd.ExecuteScalar();
                    return result?.ToString(); // Returns "admin" or "employee"
                }
            }
        }

        public static void UpdateProductQuantity(string barcode, int newQuantity)
        {
            string cleanBarcode = (barcode ?? "").Trim().ToLower();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string sql = "UPDATE Products SET Quantity = @Quantity WHERE Barcode = @Barcode";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                    cmd.Parameters.AddWithValue("@Barcode", cleanBarcode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateProductFlexible(string barcode, int? quantityChange = null, int? newQuantity = null, decimal? newPrice = null)
        {
            string cleanBarcode = (barcode ?? "").Trim().ToLower();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string selectSql = "SELECT Quantity, Price FROM Products WHERE Barcode = @Barcode";
                int currentQuantity = 0;
                decimal currentPrice = 0m;

                using (var selectCmd = new SQLiteCommand(selectSql, conn))
                {
                    selectCmd.Parameters.AddWithValue("@Barcode", cleanBarcode);
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentQuantity = Convert.ToInt32(reader["Quantity"]);
                            currentPrice = Convert.ToDecimal(reader["Price"]);
                        }
                        else
                        {
                            throw new Exception("Product not found.");
                        }
                    }
                }

                int updatedQuantity = currentQuantity;
                if (newQuantity.HasValue) updatedQuantity = newQuantity.Value;
                else if (quantityChange.HasValue) updatedQuantity += quantityChange.Value;
                if (updatedQuantity < 0) updatedQuantity = 0;

                decimal updatedPrice = currentPrice;
                if (newPrice.HasValue && newPrice.Value >= 0) updatedPrice = newPrice.Value;

                string updateSql = "UPDATE Products SET Quantity = @Quantity, Price = @Price WHERE Barcode = @Barcode";
                using (var updateCmd = new SQLiteCommand(updateSql, conn))
                {
                    updateCmd.Parameters.AddWithValue("@Quantity", updatedQuantity);
                    updateCmd.Parameters.AddWithValue("@Price", updatedPrice);
                    updateCmd.Parameters.AddWithValue("@Barcode", cleanBarcode);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteLastSaleByBarcode(string barcode)
        {
            string cleanBarcode = (barcode ?? "").Trim().ToLower();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string getIdSql = @"
                    SELECT Id FROM Sales 
                    WHERE LOWER(Barcode) = @Barcode 
                    ORDER BY SaleDate DESC 
                    LIMIT 1";

                using (var getIdCmd = new SQLiteCommand(getIdSql, conn))
                {
                    getIdCmd.Parameters.AddWithValue("@Barcode", cleanBarcode);
                    var saleId = getIdCmd.ExecuteScalar();

                    if (saleId != null)
                    {
                        string deleteSql = "DELETE FROM Sales WHERE Id = @Id";

                        using (var deleteCmd = new SQLiteCommand(deleteSql, conn))
                        {
                            deleteCmd.Parameters.AddWithValue("@Id", Convert.ToInt32(saleId));
                            deleteCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static int GetProductCount()
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Products", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static decimal GetMonthlySales()
        {
            string currentMonth = DateTime.Now.ToString("yyyy-MM");
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                string sql = "SELECT SUM(Total) FROM Sales WHERE SaleDate LIKE @Month || '%'";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Month", currentMonth);
                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
        }

        public static decimal GetInventoryValue()
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT SUM(Price * Quantity) FROM Products", conn))
                {
                    var result = cmd.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
        }

        public static int GetLowStockCount(int threshold = 5)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Products WHERE Quantity <= @Threshold";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Threshold", threshold);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static List<string> LoadRecentActivities(int limit = 10)
        {
            var activities = new List<string>();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string productSql = "SELECT Name, 'Product Added' AS Type, Barcode, rowid AS Id, '' AS SaleDate FROM Products ORDER BY rowid DESC LIMIT @Limit";
                string salesSql = "SELECT ProductName AS Name, 'Sale' AS Type, Barcode, Id, SaleDate FROM Sales ORDER BY SaleDate DESC LIMIT @Limit";

                var combined = new List<(string Name, string Type, string Date)>();

                using (var productCmd = new SQLiteCommand(productSql, conn))
                {
                    productCmd.Parameters.AddWithValue("@Limit", limit);
                    using (var reader = productCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            combined.Add((reader["Name"].ToString(), reader["Type"].ToString(), "Added just now"));
                        }
                    }
                }

                using (var salesCmd = new SQLiteCommand(salesSql, conn))
                {
                    salesCmd.Parameters.AddWithValue("@Limit", limit);
                    using (var reader = salesCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime dt = DateTime.Parse(reader["SaleDate"].ToString());
                            combined.Add((reader["Name"].ToString(), reader["Type"].ToString(), dt.ToString("yyyy-MM-dd HH:mm")));
                        }
                    }
                }

                // Sort by most recent activity
                combined = combined.OrderByDescending(x => x.Date).Take(limit).ToList();
                activities = combined.Select(x => $"{x.Type} ➤ {x.Name} ({x.Date})").ToList();
            }

            return activities;
        }

        public static bool AddUser(string username, string password, string role)
        {
            using var conn = new SQLiteConnection("Data Source=inventory.db");
            conn.Open();
            var cmd = new SQLiteCommand("INSERT INTO users (username, password, role) VALUES (@u, @p, @r)", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password); // In production, hash this!
            cmd.Parameters.AddWithValue("@r", role);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteUser(string username)
        {
            using var conn = new SQLiteConnection("Data Source=inventory.db");
            conn.Open();
            var cmd = new SQLiteCommand("DELETE FROM users WHERE username = @u AND role = 'employee'", conn);
            cmd.Parameters.AddWithValue("@u", username);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static List<(string Username, string Role)> GetUsersByRole(string role)
        {
            var list = new List<(string, string)>();
            using var conn = new SQLiteConnection("Data Source=inventory.db");
            conn.Open();
            var cmd = new SQLiteCommand("SELECT username, role FROM users WHERE role = @r", conn);
            cmd.Parameters.AddWithValue("@r", role);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((reader.GetString(0), reader.GetString(1)));
            }
            return list;
        }

        public static void SaveSale(string barcode, string name, int quantity, decimal unitPrice)
        {
            decimal total = unitPrice * quantity;
            string dateNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string sql = @"
                    INSERT INTO Sales (Barcode, ProductName, Quantity, UnitPrice, Total, SaleDate)
                    VALUES (@Barcode, @ProductName, @Quantity, @UnitPrice, @Total, @SaleDate);";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Barcode", barcode?.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@ProductName", name?.Trim());
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@SaleDate", dateNow);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Sale> GetTodaySales()
        {
            List<Sale> sales = new List<Sale>();
            using (var conn = new SQLiteConnection($"Data Source={dbPath}"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = @"SELECT Barcode, ProductName, Quantity, Total, SaleDate
                                FROM Sales
                                WHERE DATE(SaleDate) = DATE('now', 'localtime')";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sales.Add(new Sale
                            {
                                Barcode = reader["Barcode"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                QuantitySold = Convert.ToInt32(reader["Quantity"]),
                                TotalAmount = Convert.ToDecimal(reader["Total"]),
                                Timestamp = Convert.ToDateTime(reader["SaleDate"])
                            });
                        }
                    }
                }
            }
            return sales;
        }
    }
}
