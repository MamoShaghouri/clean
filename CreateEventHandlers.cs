using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Shaghouri.Services;
using Shaghouri.Models;
using Shaghouri;

namespace Shaghouri
{
    /// <summary>
    /// معالجات الأحداث للواجهة الرئيسية
    /// </summary>
    public partial class Create
    {
        #region Custom Color Picker
        
        private void ShowCustomColorPicker(Color initialColor, Action<Color> onColorSelected)
        {
            var colorPicker = new CustomColorPicker();
            colorPicker.SetInitialColor(initialColor);
            
            var window = new Window
            {
                Title = "اختيار اللون",
                Content = colorPicker,
                Width = 450,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this, // اجعل المالك نافذة Create الحالية
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow,
                Topmost = true
            };
            
            colorPicker.ColorSelected += (sender, color) =>
            {
                onColorSelected(color);
                window.Close();
            };
            
            colorPicker.Canceled += (sender, e) =>
            {
                window.Close();
            };
            
            window.ShowDialog();
        }
        
        #endregion
        #region Canvas Control Events
        
        private void btnLeftCanvasTop_Click(object sender, RoutedEventArgs e)
        {
            _canvasService.ShowOnlyLeftCanvas(canvasGrid, LeftCanvas, RightCanvas);
        }
        
        private void btnRightCanvasTop_Click(object sender, RoutedEventArgs e)
        {
            _canvasService.ShowOnlyRightCanvas(canvasGrid, LeftCanvas, RightCanvas);
        }
        
        private void btnBothCanvasTop_Click(object sender, RoutedEventArgs e)
        {
            _canvasService.ShowBothCanvases(canvasGrid, LeftCanvas, RightCanvas);
        }
        
        private void btnLeftTop_Click(object sender, RoutedEventArgs e)
        {
            _canvasService.ToggleLeftPanel(Left1, (Grid)Left1.Parent);
        }
        
        private void btnRightShow_Click(object sender, RoutedEventArgs e)
        {
            // يمكنك إضافة منطق إظهار/إخفاء اللوحة اليمنى هنا
            MessageBox.Show("Right panel functionality will be implemented later.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        #endregion
        
        #region Drawing Events
        
        private void btnRectangle_Click(object sender, RoutedEventArgs e)
        {
            if (_drawingState.IsDrawingActive)
            {
                MessageBox.Show("يرجى إكمال العملية الحالية أولاً", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _drawingState.IsDrawingRectangle = true;
            Mouse.OverrideCursor = Cursors.Cross;
            
            // Update button appearance
            btnRectangle.Background = new SolidColorBrush(Colors.Red);
            btnRectangle.Foreground = new SolidColorBrush(Colors.White);
        }
        
        private void btnEsc_Click(object sender, RoutedEventArgs e)
        {
            _drawingService.CancelDrawing();
            UpdateCanvasDrawingState();
        }
        
        private void LeftCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMouseLeftButtonDown(e);
        }
        
        private void LeftCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HandleMouseMove(e);
        }
        
        private void LeftCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleMouseLeftButtonUp(e);
        }
        
        #endregion
        
        #region Other Button Events
        
        private void btnFill_Click(object sender, RoutedEventArgs e)
        {
            IsShapeFillEnabled = !IsShapeFillEnabled;
            LeftCanvas.IsShapeFillEnabled = IsShapeFillEnabled;
            LeftCanvas.InvalidateVisual();
        }
        
        private void btnToggleSize_Click(object sender, RoutedEventArgs e)
        {
            IsLineSizeEnabled = !IsLineSizeEnabled;
            LeftCanvas.IsLineSizeEnabled = IsLineSizeEnabled;
            LeftCanvas.InvalidateVisual();
        }
        
        private void btnPointsShow_hide_Click(object sender, RoutedEventArgs e)
        {
            IsAnchorPointsEnabled = !IsAnchorPointsEnabled;
            LeftCanvas.IsAnchorPointsEnabled = IsAnchorPointsEnabled;
            LeftCanvas.InvalidateVisual();
        }
        
        private void btnSizes_Click(object sender, RoutedEventArgs e)
        {
            Sizes dlg = new Sizes();
            dlg.ShowDialog();
        }
        
        private void btnSe1_Click(object sender, RoutedEventArgs e)
        {
            var drawRectWindow = new Draw_Rectangle();
            drawRectWindow.Owner = this;
            drawRectWindow.ShowDialog();
        }
        
        private void btnRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            var recycleBinWindow = new Recycle_Bin();
            recycleBinWindow.Owner = this;
            recycleBinWindow.ShowDialog();
        }
        
