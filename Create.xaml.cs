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
using System.Windows.Forms; // لاستخدام ColorDialog
using Shaghouri;
using Microsoft.Win32;
using netDxf;
using netDxf.Entities;
using System.IO;
using WpfPoint = System.Windows.Point;
using DxfPoint = netDxf.Entities.Point;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using System.Text.Json;
using System.Globalization;

namespace Shaghouri
{
    /// <summary>
    /// Interaction logic for Create.xaml
    /// </summary>
    public partial class Create : Window
    {
        private AppSettings settings;
        private RectangleShape currentRectangleShape;
        private bool isDrawingRectangle = false;
        private WpfPoint rectangleStartPoint;
        // أضف متغير لحفظ نقطة النهاية
        private WpfPoint rectangleEndPoint;
        public List<RectangleShape> rectangles = new List<RectangleShape>();
        private bool IsShapeFillEnabled = true;
        private bool IsLineSizeEnabled = true;
        private bool IsAnchorPointsEnabled = true;
        private System.Windows.Controls.ContextMenu leftCanvasContextMenu;
        private void SetupLeftCanvasContextMenu()
        {
            leftCanvasContextMenu = new System.Windows.Controls.ContextMenu { Style = (Style)FindResource("ModernContextMenuStyle") };
            var menuRename = new System.Windows.Controls.MenuItem { Header = "Rename", Style = (Style)FindResource("ModernMenuItemStyle") };
            menuRename.Click += menuRename_Click;
            var menuSelectAll = new System.Windows.Controls.MenuItem { Header = "Select All   →   Ctrl + A", Style = (Style)FindResource("ModernMenuItemStyle") };
            var menuSave = new System.Windows.Controls.MenuItem { Header = "Save   →   Ctrl + S", Style = (Style)FindResource("ModernMenuItemStyle") };
            menuSave.Click += menuSave_Click;
            var menuCopy = new System.Windows.Controls.MenuItem { Header = "Copy   →   Ctrl + C", Style = (Style)FindResource("ModernMenuItemStyle") };
            var menuPaste = new System.Windows.Controls.MenuItem { Header = "Paste   →   Ctrl + V", Style = (Style)FindResource("ModernMenuItemStyle") };
            var menuUndo = new System.Windows.Controls.MenuItem { Header = "Undo   →   Ctrl + Z", Style = (Style)FindResource("ModernMenuItemStyle") };
            var menuRedo = new System.Windows.Controls.MenuItem { Header = "Redo   →   Ctrl + Shift + Z", Style = (Style)FindResource("ModernMenuItemStyle") };
            var menuRotate90 = new System.Windows.Controls.MenuItem { Header = "Rotate 90   →   F2", Style = (Style)FindResource("ModernMenuItemStyle") };
            var menuFlip90 = new System.Windows.Controls.MenuItem { Header = "Flip    →   Ctrl + F3", Style = (Style)FindResource("ModernMenuItemStyle") };
            leftCanvasContextMenu.Items.Add(menuRename);
            leftCanvasContextMenu.Items.Add(menuSelectAll);
            leftCanvasContextMenu.Items.Add(menuSave);
            leftCanvasContextMenu.Items.Add(menuCopy);
            leftCanvasContextMenu.Items.Add(menuPaste);
            leftCanvasContextMenu.Items.Add(menuUndo);
            leftCanvasContextMenu.Items.Add(menuRedo);
            leftCanvasContextMenu.Items.Add(menuRotate90);
            leftCanvasContextMenu.Items.Add(menuFlip90);
        }

