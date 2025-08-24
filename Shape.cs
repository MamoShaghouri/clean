using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using WpfPoint = System.Windows.Point;

namespace Shaghouri
{
    // Abstract base class for shapes
    public abstract class Shape
    {
        public List<AnchorPoint> AnchorPoints { get; set; } = new List<AnchorPoint>();
        public abstract PathGeometry GetGeometry();
        public double ArrowAngle { get; set; } = 0; // زاوية السهم (0 افتراضي)
        public double ArrowLength { get; set; } = 50; // الطول الافتراضي للسهم
        public double? ArrowLengthPreview { get; set; } = null; // معاينة الطول الجديد (أزرق)
        public virtual void Draw(DrawingContext dc, double zoomFactor, bool fillShape, bool showAnchorPoints, bool isSelected)
        {
            // Choose fill color
            SolidColorBrush fillBrush = null;
            if (fillShape)
            {
                if (isSelected)
                {
                    // Baby blue with 30% opacity
                    fillBrush = new SolidColorBrush(Color.FromArgb((byte)(0.3 * 255), 137, 207, 240)); // Baby blue
                }
                else
                {
                    // Wooden color with 30% opacity
                    fillBrush = new SolidColorBrush(Color.FromArgb((byte)(0.3 * 255), 156, 102, 31)); // SaddleBrown-like
                }
                fillBrush.Freeze();
            }
            double penThickness = 2.0 / zoomFactor;
            Pen borderPen;
            if (isSelected)
                borderPen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAB00FF")), penThickness);
            else
                borderPen = new Pen(Brushes.Black, penThickness);
            borderPen.Freeze();
            dc.DrawGeometry(fillBrush, borderPen, GetGeometry());
            // Draw anchor points with fixed 10 pixel diameter (5 pixel radius) regardless of zoom
            if (showAnchorPoints)
            {
                foreach (var anchor in AnchorPoints)
                {
                    double fixedRadius = 5.0 / zoomFactor;
                    var anchorGeometry = new EllipseGeometry(anchor.Position, fixedRadius, fixedRadius);
                    dc.DrawGeometry(anchor.Fill, null, anchorGeometry);
                }
            }
            // إذا كان الشكل مستطيل وله اسم، اعرض الاسم أسفله
            if (this is RectangleShape rectShape && !string.IsNullOrWhiteSpace(rectShape.Name))
            {
                // حساب مركز المستطيل
                var rect = rectShape.Rect;
                var center = new WpfPoint(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
                // حجم الخط ثابت على الشاشة بغض النظر عن الزوم
                double fontSize = 16 / zoomFactor;
                // الهامش ثابت على الشاشة
                double arrowPadding = 10 / zoomFactor;
                var text = new FormattedText(
                    rectShape.Name,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    fontSize, Brushes.Black, VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);
                // رسم الاسم فوق السهم مع بعض المسافة
                var textOrigin = new WpfPoint(center.X - text.Width / 2, center.Y - arrowPadding - text.Height / 2);
                dc.DrawText(text, textOrigin);
            }

            // رسم السهم الدال على اتجاه القماش (من اليسار لليمين)
            if (AnchorPoints.Count >= 2)
            {
                // Helper function to draw the arrow with given length and color
                void DrawArrow(double length, Brush color)
                {
                    Pen arrowPen = new Pen(color, 3.0 / zoomFactor) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
                    arrowPen.Freeze();
                    System.Windows.Rect bounds;
                    if (this is RectangleShape rectShape2)
                        bounds = rectShape2.Rect;
                    else
                    {
                        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
                        foreach (var pt in AnchorPoints)
                        {
                            minX = System.Math.Min(minX, pt.Position.X);
                            minY = System.Math.Min(minY, pt.Position.Y);
                            maxX = System.Math.Max(maxX, pt.Position.X);
                            maxY = System.Math.Max(maxY, pt.Position.Y);
                        }
                        bounds = new System.Windows.Rect(new WpfPoint(minX, minY), new WpfPoint(maxX, maxY));
                    }
                    double arrowLength = length;
                    double arrowHeight = bounds.Height * 0.08;
                    if (arrowLength < 10) arrowLength = 10;
                    if (arrowHeight < 3) arrowHeight = 3;
                    var center = new WpfPoint(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
                    var arrowStart = new WpfPoint(center.X - arrowLength / 2, center.Y);
                    var arrowEnd = new WpfPoint(center.X + arrowLength / 2, center.Y);
                    var arrowCenter = center;
                    dc.PushTransform(new RotateTransform(ArrowAngle, arrowCenter.X, arrowCenter.Y));
                    dc.DrawLine(arrowPen, arrowStart, arrowEnd);
                    double headLength = 12.0 / zoomFactor; // ثابت
                    double angle = 30 * System.Math.PI / 180.0;
                    var dir = new System.Windows.Vector(arrowEnd.X - arrowStart.X, arrowEnd.Y - arrowStart.Y);
                    dir.Normalize();
                    for (int sign = -1; sign <= 1; sign += 2)
                    {
                        double theta = sign * angle;
                        var headDir = new System.Windows.Vector(
                            dir.X * System.Math.Cos(theta) - dir.Y * System.Math.Sin(theta),
                            dir.X * System.Math.Sin(theta) + dir.Y * System.Math.Cos(theta)
                        );
                        var headPoint = new WpfPoint(
                            arrowEnd.X - headDir.X * headLength,
                            arrowEnd.Y - headDir.Y * headLength
                        );
                        dc.DrawLine(arrowPen, arrowEnd, headPoint);
                    }
                    double tailAngle = -45 * System.Math.PI / 180.0;
                    double tailLength = 12.0 / zoomFactor; // ثابت
                    var tailDir = new System.Windows.Vector(
                        dir.X * System.Math.Cos(tailAngle) - dir.Y * System.Math.Sin(tailAngle),
                        dir.X * System.Math.Sin(tailAngle) + dir.Y * System.Math.Cos(tailAngle)
                    );
                    var tailPoint = new WpfPoint(
                        arrowStart.X - tailDir.X * tailLength,
                        arrowStart.Y - tailDir.Y * tailLength
                    );
                    dc.DrawLine(arrowPen, arrowStart, tailPoint);
                    dc.Pop();
                }
                // Draw original arrow
                DrawArrow(ArrowLength, AnchorPoints[0].Fill ?? Brushes.Red);
                // Draw preview arrow if set
                if (ArrowLengthPreview.HasValue)
                    DrawArrow(ArrowLengthPreview.Value, Brushes.Blue);
            }
        }
    }
} 