        private void btnHpgl_Click(object sender, RoutedEventArgs e)
        {
            // تصدير إلى HPGL
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "HPGL files (*.hpgl)|*.hpgl";
            dialog.DefaultExt = "hpgl";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _fileService.ExportToHPGL(rectangles, dialog.FileName);
                    System.Windows.MessageBox.Show("HPGL exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error exporting HPGL: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        #endregion
        
        #region Grain Line Button Events
        
        private void btnChangeGrainLineDirection_Click(object sender, RoutedEventArgs e)
        {
            _drawingState.IsChangingGrainLineDirection = true;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.UpArrow;
        }
        
        private void btnScaleGrainLine_Click(object sender, RoutedEventArgs e)
        {
            _drawingState.IsScalingGrainLine = true;
            LeftCanvas.IsScalingGrainLine = true;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.UpArrow;
            this.Focus();
        }
        
        #endregion
        
        #region Grid and Background Controls
        
        private void btnLeftCanvasChangeGridColor_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = LeftCanvas != null ? LeftCanvas.GridLinesColor : Colors.LightGray;
            ShowCustomColorPicker(currentColor, (selectedColor) =>
            {
                if (LeftCanvas != null)
                    LeftCanvas.GridLinesColor = selectedColor;
            });
        }
        
        private void btnLeftCanvasToggleBackground_Click(object sender, RoutedEventArgs e)
        {
            var lastColor = AppSettings.ParseColor(settings.LeftCanvasGradientColor);
            ShowCustomColorPicker(lastColor, (chosenColor) =>
            {
                // Create gradient brush from white to chosen color
                var gradient = new LinearGradientBrush();
                gradient.StartPoint = new Point(0, 0);
                gradient.EndPoint = new Point(1, 1);
                gradient.GradientStops.Add(new GradientStop(Colors.White, 0));
                gradient.GradientStops.Add(new GradientStop(chosenColor, 1));
                LeftCanvas.Background = gradient;
                
                // Save to settings
                settings.LeftCanvasIsGradient = true;
                settings.LeftCanvasGradientColor = AppSettings.ColorToHex(chosenColor);
                settings.Save();
            });
        }
        
        private void btnLeftCanvasChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = LeftCanvas != null ? LeftCanvas.BackgroundColor : Colors.White;
            ShowCustomColorPicker(currentColor, (newColor) =>
            {
                if (LeftCanvas != null)
                {
                    LeftCanvas.BackgroundColor = newColor;
                    
                    // Save to settings immediately
                    settings.LeftCanvasBackgroundColor = AppSettings.ColorToHex(newColor);
                    settings.LeftCanvasIsGradient = false;
                    settings.Save();
                }
            });
        }
        
        #endregion
        
        #region Grid Controls
        