        public Create()
        {
            InitializeComponent();
            this.DataContext = this;
            cm.IsChecked = true; // اجعل cm محددة تلقائياً
            SelectedUnit = UnitType.CM; // واجعلها الوحدة الافتراضية للرسم
            // الوضع الافتراضي: عرض اللوحتين معًا وبنفس العرض
            LeftCanvas.Visibility = Visibility.Visible;
            RightCanvas.Visibility = Visibility.Visible;
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            btnLeftCanvasTop.Click += BtnLeftCanvasTop_Click;
            btnRightCanvasTop.Click += BtnRightCanvasTop_Click;
            btnBothCanvasTop.Click += BtnBothCanvasTop_Click;
            btnLeftTop.Click += BtnLeftTop_Click;
            btnLeftCanvasToggleBackground.Click += btnLeftCanvasToggleBackground_Click;
            btn_Rectangle.Click += btn_Rectangle_Click;
            btnEsc.Click += btnEsc_Click;
            LeftCanvas.MouseLeftButtonDown += LeftCanvas_MouseLeftButtonDown;
            LeftCanvas.MouseMove += LeftCanvas_MouseMove;
            LeftCanvas.MouseLeftButtonUp += LeftCanvas_MouseLeftButtonUp;
            btnFill.Click += btnFill_Click;
            btnToggleSize.Click += btnToggleSize_Click;
            btnPointsShow_hide.Click += btnPointsShow_hide_Click;
            btnSizes.Click += btnSizes_Click; // ربط الحدث
            // احذف السطر التالي لأنه لم يعد هناك menuSave معرف كحقل:
            // menuSave.Click += menuSave_Click; // ربط حدث الحفظ
            // احذف السطر التالي لأنه لم يعد هناك menuRename معرف كحقل:
            // menuRename.Click += menuRename_Click;
            btnSe1.Click += btnSe1_Click;
            btnRecycleBin.Click += btnRecycleBin_Click; // ربط زر سلة المحذوفات
            settings = AppSettings.Load();
            ApplySettingsToUI();
            this.Closing += (s, e) => SaveSettingsFromUI();
            SetupLeftCanvasContextMenu();
            LeftCanvas.PreviewMouseRightButtonDown += LeftCanvas_PreviewMouseRightButtonDown;
            // Remove all code related to ShapePreviewCanvas custom drawing and event hooks
            // Instead, after rectangles.Add(currentRectangleShape);, set ShapePreviewControl.ShapeToPreview = currentRectangleShape;
        }

        private void BtnLeftCanvasTop_Click(object sender, RoutedEventArgs e)
        {
            LeftCanvas.Visibility = Visibility.Visible;
            RightCanvas.Visibility = Visibility.Collapsed;
            // اجعل العمود الأول يأخذ كل المساحة
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(0);
        }

        private void BtnRightCanvasTop_Click(object sender, RoutedEventArgs e)
        {
            LeftCanvas.Visibility = Visibility.Collapsed;
            RightCanvas.Visibility = Visibility.Visible;
            // اجعل العمود الثاني يأخذ كل المساحة
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(0);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }

        private void BtnBothCanvasTop_Click(object sender, RoutedEventArgs e)
        {
            LeftCanvas.Visibility = Visibility.Visible;
            RightCanvas.Visibility = Visibility.Visible;
            // اجعل كلا العمودين متساويين
            canvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            canvasGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
        }

        private void BtnLeftTop_Click(object sender, RoutedEventArgs e)
        {
            var parentGrid = (Grid)Left1.Parent;
            if (Left1.Visibility == Visibility.Visible)
            {
                Left1.Visibility = Visibility.Collapsed;
                parentGrid.ColumnDefinitions[0].Width = new GridLength(0);
            }
            else
            {
                Left1.Visibility = Visibility.Visible;
                parentGrid.ColumnDefinitions[0].Width = new GridLength(150);
            }
        }

