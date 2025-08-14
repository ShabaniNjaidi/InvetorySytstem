using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Models;

namespace InventorySystem.Helpers
{
    public class UserShopInfo
    {
        public string ShopName { get; set; }
        public string Tagline { get; set; }
        public string Address { get; set; }
        public string ContactInfo { get; set; }
    }

    public static class DatabaseHelper
    {
        public static string dbPath = Path.Combine(Application.StartupPath, "inventory.db");

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection($"Data Source={dbPath};Version=3;");
        }
        public class SaleItemDto
        {
            public string Barcode { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
        public static string GetProductImagePath(string barcode, string sourceImagePath)
        {
            string productsDir = Path.Combine(Application.StartupPath, "products");
            Directory.CreateDirectory(productsDir);

            string extension = Path.GetExtension(sourceImagePath);
            string cleanBarcode = new string(barcode.Where(char.IsLetterOrDigit).ToArray());
            string fileName = $"product_{cleanBarcode}{extension}";

            return Path.Combine(productsDir, fileName);
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var conn = GetConnection())
            {
                conn.Open();

                string productsTable = @"
                    CREATE TABLE IF NOT EXISTS Products (
                        Barcode TEXT PRIMARY KEY,
                        Name TEXT,
                        Price REAL,
                        Quantity INTEGER,
                        Category TEXT,
                        ImagePath TEXT
                    );";
                new SQLiteCommand(productsTable, conn).ExecuteNonQuery();

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
                new SQLiteCommand(salesTable, conn).ExecuteNonQuery();

                string usersTable = @"
                    CREATE TABLE IF NOT EXISTS users (
                        username TEXT PRIMARY KEY,
                        password TEXT,
                        role TEXT
                    );";
                new SQLiteCommand(usersTable, conn).ExecuteNonQuery();
            }
        }
            
