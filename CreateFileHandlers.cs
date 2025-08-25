using System;
using System.Windows;
using System.Windows.Controls;
using Shaghouri.Services;

namespace Shaghouri
{
    /// <summary>
    /// معالجات عمليات الملفات
    /// </summary>
    public partial class Create
    {
        #region File Operations
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "HCAD files (*.hcad)|*.hcad";
            dialog.DefaultExt = "hcad";
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                _fileService.SaveProject(rectangles, dialog.FileName);
                System.Windows.MessageBox.Show("Project saved successfully!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving project: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "HCAD files (*.hcad)|*.hcad";
            dialog.DefaultExt = "hcad";
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var loadedRectangles = _fileService.LoadProject(dialog.FileName);
                rectangles.Clear();
                rectangles.AddRange(loadedRectangles);
                
                // Redraw
                LeftCanvas.ShapesToDraw = rectangles;
                LeftCanvas.InvalidateVisual();
                ShapePreviewsList.ItemsSource = null;
                ShapePreviewsList.ItemsSource = rectangles;

                System.Windows.MessageBox.Show("Project loaded successfully!", "Open", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading project: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        #endregion
    }
} 