        private void RightShow_Click(object sender, RoutedEventArgs e)
        {
            var parentGrid = (Grid)Right1.Parent;
            if (Right1.Visibility == Visibility.Visible)
            {
                Right1.Visibility = Visibility.Collapsed;
                parentGrid.ColumnDefinitions[2].Width = new GridLength(0);
            }
            else
            {
                Right1.Visibility = Visibility.Visible;
                parentGrid.ColumnDefinitions[2].Width = new GridLength(150);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {


        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnToggleSize_Click(object sender, RoutedEventArgs e)
        {
            IsLineSizeEnabled = !IsLineSizeEnabled;
            LeftCanvas.IsLineSizeEnabled = IsLineSizeEnabled;
            LeftCanvas.InvalidateVisual();
        }

        private void btnRightToggleSnap_Click(object sender, RoutedEventArgs e)
        {

        }

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
                RightCanvasBackgroundOpacityValue.Content = ((int)SilderRightCanvasbackgroundOpacity.Value).ToString();
            }
        }

        private void btnLeftCanvasChangeGridColor_Click(object sender, RoutedEventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                // تحويل اللون الحالي إلى System.Drawing.Color
                var currentColor = LeftCanvas != null ? LeftCanvas.GridLinesColor : System.Windows.Media.Colors.LightGray;
                colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // تحويل اللون المختار إلى System.Windows.Media.Color
                    var selected = colorDialog.Color;
                    if (LeftCanvas != null)
                        LeftCanvas.GridLinesColor = System.Windows.Media.Color.FromArgb(selected.A, selected.R, selected.G, selected.B);
                }
            }
        }

