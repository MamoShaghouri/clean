using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic; // Added for List
using System; // Added for Math
using WpfPoint = System.Windows.Point;

namespace Shaghouri
{
    public class CustomGridControl : Canvas
    {
        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(CustomGridControl), new PropertyMetadata(1.0, OnVisualPropertyChanged));

        public Color GridLinesColor
        {
            get { return (Color)GetValue(GridLinesColorProperty); }
            set { SetValue(GridLinesColorProperty, value); }
        }
        public static readonly DependencyProperty GridLinesColorProperty =
            DependencyProperty.Register("GridLinesColor", typeof(Color), typeof(CustomGridControl), new PropertyMetadata(Colors.LightGray, OnVisualPropertyChanged));

        public new double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }
        public static new readonly DependencyProperty OpacityProperty =
            DependencyProperty.Register("Opacity", typeof(double), typeof(CustomGridControl), new PropertyMetadata(1.0, OnVisualPropertyChanged));

        public double GridRectWidth
        {
            get { return (double)GetValue(GridRectWidthProperty); }
            set { SetValue(GridRectWidthProperty, value); }
        }
        public static readonly DependencyProperty GridRectWidthProperty =
            DependencyProperty.Register("GridRectWidth", typeof(double), typeof(CustomGridControl), new PropertyMetadata(10.0, OnVisualPropertyChanged));

        public double GridRectHeight
        {
            get { return (double)GetValue(GridRectHeightProperty); }
            set { SetValue(GridRectHeightProperty, value); }
        }
        public static readonly DependencyProperty GridRectHeightProperty =
            DependencyProperty.Register("GridRectHeight", typeof(double), typeof(CustomGridControl), new PropertyMetadata(10.0, OnVisualPropertyChanged));

        public bool ShowGrid
        {
            get { return (bool)GetValue(ShowGridProperty); }
            set { SetValue(ShowGridProperty, value); }
        }
        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register("ShowGrid", typeof(bool), typeof(CustomGridControl), new PropertyMetadata(true, OnVisualPropertyChanged));

        public Color BackgroundColor
        {
            get
            {
                if (Background is SolidColorBrush brush)
                    return brush.Color;
                return Colors.White;
            }
            set
            {
                Background = new SolidColorBrush(value);
            }
        }

        // Add a property to hold a shape to draw
        public Shape ShapeToDraw { get; set; }

        public List<RectangleShape> ShapesToDraw { get; set; } = new List<RectangleShape>();

        public bool IsDrawingShape { get; set; } = false;

        public bool IsShapeFillEnabled { get; set; } = true;

        public bool IsLineSizeEnabled { get; set; } = true;

        public bool IsAnchorPointsEnabled { get; set; } = true;

        public bool IsScalingGrainLine { get; set; } = false;

        public Shape SelectedShape { get; set; }

        private double panOffsetX = 0;
        private double panOffsetY = 0;
        private WpfPoint? lastPanPoint;
        private ScaleTransform zoomTransform = new ScaleTransform(1, 1);
        private Shape selectedShape = null;
        private WpfPoint shapeDragStartWorld;
        private List<WpfPoint> originalAnchorPositions;
        private List<Shape> selectedShapes = new List<Shape>();
        private bool isMarqueeSelecting = false;
        private WpfPoint marqueeStartWorld;
        private WpfPoint marqueeEndWorld;
        private WpfPoint marqueeStartScreen;
        private WpfPoint marqueeEndScreen;
        private Shaghouri.Create parentCreateWindow => System.Windows.Window.GetWindow(this) as Shaghouri.Create;

        public CustomGridControl()
        {
            this.MouseWheel += CustomGridControl_MouseWheel;
            this.MouseLeftButtonDown += CustomGridControl_MouseLeftButtonDown;
            this.MouseLeftButtonUp += CustomGridControl_MouseLeftButtonUp;
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CustomGridControl;
            ctrl?.InvalidateVisual();
        }

        public WpfPoint ScreenToWorld(WpfPoint screenPoint)
        {
            // Undo pan, then undo zoom
            return new WpfPoint((screenPoint.X - panOffsetX) / ZoomLevel, (screenPoint.Y - panOffsetY) / ZoomLevel);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            // Clip all drawing to the visible canvas area (push after base.OnRender)
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
            // Apply pan and zoom transforms to all drawing
            dc.PushTransform(new TranslateTransform(panOffsetX, panOffsetY));
            dc.PushTransform(new ScaleTransform(ZoomLevel, ZoomLevel));

            if (ShowGrid)
            {
                double stepX = GridRectWidth;
                double stepY = GridRectHeight;
                Pen gridPen = new Pen(new SolidColorBrush(GridLinesColor), 1.0 / ZoomLevel);
                gridPen.Freeze();

                // Calculate visible world bounds
                WpfPoint topLeft = ScreenToWorld(new WpfPoint(0, 0));
                WpfPoint bottomRight = ScreenToWorld(new WpfPoint(ActualWidth, ActualHeight));

                double minX = System.Math.Floor(System.Math.Min(topLeft.X, bottomRight.X) / stepX) * stepX;
                double maxX = System.Math.Ceiling(System.Math.Max(topLeft.X, bottomRight.X) / stepX) * stepX;
                double minY = System.Math.Floor(System.Math.Min(topLeft.Y, bottomRight.Y) / stepY) * stepY;
                double maxY = System.Math.Ceiling(System.Math.Max(topLeft.Y, bottomRight.Y) / stepY) * stepY;

                for (double x = minX; x <= maxX; x += stepX)
                    dc.DrawLine(gridPen, new WpfPoint(x, minY), new WpfPoint(x, maxY));
                for (double y = minY; y <= maxY; y += stepY)
                    dc.DrawLine(gridPen, new WpfPoint(minX, y), new WpfPoint(maxX, y));
            }
            this.Opacity = Opacity;

            // Draw all shapes in the list
            if (ShapesToDraw != null)
            {
                foreach (var shape in ShapesToDraw)
                {
                    bool isSelected = selectedShapes.Contains(shape);
                    if (shape is Shape s)
                        s.Draw(dc, ZoomLevel, IsShapeFillEnabled, IsAnchorPointsEnabled, isSelected);
                    // If it's a RectangleShape, draw side lengths
                    if (IsLineSizeEnabled && shape is RectangleShape rectShape && rectShape.AnchorPoints.Count == 4)
                    {
                        var pts = rectShape.AnchorPoints;
                        var center = new WpfPoint(
                            (pts[0].Position.X + pts[2].Position.X) / 2,
                            (pts[0].Position.Y + pts[2].Position.Y) / 2
                        );
                        for (int i = 0; i < 4; i++)
                        {
                            var p1 = pts[i].Position;
                            var p2 = pts[(i + 1) % 4].Position;
                            var mid = new WpfPoint((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                            Vector outward = mid - center;
                            outward.Normalize();
                            double offset = 40; // بدل 30
                            var labelPos = mid + outward * offset;
                            double length = (p1 - p2).Length;
                            string unit = "";
                            if (parentCreateWindow != null)
                            {
                                switch (parentCreateWindow.SelectedUnit)
                                {
                                    case Shaghouri.Create.UnitType.MM:
                                        length = length * 1.0;
                                        unit = "mm";
                                        break;
                                    case Shaghouri.Create.UnitType.CM:
                                        length = length / 10.0;
                                        unit = "cm";
                                        break;
                                    case Shaghouri.Create.UnitType.INCH:
                                        length = length / 25.4;
                                        unit = "inch";
                                        break;
                                }
                            }
                            double minDim = System.Math.Min((p1 - p2).Length, (pts[(i + 2) % 4].Position - pts[(i + 1) % 4].Position).Length);
                            double fontSize = 16 / ZoomLevel;
                            FormattedText text = new FormattedText(
                                $"{length:F1} {unit}",
                                System.Globalization.CultureInfo.InvariantCulture,
                                FlowDirection.LeftToRight,
                                new Typeface("Segoe UI"),
                                fontSize, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF004AFF")), VisualTreeHelper.GetDpi(this).PixelsPerDip);
                            if (System.Math.Abs(p1.Y - p2.Y) < 1) // horizontal side
                            {
                                var textOrigin = new WpfPoint(labelPos.X - text.Width / 2, labelPos.Y - text.Height / 2);
                                dc.DrawText(text, textOrigin);
                            }
                            else // vertical side
                            {
                                var textOrigin = new WpfPoint(labelPos.X - text.Height / 2, labelPos.Y + text.Width / 2);
                                dc.PushTransform(new RotateTransform(-90, labelPos.X, labelPos.Y));
                                dc.DrawText(text, new WpfPoint(labelPos.X - text.Width / 2, labelPos.Y - text.Height / 2));
                                dc.Pop();
                            }
                        }
                    }
                }
            }
            // Draw the current shape (preview) if set
            if (ShapeToDraw != null)
            {
                bool isSelected = (ShapeToDraw == SelectedShape);
                if (ShapeToDraw is Shape s)
                    s.Draw(dc, ZoomLevel, IsShapeFillEnabled, IsAnchorPointsEnabled, isSelected);
                if (IsLineSizeEnabled && ShapeToDraw is RectangleShape rectShape && rectShape.AnchorPoints.Count == 4)
                {
                    var pts = rectShape.AnchorPoints;
                    var center = new WpfPoint(
                        (pts[0].Position.X + pts[2].Position.X) / 2,
                        (pts[0].Position.Y + pts[2].Position.Y) / 2
                    );
                    for (int i = 0; i < 4; i++)
                    {
                        var p1 = pts[i].Position;
                        var p2 = pts[(i + 1) % 4].Position;
                        var mid = new WpfPoint((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                        Vector outward = mid - center;
                        outward.Normalize();
                        double offset = 40; // بدل 30
                        var labelPos = mid + outward * offset;
                        double length = (p1 - p2).Length;
                        string unit = "";
                        if (parentCreateWindow != null)
                        {
                            switch (parentCreateWindow.SelectedUnit)
                            {
                                case Shaghouri.Create.UnitType.MM:
                                    length = length * 1.0;
                                    unit = "mm";
                                    break;
                                case Shaghouri.Create.UnitType.CM:
                                    length = length / 10.0;
                                    unit = "cm";
                                    break;
                                case Shaghouri.Create.UnitType.INCH:
                                    length = length / 25.4;
                                    unit = "inch";
                                    break;
                            }
                        }
                        double minDim = System.Math.Min((p1 - p2).Length, (pts[(i + 2) % 4].Position - pts[(i + 1) % 4].Position).Length);
                        double fontSize = 16 / ZoomLevel;
                        FormattedText text = new FormattedText(
                            $"{length:F1} {unit}",
                            System.Globalization.CultureInfo.InvariantCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"),
                            fontSize, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF004AFF")), VisualTreeHelper.GetDpi(this).PixelsPerDip);
                        if (System.Math.Abs(p1.Y - p2.Y) < 1) // horizontal side
                        {
                            var textOrigin = new WpfPoint(labelPos.X - text.Width / 2, labelPos.Y - text.Height / 2);
                            dc.DrawText(text, textOrigin);
                        }
                        else // vertical side
                        {
                            var textOrigin = new WpfPoint(labelPos.X - text.Height / 2, labelPos.Y + text.Width / 2);
                            dc.PushTransform(new RotateTransform(-90, labelPos.X, labelPos.Y));
                            dc.DrawText(text, new WpfPoint(labelPos.X - text.Width / 2, labelPos.Y - text.Height / 2));
                            dc.Pop();
                        }
                    }
                }
            }
            // Draw marquee rectangle if active
            if (isMarqueeSelecting)
            {
                // Draw marquee in world coordinates (like other shapes)
                // Ensure rectangle is constructed from top-left to bottom-right
                double left = Math.Min(marqueeStartWorld.X, marqueeEndWorld.X);
                double top = Math.Min(marqueeStartWorld.Y, marqueeEndWorld.Y);
                double right = Math.Max(marqueeStartWorld.X, marqueeEndWorld.X);
                double bottom = Math.Max(marqueeStartWorld.Y, marqueeEndWorld.Y);
                var marqueeRect = new Rect(left, top, right - left, bottom - top);
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(60, 0, 120, 215)), new Pen(Brushes.Blue, 1), marqueeRect);
            }
            dc.Pop(); // Pop ScaleTransform
            dc.Pop(); // Pop TranslateTransform
            dc.Pop(); // Pop the clip
        }

        public WpfPoint GetShapesBoundingBoxCenter()
        {
            if (ShapesToDraw == null || ShapesToDraw.Count == 0)
                return new WpfPoint(0, 0);
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var shape in ShapesToDraw)
            {
                foreach (var anchor in shape.AnchorPoints)
                {
                    var p = anchor.Position;
                    if (p.X < minX) minX = p.X;
                    if (p.Y < minY) minY = p.Y;
                    if (p.X > maxX) maxX = p.X;
                    if (p.Y > maxY) maxY = p.Y;
                }
            }
            return new WpfPoint((minX + maxX) / 2, (minY + maxY) / 2);
        }

        public void ZoomToShapesCenter(double newZoomLevel)
        {
            var centerWorld = GetShapesBoundingBoxCenter();
            var centerScreen = new WpfPoint(ActualWidth / 2, ActualHeight / 2);
            panOffsetX = centerScreen.X - centerWorld.X * newZoomLevel;
            panOffsetY = centerScreen.Y - centerWorld.Y * newZoomLevel;
            ZoomLevel = newZoomLevel;
            InvalidateVisual();
        }

        private void CustomGridControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? 1.1 : 0.9;
            double newZoom = ZoomLevel * zoom;
            if (newZoom < 0.1) newZoom = 0.1; // Prevent zooming out too far
            if (newZoom > 20) newZoom = 20;

            // Get mouse position in screen (control) coordinates
            var mouseScreen = e.GetPosition(this);
            // Convert to world coordinates before zoom
            var mouseWorld = ScreenToWorld(mouseScreen);

            // Update zoom
            ZoomLevel = newZoom;

            // After zoom, calculate new panOffset so mouseWorld stays under mouseScreen
            panOffsetX = mouseScreen.X - mouseWorld.X * newZoom;
            panOffsetY = mouseScreen.Y - mouseWorld.Y * newZoom;

            InvalidateVisual();
        }

        private void CustomGridControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsScalingGrainLine) return;
            if (IsDrawingShape) return;
            WpfPoint mouseScreen = e.GetPosition(this);
            WpfPoint mouseWorld = ScreenToWorld(mouseScreen);
            selectedShape = null;
            bool clickedOnShape = false;
            if (ShapesToDraw != null)
            {
                // Check from topmost to bottom
                for (int i = ShapesToDraw.Count - 1; i >= 0; i--)
                {
                    var shape = ShapesToDraw[i];
                    if (shape.GetGeometry().FillContains(mouseWorld))
                    {
                        clickedOnShape = true;
                        // If we have multiple selections and this shape is selected, move all selected shapes
                        if (selectedShapes.Count > 1 && selectedShapes.Contains(shape))
                        {
                            selectedShape = shape; // Use this shape as the reference for dragging
                            shapeDragStartWorld = mouseWorld;
                            // Store original anchor positions for all selected shapes
                            originalAnchorPositions = new List<WpfPoint>();
                            foreach (var selectedShape in selectedShapes)
                            {
                                foreach (var anchor in selectedShape.AnchorPoints)
                                    originalAnchorPositions.Add(anchor.Position);
                            }
                            this.CaptureMouse();
                            this.Cursor = System.Windows.Input.Cursors.Hand;
                            break;
                        }
                        else if (selectedShapes.Count <= 1)
                        {
                            // Single shape selection - normal behavior
                            selectedShape = shape;
                            this.SelectedShape = shape; // تعيين الخاصية العامة أيضًا
                            selectedShapes.Clear();
                            selectedShapes.Add(shape);
                            InvalidateVisual();
                            shapeDragStartWorld = mouseWorld;
                            // Update selection in parent window and refresh previews
                            if (parentCreateWindow != null)
                            {
                                parentCreateWindow.selectedShape = shape;
                                System.Diagnostics.Debug.WriteLine($"[Create] selectedShape set to: {parentCreateWindow.selectedShape}");
                                parentCreateWindow.ShapePreviewsList.Items.Refresh();
                            }
                            // Store original anchor positions
                            originalAnchorPositions = new List<WpfPoint>();
                            foreach (var anchor in shape.AnchorPoints)
                                originalAnchorPositions.Add(anchor.Position);
                            this.CaptureMouse();
                            this.Cursor = System.Windows.Input.Cursors.Hand;
                            break;
                        }
                    }
                }
            }
            if (!clickedOnShape)
            {
                // Deselect all shapes when clicking on empty space
                selectedShapes.Clear();
                this.SelectedShape = null; // إلغاء التحديد أيضًا
                InvalidateVisual();
                if (parentCreateWindow != null)
                {
                    parentCreateWindow.selectedShape = null;
                    parentCreateWindow.ShapePreviewsList.Items.Refresh();
                }
            }
            if (selectedShape == null)
            {
                lastPanPoint = mouseScreen;
                this.CaptureMouse();
                this.Cursor = System.Windows.Input.Cursors.ScrollAll;
            }
        }

        private void CustomGridControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDrawingShape) return;
            selectedShape = null;
            originalAnchorPositions = null;
            lastPanPoint = null;
            this.ReleaseMouseCapture();
            this.Cursor = System.Windows.Input.Cursors.Arrow; // Restore to default
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (IsScalingGrainLine) return;
            base.OnMouseMove(e);
            if (IsDrawingShape) return;
            WpfPoint mouseScreen = e.GetPosition(this);
            WpfPoint mouseWorld = ScreenToWorld(mouseScreen);
            if (isMarqueeSelecting)
            {
                marqueeEndWorld = mouseWorld;
                marqueeEndScreen = mouseScreen;
                InvalidateVisual();
                return;
            }
            if (selectedShape != null && e.LeftButton == MouseButtonState.Pressed && originalAnchorPositions != null)
            {
                Vector delta = mouseWorld - shapeDragStartWorld;
                
                if (selectedShapes.Count > 1 && selectedShapes.Contains(selectedShape))
                {
                    // Move all selected shapes
                    int anchorIndex = 0;
                    foreach (var shape in selectedShapes)
                    {
                        for (int i = 0; i < shape.AnchorPoints.Count; i++)
                        {
                            shape.AnchorPoints[i].Position = originalAnchorPositions[anchorIndex] + delta;
                            anchorIndex++;
                        }
                        // تحديث مستطيل الإحاطة إذا كان RectangleShape
                        if (shape is RectangleShape rectShape && rectShape.AnchorPoints.Count == 4)
                        {
                            var pts = rectShape.AnchorPoints;
                            double left = System.Math.Min(System.Math.Min(pts[0].Position.X, pts[1].Position.X), System.Math.Min(pts[2].Position.X, pts[3].Position.X));
                            double right = System.Math.Max(System.Math.Max(pts[0].Position.X, pts[1].Position.X), System.Math.Max(pts[2].Position.X, pts[3].Position.X));
                            double top = System.Math.Min(System.Math.Min(pts[0].Position.Y, pts[1].Position.Y), System.Math.Min(pts[2].Position.Y, pts[3].Position.Y));
                            double bottom = System.Math.Max(System.Math.Max(pts[0].Position.Y, pts[1].Position.Y), System.Math.Max(pts[2].Position.Y, pts[3].Position.Y));
                            rectShape.Rect = new System.Windows.Rect(left, top, right - left, bottom - top);
                        }
                    }
                }
                else
                {
                    // Move single shape
                    for (int i = 0; i < selectedShape.AnchorPoints.Count; i++)
                    {
                        selectedShape.AnchorPoints[i].Position = originalAnchorPositions[i] + delta;
                    }
                    // تحديث مستطيل الإحاطة إذا كان RectangleShape
                    if (selectedShape is RectangleShape rectShape && rectShape.AnchorPoints.Count == 4)
                    {
                        var pts = rectShape.AnchorPoints;
                        double left = System.Math.Min(System.Math.Min(pts[0].Position.X, pts[1].Position.X), System.Math.Min(pts[2].Position.X, pts[3].Position.X));
                        double right = System.Math.Max(System.Math.Max(pts[0].Position.X, pts[1].Position.X), System.Math.Max(pts[2].Position.X, pts[3].Position.X));
                        double top = System.Math.Min(System.Math.Min(pts[0].Position.Y, pts[1].Position.Y), System.Math.Min(pts[2].Position.Y, pts[3].Position.Y));
                        double bottom = System.Math.Max(System.Math.Max(pts[0].Position.Y, pts[1].Position.Y), System.Math.Max(pts[2].Position.Y, pts[3].Position.Y));
                        rectShape.Rect = new System.Windows.Rect(left, top, right - left, bottom - top);
                    }
                }
                InvalidateVisual();
            }
            else if (lastPanPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                panOffsetX += mouseScreen.X - lastPanPoint.Value.X;
                panOffsetY += mouseScreen.Y - lastPanPoint.Value.Y;
                lastPanPoint = mouseScreen;
                selectedShapes.Clear();
                InvalidateVisual();
            }
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (IsDrawingShape) return;
            WpfPoint mouseWorld = ScreenToWorld(e.GetPosition(this));
            bool clickedOnShape = false;
            if (ShapesToDraw != null)
            {
                // Check if clicked on any shape
                for (int i = ShapesToDraw.Count - 1; i >= 0; i--)
                {
                    var shape = ShapesToDraw[i];
                    if (shape.GetGeometry().FillContains(mouseWorld))
                    {
                        clickedOnShape = true;
                        break;
                    }
                }
            }
            if (!clickedOnShape)
            {
                // Deselect all shapes when clicking on empty space
                selectedShapes.Clear();
                InvalidateVisual();
            }
            isMarqueeSelecting = true;
            marqueeStartWorld = ScreenToWorld(e.GetPosition(this));
            marqueeEndWorld = marqueeStartWorld;
            marqueeStartScreen = e.GetPosition(this);
            marqueeEndScreen = marqueeStartScreen;
            selectedShapes.Clear();
            CaptureMouse();
        }

        protected override void OnMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            if (!isMarqueeSelecting) return;
            isMarqueeSelecting = false;
            marqueeEndWorld = ScreenToWorld(e.GetPosition(this));
            var marqueeRect = new Rect(marqueeStartWorld, marqueeEndWorld);
            selectedShapes.Clear();
            if (ShapesToDraw != null)
            {
                foreach (var shape in ShapesToDraw)
                {
                    if (shape.GetGeometry().Bounds.IntersectsWith(marqueeRect))
                    {
                        selectedShapes.Add(shape);
                    }
                }
            }
            InvalidateVisual();
            ReleaseMouseCapture();
            // بعد إضافة أو حذف من selectedShapes أو بعد التحديد بالمربع
            if (parentCreateWindow != null)
                parentCreateWindow.ShapePreviewsList.Items.Refresh();
        }

        private Point WorldToScreen(Point world)
        {
            return new Point(world.X * ZoomLevel + panOffsetX, world.Y * ZoomLevel + panOffsetY);
        }

        public List<Shape> GetSelectedShapes()
        {
            return selectedShapes;
        }

        public void ClearAndSelectShape(Shape shape)
        {
            selectedShapes.Clear();
            if (shape != null)
                selectedShapes.Add(shape);
            this.SelectedShape = shape;
            InvalidateVisual();
            // بعد إضافة أو حذف من selectedShapes أو بعد التحديد بالمربع
            if (parentCreateWindow != null)
                parentCreateWindow.ShapePreviewsList.Items.Refresh();
        }
    }
} 