        private void TextBoxLeftCanvasGridSpacingWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LeftCanvas != null)
            {
                if (double.TryParse(TextBoxLeftCanvasGridSpacingWidth.Text, out double value))
                {
                    if (value < 0) value = 0;
                    if (value > 100) value = 100;
                    LeftCanvas.GridRectWidth = UnitService.ConvertGridSpacingToPixels(value);
                }
            }
        }
        
        private void TextBoxLeftCanvasGridSpacingHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LeftCanvas != null)
            {
                if (double.TryParse(TextBoxLeftCanvasGridSpacingHeight.Text, out double value))
                {
                    if (value < 1) value = 1;
                    if (value > 100) value = 100;
                    LeftCanvas.GridRectHeight = UnitService.ConvertGridSpacingToPixels(value);
                }
            }
        }
        
        private void btnLeftCanvasShow_HideGrid_Click(object sender, RoutedEventArgs e)
        {
            if (LeftCanvas != null)
            {
                LeftCanvas.ShowGrid = !LeftCanvas.ShowGrid;
            }
        }
        
        private void btnRighttCanvasShow_HideGrid_Click(object sender, RoutedEventArgs e)
        {
            if (RightCanvas != null)
            {
                RightCanvas.ShowGrid = !RightCanvas.ShowGrid;
            }
        }
        
        private void TextBoxRightCanvasGridSpacingWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RightCanvas != null)
            {
                if (double.TryParse(TextBoxRightCanvasGridSpacingWidth.Text, out double value))
                {
                    if (value < 0) value = 0;
                    if (value > 100) value = 100;
                    RightCanvas.GridRectWidth = UnitService.ConvertGridSpacingToPixels(value);
                }
            }
        }
        
        private void TextBoxRightCanvasGridSpacingHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RightCanvas != null)
            {
                if (double.TryParse(TextBoxRightCanvasGridSpacingHeight.Text, out double value))
                {
                    if (value < 1) value = 1;
                    if (value > 100) value = 100;
                    RightCanvas.GridRectHeight = UnitService.ConvertGridSpacingToPixels(value);
                }
            }
        }
        
        #endregion
        
        #region Slider Controls
        
        private void SilderLeftCanvasGridOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LeftCanvasGridOpacityValue != null && SilderLeftCanvasGridOpacity != null && LeftCanvas != null)
            {
                int value = (int)SilderLeftCanvasGridOpacity.Value;
                LeftCanvasGridOpacityValue.Content = value.ToString();
                
                // Set grid line opacity (alpha channel)
                var color = LeftCanvas.GridLinesColor;
                LeftCanvas.GridLinesColor = Color.FromArgb((byte)(255 * value / 100), color.R, color.G, color.B);
            }
        }
        
        private void SilderRightCanvasGridOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RightCanvasGridOpacityValue != null && SilderRightCanvasGridOpacity != null)
            {
                RightCanvasGridOpacityValue.Content = ((int)SilderRightCanvasGridOpacity.Value).ToString();
            }
        }
        
        private void SilderLeftCanvasbackgroundOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LeftCanvasBackgroundOpacityValue != null && SilderLeftCanvasbackgroundOpacity != null && LeftCanvas != null && settings != null)
            {
                int value = (int)SilderLeftCanvasbackgroundOpacity.Value;
                LeftCanvasBackgroundOpacityValue.Content = value.ToString();
                double opacity = value / 100.0;
                
                if (LeftCanvas.Background is LinearGradientBrush grad && grad.GradientStops.Count >= 2)
                {
                    // Update both stops' alpha
                    var color0 = grad.GradientStops[0].Color;
                    var color1 = grad.GradientStops[1].Color;
                    grad.GradientStops[0].Color = Color.FromArgb((byte)(opacity * 255), color0.R, color0.G, color0.B);
                    grad.GradientStops[1].Color = Color.FromArgb((byte)(opacity * 255), color1.R, color1.G, color1.B);
                    LeftCanvas.Background = grad;
                }
                else if (LeftCanvas.Background is SolidColorBrush brush)
                {
                    LeftCanvas.Background = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), brush.Color.R, brush.Color.G, brush.Color.B));
                }
                else
                {
                    var color = LeftCanvas.BackgroundColor;
                    LeftCanvas.Background = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B));
                }
                
                // Save to settings
                settings.LeftCanvasBackgroundOpacity = opacity;
                settings.Save();
            }
        }
        
        private void SilderRightCanvasbackgroundOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RightCanvasBackgroundOpacityValue != null && SilderRightCanvasbackgroundOpacity != null)
            {
                int value = (int)SilderRightCanvasbackgroundOpacity.Value;
                RightCanvasBackgroundOpacityValue.Content = value.ToString();
                
                // Apply opacity to RightCanvas background
                if (RightCanvas != null)
                {
                    double opacity = value / 100.0;
                    
                    if (RightCanvas.Background is LinearGradientBrush grad && grad.GradientStops.Count >= 2)
                    {
                        // Update both stops' alpha
                        var color0 = grad.GradientStops[0].Color;
                        var color1 = grad.GradientStops[1].Color;
                        grad.GradientStops[0].Color = Color.FromArgb((byte)(opacity * 255), color0.R, color0.G, color0.B);
                        grad.GradientStops[1].Color = Color.FromArgb((byte)(opacity * 255), color1.R, color1.G, color1.B);
                        RightCanvas.Background = grad;
                    }
                    else if (RightCanvas.Background is SolidColorBrush brush)
                    {
                        RightCanvas.Background = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), brush.Color.R, brush.Color.G, brush.Color.B));
                    }
                    else
                    {
                        var color = RightCanvas.BackgroundColor;
                        RightCanvas.Background = new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B));
                    }
                }
            }
        }
        
        #endregion
        
        #region Unit Management
        
        private void UnitCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == mm)
            {
                cm.IsChecked = false;
                inch.IsChecked = false;
                SelectedUnit = UnitService.UnitType.MM;
            }
            else if (sender == cm)
            {
                mm.IsChecked = false;
                inch.IsChecked = false;
                SelectedUnit = UnitService.UnitType.CM;
            }
            else if (sender == inch)
            {
                mm.IsChecked = false;
                cm.IsChecked = false;
                SelectedUnit = UnitService.UnitType.INCH;
            }
            
            // تحديث DPI عند تغيير الوحدة
            UnitService.UpdateDpiScaleFromWindow(this);
            UpdateShapeDimensionDisplay();
        }
        
        #endregion
        
        #region Keyboard Events
        
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_drawingState.IsScalingGrainLine && _drawingState.ScalingShape != null && e.Key == Key.Enter)
            {
                _grainLineService.ConfirmGrainLineScaling(_drawingState.ScalingShape);
                _drawingState.IsScalingGrainLine = false;
                LeftCanvas.IsScalingGrainLine = false;
                _drawingState.ScalingShape = null;
                Mouse.OverrideCursor = null;
                LeftCanvas.InvalidateVisual();
                e.Handled = true;
                return;
            }
        }
        
        #endregion
        
        #region Right Canvas Controls
        
        private void btnRighttCanvasChangeGridColor_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = RightCanvas != null ? RightCanvas.GridLinesColor : Colors.LightGray;
            ShowCustomColorPicker(currentColor, (selectedColor) =>
            {
                if (RightCanvas != null)
                    RightCanvas.GridLinesColor = selectedColor;
            });
        }
        
        private void btnRightCanvasToggleBackground_Click(object sender, RoutedEventArgs e)
        {
            var lastColor = AppSettings.ParseColor("#FFBEE6E6");
            ShowCustomColorPicker(lastColor, (chosenColor) =>
            {
                // Create gradient brush from white to chosen color
                var gradient = new LinearGradientBrush();
                gradient.StartPoint = new Point(0, 0);
                gradient.EndPoint = new Point(1, 1);
                gradient.GradientStops.Add(new GradientStop(Colors.White, 0));
                gradient.GradientStops.Add(new GradientStop(chosenColor, 1));
                RightCanvas.Background = gradient;
            });
        }
        
        private void btnRightCanvasChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = RightCanvas != null ? RightCanvas.BackgroundColor : Colors.White;
            ShowCustomColorPicker(currentColor, (newColor) =>
            {
                if (RightCanvas != null)
                {
                    RightCanvas.BackgroundColor = newColor;
                    RightCanvas.Background = new SolidColorBrush(newColor);
                }
            });
        }
        
        #endregion
        
        #region Shape Preview
        
        private void ShapePreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ShapePreviewControl preview && preview.ShapeToPreview != null)
            {
                selectedShape = preview.ShapeToPreview;
                System.Diagnostics.Debug.WriteLine($"[Create] selectedShape set to: {selectedShape}");
                LeftCanvas.SelectedShape = selectedShape;
                LeftCanvas.ClearAndSelectShape(selectedShape);
                LeftCanvas.InvalidateVisual();
                ShapePreviewsList.Items.Refresh();
            }
        }
        
        #endregion
    }
} 