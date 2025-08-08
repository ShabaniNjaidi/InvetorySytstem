using System;
using System.IO;
using System.Windows.Forms;

namespace InventorySystem.Helpers
{
    public static class ImageStorageHelper
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InventorySystem",
            "image_storage.config");

        public static string GetImageSaveFolder()
        {
            // Create config directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

            // If config exists, return stored path
            if (File.Exists(ConfigPath))
            {
                string savedPath = File.ReadAllText(ConfigPath).Trim();
                if (Directory.Exists(savedPath) && !savedPath.StartsWith("C:\\", StringComparison.OrdinalIgnoreCase))
                {
                    return savedPath;
                }
            }

            // If no valid config, prompt user to select a location
            return PromptForImageStorageLocation();
        }

        public static string PromptForImageStorageLocation()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select permanent storage location for product images (NOT on C: drive)";
                dialog.ShowNewFolderButton = true;

                // Suggest common non-C drive locations
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && !drive.Name.StartsWith("C:\\"))
                    {
                        dialog.SelectedPath = drive.Name;
                        break;
                    }
                }

                while (true)
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string selectedPath = dialog.SelectedPath;

                        // Validate not on C drive
                        if (selectedPath.StartsWith("C:\\", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Please select a location NOT on the C: drive for data safety.",
                                "Invalid Location", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        // Validate write permissions
                        try
                        {
                            string testFile = Path.Combine(selectedPath, "inventory_test.tmp");
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                        }
                        catch
                        {
                            MessageBox.Show("Cannot write to selected location. Please choose a different folder.",
                                "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        // Save the location
                        File.WriteAllText(ConfigPath, selectedPath);
                        return selectedPath;
                    }
                    else
                    {
                        // User cancelled - use a fallback location
                        string fallback = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            "InventorySystem_Images");
                        Directory.CreateDirectory(fallback);
                        File.WriteAllText(ConfigPath, fallback);
                        return fallback;
                    }
                }
            }
        }

        public static string GetProductImagePath(string barcode, string sourceImagePath)
        {
            string storageRoot = GetImageSaveFolder();
            string productsDir = Path.Combine(storageRoot, "ProductImages");
            Directory.CreateDirectory(productsDir);

            // Sanitize barcode and create filename
            string cleanBarcode = new string(barcode.Where(char.IsLetterOrDigit).ToArray());
            string extension = Path.GetExtension(sourceImagePath);
            string filename = $"product_{cleanBarcode}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

            return Path.Combine(productsDir, filename);
        }
    }
}