        public static void SaveProduct(Product product)
        {
            string cleanBarcode = (product.Barcode ?? "").Trim().ToLower();

            using (var conn = GetConnection())
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

            using (var conn = GetConnection())
            {
                conn.Open();

                string sql = "SELECT * FROM Products";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Barcode = reader["Barcode"].ToString()?.Trim().ToLower(),
                            Name = reader["Name"].ToString()?.Trim(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            Category = reader["Category"].ToString()?.Trim(),
                            ImagePath = reader["ImagePath"].ToString()?.Trim()
                        });
                    }
                }
            }

            return products;
        }
        public static bool UpdateProductQuantityAtomic(string barcode, int qtyToDeduct)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    var cmd = conn.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                UPDATE Products
                SET Quantity = Quantity - @qty
                WHERE Barcode = @barcode AND Quantity >= @qty;
            ";

                    cmd.Parameters.AddWithValue("@qty", qtyToDeduct);
                    cmd.Parameters.AddWithValue("@barcode", barcode);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        transaction.Commit();
                        return true; // Success
                    }
                    else
                    {
                        transaction.Rollback();
                        return false; // Not enough stock or barcode not found
                    }
                }
            }
        }


        public static UserShopInfo GetUserShopInfo(int userId)
        {
            UserShopInfo info = null;

            using (var conn = GetConnection())
            {
                conn.Open();  // Make sure connection is open

                string query = @"
            SELECT ShopName, shop_tagline, shop_address, shop_contact
            FROM users
            WHERE id = @userId";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            info = new UserShopInfo
                            {
                                ShopName = reader["shopname"]?.ToString() ?? "Unknown Shop",
                                Tagline = reader["shop_tagline"]?.ToString() ?? "",
                                Address = reader["shop_address"]?.ToString() ?? "",
                                ContactInfo = reader["shop_contact"]?.ToString() ?? ""
                            };
                        }
                    }
                }
            }

            return info;
        }

        public static void SaveTransactionSummary(DateTime date, decimal total, decimal paid, decimal change, List<CartItem> items)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                INSERT INTO SalesSummary (Date, TotalAmount, AmountPaid, ChangeAmount, ItemCount)
                VALUES (@date, @total, @paid, @change, @count);
                SELECT last_insert_rowid();";  // SQLite specific way to get last inserted Id

                        cmd.Parameters.AddWithValue("@date", date);
                        cmd.Parameters.AddWithValue("@total", total);
                        cmd.Parameters.AddWithValue("@paid", paid);
                        cmd.Parameters.AddWithValue("@change", change);
                        cmd.Parameters.AddWithValue("@count", items.Sum(i => i.Quantity));

                        long summaryId = (long)cmd.ExecuteScalar();

                        foreach (var item in items)
                        {
                            using (var itemCmd = conn.CreateCommand())
                            {
                                itemCmd.CommandText = @"
                        INSERT INTO SalesDetails (SalesSummaryId, ProductName, Quantity, Price, SubTotal)
                        VALUES (@summaryId, @productName, @quantity, @price, @subtotal)";
                                itemCmd.Parameters.AddWithValue("@summaryId", summaryId);
                                itemCmd.Parameters.AddWithValue("@productName", item.Name);
                                itemCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                                itemCmd.Parameters.AddWithValue("@price", item.Price);
                                itemCmd.Parameters.AddWithValue("@subtotal", item.SubTotal);
                                itemCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();
                }
            }
        }


        public static (int UserId, string Role)? GetUserCredentials(string username, string password)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT Id, Role FROM Users WHERE Username = @username AND Password = @password";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string role = reader.GetString(1);
                            return (id, role);
                        }
                    }
                }
            }
            return null;
        }



        public static List<string> LoadAllCategories()
        {
            var categories = new List<string>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT Name FROM Categories ORDER BY Name";
                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        categories.Add(reader.GetString(0));
                }
            }
            return categories;
        }

        public static void AddCategory(string categoryName)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string insert = "INSERT OR IGNORE INTO Categories (Name) VALUES (@name)";
                using (var cmd = new SQLiteCommand(insert, conn))
                {
                    cmd.Parameters.AddWithValue("@name", categoryName.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public static void UpdateProductImagePath(string barcode, string imagePath)
        {
            string cleanBarcode = barcode?.Trim().ToLower();

            using (var conn = GetConnection())
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
            using (var conn = GetConnection())
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
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT role FROM users WHERE username = @username AND password = @password", conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }

        public static void UpdateProductQuantity(string barcode, int newQuantity)
        {
            string cleanBarcode = (barcode ?? "").Trim().ToLower();

            using (var conn = GetConnection())
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

            using (var conn = GetConnection())
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

                int updatedQuantity = newQuantity ?? currentQuantity + (quantityChange ?? 0);
                updatedQuantity = Math.Max(0, updatedQuantity);
                decimal updatedPrice = newPrice ?? currentPrice;

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

            using (var conn = GetConnection())
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
            using var conn = GetConnection();
            conn.Open();
            var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Products", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static decimal GetMonthlySales()
        {
            string currentMonth = DateTime.Now.ToString("yyyy-MM");
            using var conn = GetConnection();
            conn.Open();
            var cmd = new SQLiteCommand("SELECT SUM(Total) FROM Sales WHERE SaleDate LIKE @Month || '%'", conn);
            cmd.Parameters.AddWithValue("@Month", currentMonth);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }

        public static decimal GetInventoryValue()
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new SQLiteCommand("SELECT SUM(Price * Quantity) FROM Products", conn);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }

        public static int GetLowStockCount(int threshold = 5)
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Products WHERE Quantity <= @Threshold", conn);
            cmd.Parameters.AddWithValue("@Threshold", threshold);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static List<string> LoadRecentActivities(int limit = 10)
        {
            var activities = new List<string>();

            using var conn = GetConnection();
            conn.Open();

            var combined = new List<(string Name, string Type, string Date)>();

            var productCmd = new SQLiteCommand("SELECT Name, 'Product Added' AS Type, '' AS SaleDate FROM Products ORDER BY rowid DESC LIMIT @Limit", conn);
            productCmd.Parameters.AddWithValue("@Limit", limit);
            using (var reader = productCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    combined.Add((reader["Name"].ToString(), reader["Type"].ToString(), "Just now"));
                }
            }

            var salesCmd = new SQLiteCommand("SELECT ProductName AS Name, 'Sale' AS Type, SaleDate FROM Sales ORDER BY SaleDate DESC LIMIT @Limit", conn);
            salesCmd.Parameters.AddWithValue("@Limit", limit);
            using (var reader = salesCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    DateTime dt = DateTime.Parse(reader["SaleDate"].ToString());
                    combined.Add((reader["Name"].ToString(), reader["Type"].ToString(), dt.ToString("yyyy-MM-dd HH:mm")));
                }
            }

            return combined.OrderByDescending(x => x.Date)
                           .Take(limit)
                           .Select(x => $"{x.Type} ➤ {x.Name} ({x.Date})")
                           .ToList();
        }

        public static bool AddUser(string username, string password, string role)
        {
            using var conn = GetConnection();
            conn.Open();
            var cmd = new SQLiteCommand("INSERT INTO users (username, password, role) VALUES (@u, @p, @r)", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
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
            using var conn = GetConnection();
            conn.Open();
            var cmd = new SQLiteCommand("DELETE FROM users WHERE username = @u AND role = 'employee'", conn);
            cmd.Parameters.AddWithValue("@u", username);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static List<(string Username, string Role)> GetUsersByRole(string role)
        {
            var list = new List<(string, string)>();
            using var conn = GetConnection();
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


        public static async Task<long> FinalizeTransactionAsync(
    DateTime timestamp,
    int userId,
    string customerName,
    string paymentMethod,
    string paymentRef,
    decimal total,
    decimal paid,
    decimal change,
    decimal discount, // <-- new parameter
    List<SaleItemDto> items)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("No items to process.");

            using var conn = GetConnection();
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                // 1) Check stock and update Products table
                foreach (var item in items)
                {
                    using var checkCmd = conn.CreateCommand();
                    checkCmd.Transaction = tx;
                    checkCmd.CommandText = "SELECT Quantity FROM Products WHERE Barcode = @Barcode";
                    checkCmd.Parameters.AddWithValue("@Barcode", item.Barcode);

                    var currentQtyObj = await checkCmd.ExecuteScalarAsync();
                    if (currentQtyObj == null)
                        throw new InvalidOperationException($"Product not found: {item.Name}");

                    int currentQty = Convert.ToInt32(currentQtyObj);

                    if (currentQty <= 0 || currentQty < item.Quantity)
                    {
                        string status = currentQty <= 0 ? "out of stock" : $"only {currentQty} available";
                        throw new InvalidOperationException($"{item.Name} is {status}. Requested: {item.Quantity}");
                    }

                    using var updateCmd = conn.CreateCommand();
                    updateCmd.Transaction = tx;
                    updateCmd.CommandText = "UPDATE Products SET Quantity = Quantity - @Qty WHERE Barcode = @Barcode";
                    updateCmd.Parameters.AddWithValue("@Qty", item.Quantity);
                    updateCmd.Parameters.AddWithValue("@Barcode", item.Barcode);
                    await updateCmd.ExecuteNonQueryAsync();
                }

                // 2) Insert transaction header with Discount
                long transactionId;
                using (var insertHeaderCmd = conn.CreateCommand())
                {
                    insertHeaderCmd.Transaction = tx;
                    insertHeaderCmd.CommandText = @"
                INSERT INTO Transactions
                (Timestamp, UserId, CustomerName, PaymentMethod, PaymentRef, Total, Paid, Change, Discount)
                VALUES (@Timestamp, @UserId, @CustomerName, @PaymentMethod, @PaymentRef, @Total, @Paid, @Change, @Discount);
                SELECT last_insert_rowid();
            ";
                    insertHeaderCmd.Parameters.AddWithValue("@Timestamp", timestamp);
                    insertHeaderCmd.Parameters.AddWithValue("@UserId", userId);
                    insertHeaderCmd.Parameters.AddWithValue("@CustomerName", customerName ?? "");
                    insertHeaderCmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod ?? "");
                    insertHeaderCmd.Parameters.AddWithValue("@PaymentRef", paymentRef ?? "");
                    insertHeaderCmd.Parameters.AddWithValue("@Total", total);
                    insertHeaderCmd.Parameters.AddWithValue("@Paid", paid);
                    insertHeaderCmd.Parameters.AddWithValue("@Change", change);
                    insertHeaderCmd.Parameters.AddWithValue("@Discount", discount);

                    transactionId = Convert.ToInt64(await insertHeaderCmd.ExecuteScalarAsync());
                }

                // 3) Insert sale items
                foreach (var item in items)
                {
                    using var insertItemCmd = conn.CreateCommand();
                    insertItemCmd.Transaction = tx;
                    insertItemCmd.CommandText = @"
                INSERT INTO Sales
                (TransactionId, Barcode, ProductName, Quantity, UnitPrice, Total, SaleDate)
                VALUES (@TransactionId, @Barcode, @ProductName, @Quantity, @UnitPrice, @Total, @SaleDate)";
                    insertItemCmd.Parameters.AddWithValue("@TransactionId", transactionId);
                    insertItemCmd.Parameters.AddWithValue("@Barcode", item.Barcode);
                    insertItemCmd.Parameters.AddWithValue("@ProductName", item.Name);
                    insertItemCmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    insertItemCmd.Parameters.AddWithValue("@UnitPrice", item.Price);
                    insertItemCmd.Parameters.AddWithValue("@Total", item.Price * item.Quantity);
                    insertItemCmd.Parameters.AddWithValue("@SaleDate", timestamp);

                    await insertItemCmd.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return transactionId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }



        public static List<Sale> GetTodaySales()
        {
            List<Sale> sales = new List<Sale>();
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
                    SELECT Barcode, ProductName, Quantity, Total, SaleDate
                    FROM Sales
                    WHERE DATE(SaleDate) = DATE('now', 'localtime')", conn);

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
            return sales;
        }
    }
}
