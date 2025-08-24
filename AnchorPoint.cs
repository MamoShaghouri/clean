using System.Windows;
using System.Windows.Media;
using WpfPoint = System.Windows.Point;

namespace Shaghouri
{
    // Represents an anchor point (red dot)
    public class AnchorPoint
    {
        public WpfPoint Position { get; set; }
        public double Radius { get; set; } = 5;
        public Brush Fill { get; set; } = Brushes.Red;

        public AnchorPoint(WpfPoint position)
        {
            Position = position;
        }

        public EllipseGeometry GetGeometry()
        {
            return new EllipseGeometry(Position, Radius, Radius);
        }
    }
} 