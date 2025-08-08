using System;
using System.Windows.Forms;

namespace InventorySystem.Helpers
{
    public static class FolderHelper
    {
        public static string PromptUserForImageFolder()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select folder to store product images";
                folderDialog.ShowNewFolderButton = true;
                folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    return folderDialog.SelectedPath;
                }
                else
                {
                    MessageBox.Show("Image folder selection is required to continue.", "Folder Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
            }
        }
    }
}