        private void btnLeftCanvasToggleBackground_Click(object sender, RoutedEventArgs e)
        {
            using (var colorDialog = new System.Windows.Forms.ColorDialog())
            {
                // Use last gradient color or default
                var lastColor = AppSettings.ParseColor(settings.LeftCanvasGradientColor);
                colorDialog.Color = System.Drawing.Color.FromArgb(lastColor.A, lastColor.R, lastColor.G, lastColor.B);
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var selected = colorDialog.Color;
                    var chosenColor = Color.FromArgb(selected.A, selected.R, selected.G, selected.B);
                    // Create gradient brush from white to chosen color
                    var gradient = new LinearGradientBrush();
                    gradient.StartPoint = new WpfPoint(0, 0);
                    gradient.EndPoint = new WpfPoint(1, 1);
                    gradient.GradientStops.Add(new GradientStop(Colors.White, 0));
                    gradient.GradientStops.Add(new GradientStop(chosenColor, 1));
                    LeftCanvas.Background = gradient;
                    // Save to settings
                    settings.LeftCanvasIsGradient = true;
                    settings.LeftCanvasGradientColor = AppSettings.ColorToHex(chosenColor);
                    settings.Save();
                }
            }
        }

        private void btnLeftCanvasChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            using (var colorDialog = new System.Windows.Forms.ColorDialog())
            {
                var currentColor = LeftCanvas != null ? LeftCanvas.BackgroundColor : System.Windows.Media.Colors.White;
                colorDialog.Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var selected = colorDialog.Color;
                    if (LeftCanvas != null)
                    {
                        var newColor = System.Windows.Media.Color.FromArgb(selected.A, selected.R, selected.G, selected.B);
                        LeftCanvas.BackgroundColor = newColor;
                        // Save to settings immediately
                        settings.LeftCanvasBackgroundColor = AppSettings.ColorToHex(newColor);
                        settings.LeftCanvasIsGradient = false; // Switch back to static
                        settings.Save();
                    }
                }
            }
        }

        private void TextBoxLeftCanvasGridSpacingWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LeftCanvas != null)
            {
                if (double.TryParse(TextBoxLeftCanvasGridSpacingWidth.Text, out double value))
                {
                    if (value < 0) value = 0;
                    if (value > 100) value = 100;
                    LeftCanvas.GridRectWidth = value * 10; // كل وحدة = 10 بكسل
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
                    LeftCanvas.GridRectHeight = value * 10; // كل وحدة = 10 بكسل
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

        private void btnPointsShow_hide_Click(object sender, RoutedEventArgs e)
        {
            IsAnchorPointsEnabled = !IsAnchorPointsEnabled;
            LeftCanvas.IsAnchorPointsEnabled = IsAnchorPointsEnabled;
            LeftCanvas.InvalidateVisual();
        }

        private void ApplySettingsToUI()
        {
            if (LeftCanvas != null && settings != null)
            {
                // عرض القيمة للمستخدم مقسومة على 10
                TextBoxLeftCanvasGridSpacingWidth.Text = (settings.GridRectWidth / 10.0).ToString();
                TextBoxLeftCanvasGridSpacingHeight.Text = (settings.GridRectHeight / 10.0).ToString();
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
                    gradient.StartPoint = new WpfPoint(0, 0);
                    gradient.EndPoint = new WpfPoint(1, 1);
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
                // Apply to UI controls
                // ... add more if needed
            }
        }

        private void SaveSettingsFromUI()
        {
            if (LeftCanvas != null && settings != null)
            {
                // احفظ القيمة المنطقية * 10
                if (double.TryParse(TextBoxLeftCanvasGridSpacingWidth.Text, out double widthValue))
                    settings.GridRectWidth = widthValue * 10;
                if (double.TryParse(TextBoxLeftCanvasGridSpacingHeight.Text, out double heightValue))
                    settings.GridRectHeight = heightValue * 10;
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
                // ... add more if needed
                settings.Save();
            }
        }

        private void btn_Rectangle_Click(object sender, RoutedEventArgs e)
        {
            isDrawingRectangle = true;
            currentRectangleShape = null;
            LeftCanvas.IsDrawingShape = true;
            LeftCanvas.ShapeToDraw = null;
            LeftCanvas.ShapesToDraw = rectangles;
            LeftCanvas.InvalidateVisual();
        }

        private void btnEsc_Click(object sender, RoutedEventArgs e)
        {
            isDrawingRectangle = false;
            LeftCanvas.IsDrawingShape = false;
            currentRectangleShape = null;
            LeftCanvas.ShapeToDraw = null;
            LeftCanvas.InvalidateVisual();
        }

        private bool isChangingGrainLineDirection = false;
        private void btnChangeGrainLineDirection_Click(object sender, RoutedEventArgs e)
        {
            isChangingGrainLineDirection = true;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.UpArrow; // Use up arrow cursor
        }

        private bool isScalingGrainLine = false;
        private Shape scalingShape = null;
        private double scalingStartLength = 0;
        private WpfPoint scalingStartPoint;
        private void btnScaleGrainLine_Click(object sender, RoutedEventArgs e)
        {
            isScalingGrainLine = true;
            LeftCanvas.IsScalingGrainLine = true;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.UpArrow; // Use up arrow cursor for scaling tool
            this.Focus(); // Force focus to window so Enter key is received
        }

        private void LeftCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isScalingGrainLine)
            {
                var mouseWorld = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
                foreach (var shape in LeftCanvas.ShapesToDraw)
                {
                    if (shape.GetGeometry().FillContains(mouseWorld))
                    {
                        scalingShape = shape;
                        scalingStartLength = shape.ArrowLength;
                        scalingStartPoint = mouseWorld;
                        shape.ArrowLengthPreview = shape.ArrowLength;
                        LeftCanvas.CaptureMouse();
                        break;
                    }
                }
                return;
            }
            if (isChangingGrainLineDirection)
            {
                var mouseWorld = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
                foreach (var shape in LeftCanvas.ShapesToDraw)
                {
                    if (shape.GetGeometry().FillContains(mouseWorld))
                    {
                        shape.ArrowAngle = (shape.ArrowAngle == 0) ? 90 : 0; // Toggle between 0 and +90 degrees
                        LeftCanvas.InvalidateVisual();
                        break;
                    }
                }
                isChangingGrainLineDirection = false;
                Mouse.OverrideCursor = null;
                return;
            }
            if (isDrawingRectangle)
            {
                var worldStart = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
                rectangleStartPoint = worldStart;
                currentRectangleShape = new RectangleShape(new Rect(rectangleStartPoint, new Size(1, 1)));
                LeftCanvas.ShapeToDraw = currentRectangleShape;
                LeftCanvas.CaptureMouse();
            }
        }

        private void LeftCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isScalingGrainLine && scalingShape != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var mouseWorld = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
                double delta;
                if (scalingShape.ArrowAngle == 90)
                    delta = mouseWorld.Y - scalingStartPoint.Y; // vertical drag for vertical arrow
                else
                    delta = mouseWorld.X - scalingStartPoint.X; // horizontal drag for horizontal arrow
                double newLength = scalingStartLength + delta;
                if (newLength < 10) newLength = 10;
                scalingShape.ArrowLengthPreview = newLength;
                LeftCanvas.InvalidateVisual();
                return;
            }
            // Prevent shape moving while scaling grain line
            if (isScalingGrainLine) return;
            if (isDrawingRectangle && currentRectangleShape != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var worldCurrent = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
                double move_x1 = rectangleStartPoint.X;
                double move_y1 = rectangleStartPoint.Y;
                double move_x2 = worldCurrent.X;
                double move_y2 = worldCurrent.Y;
                var move_anchorPoints = new List<AnchorPoint>();
                move_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(move_x1, move_y1)));
                move_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(move_x2, move_y1)));
                move_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(move_x2, move_y2)));
                move_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(move_x1, move_y2)));
                currentRectangleShape.AnchorPoints = move_anchorPoints;
                currentRectangleShape.Rect = new System.Windows.Rect(new System.Windows.Point(move_x1, move_y1), new System.Windows.Point(move_x2, move_y2));
                LeftCanvas.ShapeToDraw = currentRectangleShape;
                LeftCanvas.InvalidateVisual();
            }
        }

        private void LeftCanvas_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isScalingGrainLine && scalingShape != null)
            {
                // Keep preview, wait for Enter
                LeftCanvas.ReleaseMouseCapture();
                return;
            }
            if (isDrawingRectangle && currentRectangleShape != null)
            {
                var worldEnd = LeftCanvas.ScreenToWorld(e.GetPosition(LeftCanvas));
                rectangleEndPoint = worldEnd; // حفظ نقطة النهاية
                // اسم مؤقت
                string tempName = $"Rect{rectangles.Count + 1}";
                currentRectangleShape.Name = tempName;
                // تأكد أن العرض والارتفاع لا يساويان صفر
                double initialWidth = currentRectangleShape.Rect.Width == 0 ? 10 : currentRectangleShape.Rect.Width;
                double initialHeight = currentRectangleShape.Rect.Height == 0 ? 10 : currentRectangleShape.Rect.Height;
                // افتح نافذة Draw_Rectangle
                string unitStr = SelectedUnit == UnitType.MM ? "mm" : SelectedUnit == UnitType.INCH ? "inch" : "cm";
                double widthForDialog = initialWidth;
                double heightForDialog = initialHeight;
                switch (SelectedUnit)
                {
                    case UnitType.CM:
                        widthForDialog = initialWidth / 10.0;
                        heightForDialog = initialHeight / 10.0;
                        break;
                    case UnitType.INCH:
                        widthForDialog = initialWidth / 25.4;
                        heightForDialog = initialHeight / 25.4;
                        break;
                    // MM: no change
                }
                var dlg = new Draw_Rectangle(currentRectangleShape.Name, widthForDialog, heightForDialog, unitStr);
                dlg.Owner = this;
                dlg.ShowDialog();
                if (dlg.IsConfirmed)
                {
                    // Convert width and height to mm if needed
                    double widthInMM = dlg.RectangleWidth;
                    double heightInMM = dlg.RectangleHeight;
                    switch (SelectedUnit)
                    {
                        case UnitType.CM:
                            widthInMM = dlg.RectangleWidth * 10.0;
                            heightInMM = dlg.RectangleHeight * 10.0;
                            break;
                        case UnitType.INCH:
                            widthInMM = dlg.RectangleWidth * 25.4;
                            heightInMM = dlg.RectangleHeight * 25.4;
                            break;
                        // MM: no change
                    }
                    // عند تأكيد الأبعاد في نافذة الإدخال (بعد إدخال القيم)
                    // استخدم اتجاه السحب الأصلي لتحديد الزاوية الأولى
                    var dx = rectangleEndPoint.X - rectangleStartPoint.X;
                    var dy = rectangleEndPoint.Y - rectangleStartPoint.Y;
                    double x1, y1, x2, y2;
                    if (dx >= 0 && dy >= 0) // سحب لليمين والأسفل
                    {
                        x1 = rectangleStartPoint.X;
                        y1 = rectangleStartPoint.Y;
                        x2 = x1 + widthInMM;
                        y2 = y1 + heightInMM;
                    }
                    else if (dx < 0 && dy < 0) // سحب لليسار وللأعلى
                    {
                        x2 = rectangleStartPoint.X;
                        y2 = rectangleStartPoint.Y;
                        x1 = x2 - widthInMM;
                        y1 = y2 - heightInMM;
                    }
                    else if (dx >= 0 && dy < 0) // سحب لليمين وللأعلى
                    {
                        x1 = rectangleStartPoint.X;
                        y2 = rectangleStartPoint.Y;
                        x2 = x1 + widthInMM;
                        y1 = y2 - heightInMM;
                    }
                    else // dx < 0 && dy >= 0 // سحب لليسار وللأسفل
                    {
                        x2 = rectangleStartPoint.X;
                        y1 = rectangleStartPoint.Y;
                        x1 = x2 - widthInMM;
                        y2 = y1 + heightInMM;
                    }
                    var dialog_anchorPoints = new List<AnchorPoint>();
                    dialog_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(x1, y1)));
                    dialog_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(x2, y1)));
                    dialog_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(x2, y2)));
                    dialog_anchorPoints.Add(new AnchorPoint(new System.Windows.Point(x1, y2)));
                    currentRectangleShape.AnchorPoints = dialog_anchorPoints;
                    currentRectangleShape.Rect = new System.Windows.Rect(new System.Windows.Point(x1, y1), new System.Windows.Point(x2, y2));
                    currentRectangleShape.Name = dlg.RectangleName;
                    currentRectangleShape.ArrowLength = widthInMM * 0.3;
                    rectangles.Add(currentRectangleShape);
                    ShapePreviewsList.ItemsSource = null;
                    ShapePreviewsList.ItemsSource = rectangles;
                }
                // إذا لم يتم التأكيد لا تضف المستطيل
                LeftCanvas.ShapesToDraw = rectangles;
                currentRectangleShape = null;
                LeftCanvas.ShapeToDraw = null;
                LeftCanvas.ReleaseMouseCapture();
                LeftCanvas.InvalidateVisual();
                // أوقف وضع الرسم بعد رسم مستطيل واحد
                isDrawingRectangle = false;
                LeftCanvas.IsDrawingShape = false;
            }
        }

        private void btnFill_Click(object sender, RoutedEventArgs e)
        {
            IsShapeFillEnabled = !IsShapeFillEnabled;
            LeftCanvas.IsShapeFillEnabled = IsShapeFillEnabled;
            LeftCanvas.InvalidateVisual();
        }

        private void btnSizes_Click(object sender, RoutedEventArgs e)
        {
            Sizes dlg = new Sizes();
            dlg.ShowDialog();
        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            Save_Pattern dlg = new Save_Pattern();
            dlg.ShowDialog();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "HCAD files (*.hcad)|*.hcad";
            dialog.DefaultExt = "hcad";
            if (dialog.ShowDialog() != true)
                return;

            // Map to DTOs
            var projectDto = new HcadProjectDto
            {
                Rectangles = rectangles.Select(r => new RectangleShapeDto
                {
                    X = r.Rect.X,
                    Y = r.Rect.Y,
                    Width = r.Rect.Width,
                    Height = r.Rect.Height,
                    Name = r.Name,
                    ArrowAngle = r.ArrowAngle,
                    ArrowLength = r.ArrowLength,
                    AnchorPoints = r.AnchorPoints.Select(a => new AnchorPointDto { X = a.Position.X, Y = a.Position.Y }).ToList()
                }).ToList()
            };

            // Serialize to JSON and encode as Base64
            string json = JsonSerializer.Serialize(projectDto, new JsonSerializerOptions { WriteIndented = true });
            string encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

            // Write to file
            System.IO.File.WriteAllText(dialog.FileName, encoded);

            System.Windows.MessageBox.Show("Project saved successfully!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "HCAD files (*.hcad)|*.hcad";
            dialog.DefaultExt = "hcad";
            if (dialog.ShowDialog() != true)
                return;

            // Read and decode Base64
            string encoded = System.IO.File.ReadAllText(dialog.FileName);
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var projectDto = JsonSerializer.Deserialize<HcadProjectDto>(json);

            // Convert DTOs to RectangleShape objects
            rectangles.Clear();
            if (projectDto?.Rectangles != null)
            {
                foreach (var dto in projectDto.Rectangles)
                {
                    var rect = new Rect(dto.X, dto.Y, dto.Width, dto.Height);
                    var shape = new RectangleShape(rect)
                    {
                        Name = dto.Name,
                        ArrowAngle = dto.ArrowAngle,
                        ArrowLength = dto.ArrowLength,
                        AnchorPoints = dto.AnchorPoints.Select(a => new AnchorPoint(new WpfPoint(a.X, a.Y))).ToList()
                    };
                    rectangles.Add(shape);
                }
            }

            // Redraw
            LeftCanvas.ShapesToDraw = rectangles;
            LeftCanvas.InvalidateVisual();
            ShapePreviewsList.ItemsSource = null;
            ShapePreviewsList.ItemsSource = rectangles;

            System.Windows.MessageBox.Show("Project loaded successfully!", "Open", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void btnHpgl_Click(object sender, RoutedEventArgs e)
        {
            // 1. Show SaveFileDialog for DXF
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "DXF files (*.dxf)|*.dxf";
            dialog.DefaultExt = "dxf";
            if (dialog.ShowDialog() != true)
                return;

            string dxfPath = dialog.FileName;

            // 2. Create a new DXF document
            DxfDocument dxf = new DxfDocument();

            // 3. Prepare HPGL commands
            var hpgl = new System.Text.StringBuilder();
            hpgl.AppendLine("IN;"); // Initialize
            hpgl.AppendLine("SP1;"); // Select pen 1

            // 4. Export only selected RectangleShape objects
            var selectedRectangles = LeftCanvas.GetSelectedShapes();
            if (selectedRectangles == null || selectedRectangles.Count == 0)
            {
                System.Windows.MessageBox.Show("No shapes selected!", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            foreach (var shape in selectedRectangles)
            {
                if (shape is RectangleShape rect)
                {
                    // Convert pixel to cm (10px = 1cm)
                    double x = rect.Rect.Left / 10.0;
                    double y = rect.Rect.Top / 10.0;
                    double w = rect.Rect.Width / 10.0;
                    double h = rect.Rect.Height / 10.0;

                    // DXF: Create rectangle as a closed polyline
                    var poly = new netDxf.Entities.LwPolyline(new[]
                    {
                        new netDxf.Entities.LwPolylineVertex(x, y),
                        new netDxf.Entities.LwPolylineVertex(x + w, y),
                        new netDxf.Entities.LwPolylineVertex(x + w, y + h),
                        new netDxf.Entities.LwPolylineVertex(x, y + h)
                    }, true);
                    dxf.AddEntity(poly);

                    // HPGL: 1cm = 400 units
                    int x1 = (int)(x * 400);
                    int y1 = (int)(y * 400);
                    int x2 = (int)((x + w) * 400);
                    int y2 = (int)((y + h) * 400);
                    // Move to start
                    hpgl.AppendLine($"PU{x1},{y1};");
                    // Draw rectangle
                    hpgl.AppendLine($"PD{x2},{y1},{x2},{y2},{x1},{y2},{x1},{y1};");
                    hpgl.AppendLine("PU;"); // Pen up
                }
            }

            // 5. Save the DXF file
            dxf.Save(dxfPath);

            // 6. Save HPGL file (same location, .plt extension)
            string hpglPath = System.IO.Path.ChangeExtension(dxfPath, ".plt");
            System.IO.File.WriteAllText(hpglPath, hpgl.ToString());

            // 7. Show success message
            System.Windows.MessageBox.Show("Exported selected DXF and HPGL files successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnSe1_Click(object sender, RoutedEventArgs e)
        {
            var drawRectWindow = new Draw_Rectangle();
            drawRectWindow.Owner = this;
            drawRectWindow.ShowDialog();
        }

        // فتح نافذة سلة المحذوفات عند الضغط على الزر
        private void btnRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            var recycleBinWindow = new Recycle_Bin();
            recycleBinWindow.Owner = this;
            recycleBinWindow.ShowDialog();
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
            // إذا لم يكن هناك تحديد، لا تظهر القائمة
        }

        private void Window_KeyDown(object sender, WpfKeyEventArgs e)
        {
            if (isScalingGrainLine && scalingShape != null && e.Key == Key.Enter)
            {
                scalingShape.ArrowLength = scalingShape.ArrowLengthPreview ?? scalingShape.ArrowLength;
                scalingShape.ArrowLengthPreview = null;
                isScalingGrainLine = false;
                LeftCanvas.IsScalingGrainLine = false;
                scalingShape = null;
                Mouse.OverrideCursor = null;
                LeftCanvas.InvalidateVisual();
                e.Handled = true;
                return;
            }
        }

        public enum UnitType { MM, CM, INCH }
        public UnitType SelectedUnit = UnitType.CM; // Default to cm

        private void UnitCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == mm)
            {
                cm.IsChecked = false;
                inch.IsChecked = false;
                SelectedUnit = UnitType.MM;
            }
            else if (sender == cm)
            {
                mm.IsChecked = false;
                inch.IsChecked = false;
                SelectedUnit = UnitType.CM;
            }
            else if (sender == inch)
            {
                mm.IsChecked = false;
                cm.IsChecked = false;
                SelectedUnit = UnitType.INCH;
            }
            UpdateShapeDimensionDisplay();
        }

        private void UpdateShapeDimensionDisplay()
        {
            // Example: update the TextBoxes or labels that show shape dimensions
            // You may need to call this after drawing or selecting a shape
            foreach (var rect in rectangles)
            {
                double width = rect.Rect.Width;
                double height = rect.Rect.Height;
                string unit = "";
                switch (SelectedUnit)
                {
                    case UnitType.MM:
                        width = width * 1.0;
                        height = height * 1.0;
                        unit = "mm";
                        break;
                    case UnitType.CM:
                        width = width / 10.0;
                        height = height / 10.0;
                        unit = "cm";
                        break;
                    case UnitType.INCH:
                        width = width / 25.4;
                        height = height / 25.4;
                        unit = "inch";
                        break;
                }
                // Update your UI here, e.g. set TextBox or Label text
                // Example: rect.DimensionLabel.Text = $"{width:F2} x {height:F2} {unit}";
                // If you have a selected shape, update its display
            }
            // If you have a selected shape, update its display as well
            // If you show dimensions on the canvas, you may need to invalidate/redraw
            LeftCanvas.InvalidateVisual();
        }

        // Add a field to store the last drawn shape for preview
        private Shape lastShapeForPreview = null;

        // Remove: lastShapeForPreview, ShapePreviewCanvas_OnRender, and all ShapePreviewCanvas event hooks
        private Shape _selectedShape;
        public Shape selectedShape
        {
            get => _selectedShape;
            set
            {
                _selectedShape = value;
                System.Diagnostics.Debug.WriteLine($"[Create] selectedShape property set to: {_selectedShape}");
            }
        }

        private void ShapePreview_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        public List<Shape> SelectedShapes => LeftCanvas?.GetSelectedShapes();
    }

    public class CanvasBase : Canvas
    {
        // يمكنك إضافة خصائص أو دوال مشتركة هنا لاحقاً
    }

    public class LeftCanvas : CanvasBase
    {
        // يمكنك تخصيص منطق أو خصائص خاصة بلوحة اليسار هنا
    }
    public class RightCanvas : CanvasBase
    {
        // يمكنك تخصيص منطق أو خصائص خاصة بلوحة اليمين هنا
    }

    public class RectangleShapeDto
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Name { get; set; }
        public double ArrowAngle { get; set; }
        public double ArrowLength { get; set; }
        public List<AnchorPointDto> AnchorPoints { get; set; }
    }
    public class AnchorPointDto
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    public class HcadProjectDto
    {
        public List<RectangleShapeDto> Rectangles { get; set; }
    }
}
