using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Diagnostics;

namespace Shaghouri
{
    public class ShapePreviewControl : Canvas
    {
        public static readonly DependencyProperty ShapeToPreviewProperty =
            DependencyProperty.Register("ShapeToPreview", typeof(Shape), typeof(ShapePreviewControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ShapePreviewControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public Shape ShapeToPreview
        {
            get => (Shape)GetValue(ShapeToPreviewProperty);
            set => SetValue(ShapeToPreviewProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public ShapePreviewControl()
        {
            Width = 120;
            Height = 120;
            Background = Brushes.Transparent;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (ShapeToPreview == null) return;
            var bounds = ShapeToPreview.GetGeometry().Bounds;
            if (bounds.Width < 1 || bounds.Height < 1) return;
            double canvasW = ActualWidth;
            double canvasH = ActualHeight;
            double scale = 0.8 * System.Math.Min(canvasW / bounds.Width, canvasH / bounds.Height);
            double offsetX = (canvasW - bounds.Width * scale) / 2 - bounds.Left * scale;
            double offsetY = (canvasH - bounds.Height * scale) / 2 - bounds.Top * scale;

            // Draw drop shadow
            DropShadowEffect shadow = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 8,
                ShadowDepth = 3,
                Opacity = 0.25
            };
            this.Effect = shadow;

            dc.PushTransform(new TranslateTransform(offsetX, offsetY));
            dc.PushTransform(new ScaleTransform(scale, scale));
            var geometry = ShapeToPreview.GetGeometry();
            Brush fillBrush;
            if (IsSelected)
            {
                fillBrush = new SolidColorBrush(Color.FromRgb(255, 182, 193)); // LightPink
                Debug.WriteLine($"[ShapePreviewControl] IsSelected=True, using LightPink for {ShapeToPreview}");
            }
            else
            {
                fillBrush = new SolidColorBrush(Color.FromRgb(137, 207, 240)); // Baby blue
                Debug.WriteLine($"[ShapePreviewControl] IsSelected=False, using BabyBlue for {ShapeToPreview}");
            }
            fillBrush.Freeze();
            var blackPen = new Pen(Brushes.Black, 1.0 / scale);
            blackPen.Freeze();
            dc.DrawGeometry(fillBrush, blackPen, geometry);

            // Draw the shape name centered below the shape
            if (ShapeToPreview is RectangleShape rectShape && !string.IsNullOrWhiteSpace(rectShape.Name))
            {
                string name = rectShape.Name;
                double fontSize = 14.0 / scale;
                var formattedText = new FormattedText(
                    name,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                    fontSize,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                double textX = bounds.Left + (bounds.Width - formattedText.Width) / 2;
                double textY = bounds.Bottom + 15 / scale; // <-- بدل 5 إلى 15
                dc.DrawText(formattedText, new Point(textX, textY));
            }
            dc.Pop();
            dc.Pop();

            // Draw selection border if selected
            if (IsSelected)
            {
                var borderPen = new Pen(Brushes.DeepSkyBlue, 3.0);
                borderPen.Freeze();
                dc.DrawRectangle(null, borderPen, new Rect(1.5, 1.5, ActualWidth - 3, ActualHeight - 3));
            }
        }
    }
} 