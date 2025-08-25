using System.Windows;
using System.Windows.Controls;
using Shaghouri.Models;

namespace Shaghouri.Services
{
    /// <summary>
    /// خدمة إدارة اللوحات وعرضها
    /// </summary>
    public class CanvasService
    {
        /// <summary>
        /// عرض اللوحة اليسرى فقط
        /// </summary>
        public void ShowOnlyLeftCanvas(Grid canvasGrid, Canvas leftCanvas, Canvas rightCanvas)
        {
            leftCanvas.Visibility = Visibility.Visible;
            rightCanvas.Visibility = Visibility.Collapsed;
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(0);
        }
        
        /// <summary>
        /// عرض اللوحة اليمنى فقط
        /// </summary>
        public void ShowOnlyRightCanvas(Grid canvasGrid, Canvas leftCanvas, Canvas rightCanvas)
        {
            leftCanvas.Visibility = Visibility.Collapsed;
            rightCanvas.Visibility = Visibility.Visible;
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(0);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }
        
        /// <summary>
        /// عرض كلا اللوحتين
        /// </summary>
        public void ShowBothCanvases(Grid canvasGrid, Canvas leftCanvas, Canvas rightCanvas)
        {
            leftCanvas.Visibility = Visibility.Visible;
            rightCanvas.Visibility = Visibility.Visible;
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }
        
        /// <summary>
        /// تبديل عرض اللوحة اليسرى
        /// </summary>
        public void ToggleLeftPanel(FrameworkElement leftPanel, Grid parentGrid)
        {
            if (leftPanel.Visibility == Visibility.Visible)
            {
                leftPanel.Visibility = Visibility.Collapsed;
                parentGrid.ColumnDefinitions[0].Width = new GridLength(0);
            }
            else
            {
                leftPanel.Visibility = Visibility.Visible;
                parentGrid.ColumnDefinitions[0].Width = new GridLength(150);
            }
        }
        
        /// <summary>
        /// تبديل عرض اللوحة اليمنى
        /// </summary>
        public void ToggleRightPanel(FrameworkElement rightPanel, Grid parentGrid)
        {
            if (rightPanel.Visibility == Visibility.Visible)
            {
                rightPanel.Visibility = Visibility.Collapsed;
                parentGrid.ColumnDefinitions[2].Width = new GridLength(0);
            }
            else
            {
                rightPanel.Visibility = Visibility.Visible;
                parentGrid.ColumnDefinitions[2].Width = new GridLength(150);
            }
        }
        
        /// <summary>
        /// إعداد اللوحات الافتراضية
        /// </summary>
        public void InitializeDefaultCanvasLayout(Grid canvasGrid, Canvas leftCanvas, Canvas rightCanvas)
        {
            leftCanvas.Visibility = Visibility.Visible;
            rightCanvas.Visibility = Visibility.Visible;
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }
    }
} 