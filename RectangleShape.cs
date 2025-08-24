using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WpfPoint = System.Windows.Point;

namespace Shaghouri
{
    // Rectangle shape with 4 anchor points (corners)
    public class RectangleShape : Shape
    {
        public Rect Rect { get; set; }
        public string Name { get; set; } = "Unnamed";
        public RectangleShape(System.Windows.Rect rect)
        {
            Rect = rect;
            AnchorPoints = new List<AnchorPoint>
            {
                new AnchorPoint(new WpfPoint(rect.Left, rect.Top)),
                new AnchorPoint(new WpfPoint(rect.Right, rect.Top)),
                new AnchorPoint(new WpfPoint(rect.Right, rect.Bottom)),
                new AnchorPoint(new WpfPoint(rect.Left, rect.Bottom))
            };
            ArrowLength = rect.Width * 0.3; // طول السهم = 30% من عرض المستطيل
        }
        public override PathGeometry GetGeometry()
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = AnchorPoints[0].Position, IsClosed = true };
            for (int i = 1; i < AnchorPoints.Count; i++)
                figure.Segments.Add(new LineSegment(AnchorPoints[i].Position, true));
            geometry.Figures.Add(figure);
            return geometry;
        }
    }
} 