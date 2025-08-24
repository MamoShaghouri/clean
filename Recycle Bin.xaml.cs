using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Shaghouri
{
    /// <summary>
    /// Interaction logic for Recycle_Bin.xaml
    /// </summary>
    public partial class Recycle_Bin : Window
    {
        public Recycle_Bin()
        {
            InitializeComponent();
            Recycle_Bin_Show_Canvas.MouseRightButtonUp += Canvas_MouseRightButtonUp;
        }

        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Check if any item is selected
            if (IsAnyItemSelected())
                return; // Do not show context menu

            // Create context menu with modern style
            var contextMenu = new ContextMenu
            {
                Style = (Style)FindResource("ModernContextMenuStyle")
            };

            var deleteAllItem = new MenuItem { Header = "Delete all", Style = (Style)FindResource("ModernMenuItemStyle") };
            deleteAllItem.Click += (s, args) => DeleteAll();

            var restoreAllItem = new MenuItem { Header = "Restore all", Style = (Style)FindResource("ModernMenuItemStyle") };
            restoreAllItem.Click += (s, args) => RestoreAll();

            contextMenu.Items.Add(deleteAllItem);
            contextMenu.Items.Add(restoreAllItem);

            // Show context menu at mouse position
            contextMenu.IsOpen = true;
        }

        // Dummy implementation, replace with your actual selection logic
        private bool IsAnyItemSelected()
        {
            // TODO: Replace with your logic to check if any item is selected on the canvas
            return false;
        }

        private void DeleteAll()
        {
            // TODO: Implement delete all logic
            MessageBox.Show("All items deleted.");
        }

        private void RestoreAll()
        {
            // TODO: Implement restore all logic
            MessageBox.Show("All items restored.");
        }
    }
}
