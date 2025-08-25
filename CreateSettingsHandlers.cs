using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Shaghouri.Models;
using Shaghouri.Services;

namespace Shaghouri
{
    /// <summary>
    /// معالجات الإعدادات والواجهة
    /// </summary>
    public partial class Create
    {
        #region Settings and UI
        
        private void ApplySettingsToUI()
        {
            if (LeftCanvas != null && settings != null)
            {
                // عرض القيمة للمستخدم مقسومة على 10
                            TextBoxLeftCanvasGridSpacingWidth.Text = UnitService.ConvertGridSpacingFromPixels(settings.GridRectWidth).ToString();
            TextBoxLeftCanvasGridSpacingHeight.Text = UnitService.ConvertGridSpacingFromPixels(settings.GridRectHeight).ToString();
                LeftCanvas.GridRectWidth = settings.GridRectWidth;
                LeftCanvas.GridRectHeight = settings.GridRectHeight;
                LeftCanvas.GridLinesColor = AppSettings.ParseColor(settings.GridLinesColor);
                LeftCanvas.Opacity = settings.GridOpacity;
                LeftCanvas.ShowGrid = settings.ShowGrid;
                LeftCanvas.ZoomLevel = settings.ZoomLevel;
                
                // Apply background (gradient or static)
                if (settings.LeftCanvasIsGradient)
                {
                    var color = AppSettings.ParseColor(settings.LeftCanvasGradientColor);
                    byte alpha = (byte)(settings.LeftCanvasBackgroundOpacity * 255);
                    var gradient = new LinearGradientBrush();
                    gradient.StartPoint = new Point(0, 0);
                    gradient.EndPoint = new Point(1, 1);
                    gradient.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, 255, 255, 255), 0));
                    gradient.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, color.R, color.G, color.B), 1));
                    LeftCanvas.Background = gradient;
                }
                else if (!string.IsNullOrEmpty(settings.LeftCanvasBackgroundColor))
                {
                    var color = AppSettings.ParseColor(settings.LeftCanvasBackgroundColor);
                    byte alpha = (byte)(settings.LeftCanvasBackgroundOpacity * 255);
                    LeftCanvas.Background = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
                }
                
                // Restore slider value
                SilderLeftCanvasbackgroundOpacity.Value = settings.LeftCanvasBackgroundOpacity * 100;
            }
            
            // Apply settings to Right Canvas
            if (RightCanvas != null && settings != null)
            {
                // Grid settings
                RightCanvas.GridLinesColor = AppSettings.ParseColor(settings.RightCanvasGridLinesColor);
                RightCanvas.Opacity = settings.RightCanvasGridOpacity;
                RightCanvas.ShowGrid = settings.RightCanvasShowGrid;
                
                // Background settings
                if (settings.RightCanvasIsGradient)
                {
                    var color = AppSettings.ParseColor(settings.RightCanvasGradientColor);
                    byte alpha = (byte)(settings.RightCanvasBackgroundOpacity * 255);
                    var gradient = new LinearGradientBrush();
                    gradient.StartPoint = new Point(0, 0);
                    gradient.EndPoint = new Point(1, 1);
                    gradient.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, 255, 255, 255), 0));
                    gradient.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, color.R, color.G, color.B), 1));
                    RightCanvas.Background = gradient;
                }
                else if (!string.IsNullOrEmpty(settings.RightCanvasBackgroundColor))
                {
                    var color = AppSettings.ParseColor(settings.RightCanvasBackgroundColor);
                    byte alpha = (byte)(settings.RightCanvasBackgroundOpacity * 255);
                    RightCanvas.Background = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
                }
                
                // Restore slider values
                SilderRightCanvasGridOpacity.Value = settings.RightCanvasGridOpacity * 100;
                SilderRightCanvasbackgroundOpacity.Value = settings.RightCanvasBackgroundOpacity * 100;
            }
        }
        
        private void SaveSettingsFromUI()
        {
            if (LeftCanvas != null && settings != null)
            {
                // احفظ القيمة المنطقية باستخدام خدمة الوحدات
                if (double.TryParse(TextBoxLeftCanvasGridSpacingWidth.Text, out double widthValue))
                    settings.GridRectWidth = UnitService.ConvertGridSpacingToPixels(widthValue);
                if (double.TryParse(TextBoxLeftCanvasGridSpacingHeight.Text, out double heightValue))
                    settings.GridRectHeight = UnitService.ConvertGridSpacingToPixels(heightValue);
                    
                settings.GridLinesColor = AppSettings.ColorToHex(LeftCanvas.GridLinesColor);
                settings.GridOpacity = SilderLeftCanvasGridOpacity.Value / 100.0;
                settings.ShowGrid = LeftCanvas.ShowGrid;
                settings.ZoomLevel = LeftCanvas.ZoomLevel;
                
                // Save background (gradient or static)
                if (LeftCanvas.Background is LinearGradientBrush grad && grad.GradientStops.Count >= 2)
                {
                    settings.LeftCanvasIsGradient = true;
                    settings.LeftCanvasGradientColor = AppSettings.ColorToHex(grad.GradientStops[1].Color);
                    settings.LeftCanvasBackgroundOpacity = grad.GradientStops[1].Color.A / 255.0;
                }
                else if (LeftCanvas.Background is SolidColorBrush brush)
                {
                    settings.LeftCanvasIsGradient = false;
                    settings.LeftCanvasBackgroundColor = AppSettings.ColorToHex(Color.FromArgb(255, brush.Color.R, brush.Color.G, brush.Color.B));
                    settings.LeftCanvasBackgroundOpacity = brush.Color.A / 255.0;
                }
                else
                {
                    settings.LeftCanvasIsGradient = false;
                    settings.LeftCanvasBackgroundColor = AppSettings.ColorToHex(LeftCanvas.BackgroundColor);
                    settings.LeftCanvasBackgroundOpacity = 1.0;
                }
                
                settings.Save();
            }
            
            // Save Right Canvas settings
            if (RightCanvas != null && settings != null)
            {
                // Grid settings
                settings.RightCanvasGridLinesColor = AppSettings.ColorToHex(RightCanvas.GridLinesColor);
                settings.RightCanvasGridOpacity = SilderRightCanvasGridOpacity.Value / 100.0;
                settings.RightCanvasShowGrid = RightCanvas.ShowGrid;
                
                // Background settings
                if (RightCanvas.Background is LinearGradientBrush grad && grad.GradientStops.Count >= 2)
                {
                    settings.RightCanvasIsGradient = true;
                    settings.RightCanvasGradientColor = AppSettings.ColorToHex(grad.GradientStops[1].Color);
                    settings.RightCanvasBackgroundOpacity = grad.GradientStops[1].Color.A / 255.0;
                }
                else if (RightCanvas.Background is SolidColorBrush brush)
                {
                    settings.RightCanvasIsGradient = false;
                    settings.RightCanvasBackgroundColor = AppSettings.ColorToHex(Color.FromArgb(255, brush.Color.R, brush.Color.G, brush.Color.B));
                    settings.RightCanvasBackgroundOpacity = brush.Color.A / 255.0;
                }
                else
                {
                    settings.RightCanvasIsGradient = false;
                    settings.RightCanvasBackgroundColor = AppSettings.ColorToHex(RightCanvas.BackgroundColor);
                    settings.RightCanvasBackgroundOpacity = 1.0;
                }
                
                settings.Save();
            }
        }
        
        #endregion
        
        #region Context Menu
        
        private void SetupLeftCanvasContextMenu()
        {
            leftCanvasContextMenu = new System.Windows.Controls.ContextMenu { Style = (Style)FindResource("ModernContextMenuStyle") };
            
            var menuItems = new[]
            {
                new { Header = "Rename", Click = (RoutedEventHandler)menuRename_Click },
                new { Header = "Select All   →   Ctrl + A", Click = (RoutedEventHandler)null },
                new { Header = "Save   →   Ctrl + S", Click = (RoutedEventHandler)menuSave_Click },
                new { Header = "Copy   →   Ctrl + C", Click = (RoutedEventHandler)null },
                new { Header = "Paste   →   Ctrl + V", Click = (RoutedEventHandler)null },
                new { Header = "Undo   →   Ctrl + Z", Click = (RoutedEventHandler)null },
                new { Header = "Redo   →   Ctrl + Shift + Z", Click = (RoutedEventHandler)null },
                new { Header = "Rotate 90   →   F2", Click = (RoutedEventHandler)null },
                new { Header = "Flip    →   Ctrl + F3", Click = (RoutedEventHandler)null }
            };
            
            foreach (var item in menuItems)
            {
                var menuItem = new System.Windows.Controls.MenuItem 
                { 
                    Header = item.Header, 
                    Style = (Style)FindResource("ModernMenuItemStyle") 
                };
                
                if (item.Click != null)
                    menuItem.Click += item.Click;
                    
                leftCanvasContextMenu.Items.Add(menuItem);
            }
        }
        
        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            Save_Pattern dlg = new Save_Pattern();
            dlg.ShowDialog();
        }
        
        private void menuRename_Click(object sender, RoutedEventArgs e)
        {
            var selected = LeftCanvas.GetSelectedShapes();
            if (selected == null || selected.Count == 0)
            {
                System.Windows.MessageBox.Show("No shape selected!", "Rename", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (selected.Count > 1)
            {
                System.Windows.MessageBox.Show("You have to select only one shape.", "Rename", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var shape = selected[0];
            string currentName = "";
            if (shape is RectangleShape rect)
                currentName = rect.Name;
                
            var dlg = new Rename_Piece(currentName);
            dlg.Owner = this;
            dlg.ShowDialog();
            
            if (dlg.IsConfirmed && shape is RectangleShape rect2)
            {
                rect2.Name = dlg.NewName;
                LeftCanvas.InvalidateVisual();
            }
        }
        
        private void LeftCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selected = LeftCanvas.GetSelectedShapes();
            if (selected != null && selected.Count > 0)
            {
                leftCanvasContextMenu.PlacementTarget = LeftCanvas;
                leftCanvasContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
        
        #endregion
    }
} 