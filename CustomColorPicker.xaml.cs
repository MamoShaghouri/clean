using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;

namespace Shaghouri
{
    public partial class CustomColorPicker : UserControl
    {
        #region Events
        public event EventHandler<Color> ColorSelected;
        public event EventHandler Canceled;
        #endregion

        #region Private Fields
        private double _hue = 0;
        private double _saturation = 1;
        private double _value = 1;
        private bool _isDraggingColorWheel = false;
        private bool _isDraggingSaturationBrightness = false;
        private Point _lastMousePosition;
        private List<Color> _favoriteColors = new List<Color>();
        private const int COLOR_WHEEL_SIZE = 200;
        private const int SATURATION_BRIGHTNESS_SIZE = 200;
        #endregion

        #region Constructor
        public CustomColorPicker()
        {
            InitializeComponent();
            InitializeColorPicker();
            LoadFavoriteColors();
        }
        #endregion

        #region Initialization
        private void InitializeColorPicker()
        {
            // Set initial color (red)
            UpdateColor(255, 0, 0);
            
            // Render color wheel
            RenderColorWheel();
            
            // Render saturation/brightness picker
            RenderSaturationBrightnessPicker();
            
            // Set up event handlers
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            // Hue slider
            HueSlider.ValueChanged += (s, e) =>
            {
                _hue = e.NewValue;
                UpdateColorFromHSV();
                RenderSaturationBrightnessPicker();
            };
        }
        #endregion

        #region Color Wheel Rendering
        private void RenderColorWheel()
        {
            ColorWheelCanvas.Children.Clear();
            
            // استخدام صورة ثابتة للأداء الأفضل
            var center = COLOR_WHEEL_SIZE / 2;
            var radius = center - 10;
            
            // إنشاء صورة عجلة الألوان
            var writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                COLOR_WHEEL_SIZE, COLOR_WHEEL_SIZE, 96, 96, 
                System.Windows.Media.PixelFormats.Bgr32, null);
            
            writeableBitmap.Lock();
            
            try
            {
                var pixels = new byte[COLOR_WHEEL_SIZE * COLOR_WHEEL_SIZE * 4];
                
                for (int y = 0; y < COLOR_WHEEL_SIZE; y++)
                {
                    for (int x = 0; x < COLOR_WHEEL_SIZE; x++)
                    {
                        var dx = x - center;
                        var dy = y - center;
                        var distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= radius)
                        {
                            var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
                            if (angle < 0) angle += 360;

                            var hue = angle;
                            var saturation = distance / radius;
                            var value = 1.0;

                            var color = HsvToRgb(hue, saturation, value);
                            
                            var index = (y * COLOR_WHEEL_SIZE + x) * 4;
                            pixels[index] = color.B;     // Blue
                            pixels[index + 1] = color.G; // Green
                            pixels[index + 2] = color.R; // Red
                            pixels[index + 3] = 255;     // Alpha
                        }
                    }
                }
                
                writeableBitmap.WritePixels(
                    new System.Windows.Int32Rect(0, 0, COLOR_WHEEL_SIZE, COLOR_WHEEL_SIZE),
                    pixels, COLOR_WHEEL_SIZE * 4, 0);
            }
            finally
            {
                writeableBitmap.Unlock();
            }
            
            var imageControl = new Image
            {
                Source = writeableBitmap,
                Stretch = Stretch.None
            };
            
            ColorWheelCanvas.Children.Add(imageControl);
            
            // Add selection indicator
            AddColorWheelIndicator();
        }

        private void AddColorWheelIndicator()
        {
            var indicator = new Ellipse
            {
                Width = 12,
                Height = 12,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Fill = Brushes.Black
            };

            var center = COLOR_WHEEL_SIZE / 2;
            var radius = center - 10;
            var angle = _hue * Math.PI / 180;
            var x = center + radius * _saturation * Math.Cos(angle);
            var y = center + radius * _saturation * Math.Sin(angle);

            Canvas.SetLeft(indicator, x - 6);
            Canvas.SetTop(indicator, y - 6);

            ColorWheelCanvas.Children.Add(indicator);
        }
        #endregion

        #region Saturation/Brightness Picker
        private void RenderSaturationBrightnessPicker()
        {
            SaturationBrightnessCanvas.Children.Clear();

            // استخدام صورة ثابتة للأداء الأفضل
            var writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                SATURATION_BRIGHTNESS_SIZE, SATURATION_BRIGHTNESS_SIZE, 96, 96, 
                System.Windows.Media.PixelFormats.Bgr32, null);
            
            writeableBitmap.Lock();
            
            try
            {
                var pixels = new byte[SATURATION_BRIGHTNESS_SIZE * SATURATION_BRIGHTNESS_SIZE * 4];
                
                for (int y = 0; y < SATURATION_BRIGHTNESS_SIZE; y++)
                {
                    for (int x = 0; x < SATURATION_BRIGHTNESS_SIZE; x++)
                    {
                        var saturation = (double)x / SATURATION_BRIGHTNESS_SIZE;
                        var value = 1.0 - (double)y / SATURATION_BRIGHTNESS_SIZE;

                        var color = HsvToRgb(_hue, saturation, value);
                        
                        var index = (y * SATURATION_BRIGHTNESS_SIZE + x) * 4;
                        pixels[index] = color.B;     // Blue
                        pixels[index + 1] = color.G; // Green
                        pixels[index + 2] = color.R; // Red
                        pixels[index + 3] = 255;     // Alpha
                    }
                }
                
                writeableBitmap.WritePixels(
                    new System.Windows.Int32Rect(0, 0, SATURATION_BRIGHTNESS_SIZE, SATURATION_BRIGHTNESS_SIZE),
                    pixels, SATURATION_BRIGHTNESS_SIZE * 4, 0);
            }
            finally
            {
                writeableBitmap.Unlock();
            }
            
            var imageControl = new Image
            {
                Source = writeableBitmap,
                Stretch = Stretch.None
            };
            
            SaturationBrightnessCanvas.Children.Add(imageControl);
            
            // Add selection indicator
            AddSaturationBrightnessIndicator();
        }

        private void AddSaturationBrightnessIndicator()
        {
            var indicator = new Ellipse
            {
                Width = 12,
                Height = 12,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Fill = Brushes.Black
            };

            var x = _saturation * SATURATION_BRIGHTNESS_SIZE;
            var y = (1.0 - _value) * SATURATION_BRIGHTNESS_SIZE;

            Canvas.SetLeft(indicator, x - 6);
            Canvas.SetTop(indicator, y - 6);

            SaturationBrightnessCanvas.Children.Add(indicator);
        }
        #endregion

        #region Color Conversion
        private Color HsvToRgb(double h, double s, double v)
        {
            var c = v * s;
            var x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            var m = v - c;

            double r, g, b;

            if (h >= 0 && h < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (h >= 60 && h < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (h >= 120 && h < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (h >= 180 && h < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (h >= 240 && h < 300)
            {
                r = x; g = 0; b = c;
            }
            else
            {
                r = c; g = 0; b = x;
            }

            return Color.FromRgb(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }

        private void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
        {
            var red = r / 255.0;
            var green = g / 255.0;
            var blue = b / 255.0;

            var max = Math.Max(Math.Max(red, green), blue);
            var min = Math.Min(Math.Min(red, green), blue);
            var delta = max - min;

            v = max;
            s = max == 0 ? 0 : delta / max;

            if (delta == 0)
            {
                h = 0;
            }
            else if (max == red)
            {
                h = 60 * (((green - blue) / delta) % 6);
            }
            else if (max == green)
            {
                h = 60 * ((blue - red) / delta + 2);
            }
            else
            {
                h = 60 * ((red - green) / delta + 4);
            }

            if (h < 0) h += 360;
        }
        #endregion

        #region Event Handlers
        private void ColorWheelCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingColorWheel = true;
            _lastMousePosition = e.GetPosition(ColorWheelCanvas);
            UpdateColorFromColorWheel(_lastMousePosition);
        }

        private void ColorWheelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingColorWheel)
            {
                var position = e.GetPosition(ColorWheelCanvas);
                UpdateColorFromColorWheel(position);
                _lastMousePosition = position;
            }
        }

        private void ColorWheelCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingColorWheel = false;
        }

        private void SaturationBrightnessCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSaturationBrightness = true;
            _lastMousePosition = e.GetPosition(SaturationBrightnessCanvas);
            UpdateColorFromSaturationBrightness(_lastMousePosition);
        }

        private void SaturationBrightnessCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingSaturationBrightness)
            {
                var position = e.GetPosition(SaturationBrightnessCanvas);
                UpdateColorFromSaturationBrightness(position);
                _lastMousePosition = position;
            }
        }

        private void SaturationBrightnessCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingSaturationBrightness = false;
        }

        private void UpdateColorFromColorWheel(Point position)
        {
            var center = COLOR_WHEEL_SIZE / 2;
            var dx = position.X - center;
            var dy = position.Y - center;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            var radius = center - 10;

            if (distance <= radius)
            {
                var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
                if (angle < 0) angle += 360;

                _hue = angle;
                _saturation = Math.Min(1.0, distance / radius);

                UpdateColorFromHSV();
                RenderSaturationBrightnessPicker();
                AddColorWheelIndicator();
            }
        }

        private void UpdateColorFromSaturationBrightness(Point position)
        {
            _saturation = Math.Max(0, Math.Min(1, position.X / SATURATION_BRIGHTNESS_SIZE));
            _value = Math.Max(0, Math.Min(1, 1.0 - position.Y / SATURATION_BRIGHTNESS_SIZE));

            UpdateColorFromHSV();
            AddSaturationBrightnessIndicator();
        }

        private void UpdateColorFromHSV()
        {
            var color = HsvToRgb(_hue, _saturation, _value);
            UpdateColor(color.R, color.G, color.B);
        }

        private void UpdateColor(byte r, byte g, byte b)
        {
            // حماية من Null Reference
            if (ColorPreview == null || RedTextBox == null || GreenTextBox == null || 
                BlueTextBox == null || HexTextBox == null || HueSlider == null)
                return;
                
            var color = Color.FromRgb(r, g, b);
            
            // Update preview
            ColorPreview.Fill = new SolidColorBrush(color);
            
            // Update RGB textboxes
            RedTextBox.Text = r.ToString();
            GreenTextBox.Text = g.ToString();
            BlueTextBox.Text = b.ToString();
            
            // Update HEX textbox
            HexTextBox.Text = $"#{r:X2}{g:X2}{b:X2}";
            
            // Update HSV values
            RgbToHsv(r, g, b, out _hue, out _saturation, out _value);
            HueSlider.Value = _hue;
        }

        private void RGB_TextChanged(object sender, TextChangedEventArgs e)
        {
            // حماية من Null Reference
            if (RedTextBox?.Text == null || GreenTextBox?.Text == null || BlueTextBox?.Text == null)
                return;
                
            if (byte.TryParse(RedTextBox.Text, out byte r) &&
                byte.TryParse(GreenTextBox.Text, out byte g) &&
                byte.TryParse(BlueTextBox.Text, out byte b))
            {
                UpdateColor(r, g, b);
                RenderSaturationBrightnessPicker();
                AddColorWheelIndicator();
                AddSaturationBrightnessIndicator();
            }
        }

        private void Hex_TextChanged(object sender, TextChangedEventArgs e)
        {
            // حماية من Null Reference
            if (HexTextBox?.Text == null)
                return;
                
            var hex = HexTextBox.Text.TrimStart('#');
            if (hex.Length == 6 && 
                byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte r) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out byte g) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
            {
                UpdateColor(r, g, b);
                RenderSaturationBrightnessPicker();
                AddColorWheelIndicator();
                AddSaturationBrightnessIndicator();
            }
        }

        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // حماية من Null Reference
            if (HueSlider == null)
                return;
                
            _hue = e.NewValue;
            UpdateColorFromHSV();
            RenderSaturationBrightnessPicker();
            AddColorWheelIndicator();
        }
        #endregion

        #region Favorite Colors
        private void LoadFavoriteColors()
        {
            // Add some default favorite colors
            var defaultColors = new List<Color>
            {
                Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow,
                Colors.Orange, Colors.Purple, Colors.Pink, Colors.Brown,
                Colors.Black, Colors.White, Colors.Gray, Colors.Cyan
            };

            foreach (var color in defaultColors)
            {
                AddFavoriteColorButton(color);
            }
        }

        private void AddFavoriteColorButton(Color color)
        {
            var button = new Button
            {
                Width = 30,
                Height = 30,
                Margin = new Thickness(2, 2, 2, 2),
                Background = new SolidColorBrush(color),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            button.Click += (s, e) => SelectFavoriteColor(color);
            FavoriteColorsPanel.Children.Add(button);
        }

        private void SelectFavoriteColor(Color color)
        {
            RgbToHsv(color.R, color.G, color.B, out _hue, out _saturation, out _value);
            UpdateColor(color.R, color.G, color.B);
            
            HueSlider.Value = _hue;
            RenderSaturationBrightnessPicker();
            AddColorWheelIndicator();
            AddSaturationBrightnessIndicator();
        }

        private void AddToFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            var currentColor = ((SolidColorBrush)ColorPreview.Fill).Color;
            
            if (!_favoriteColors.Contains(currentColor))
            {
                _favoriteColors.Add(currentColor);
                AddFavoriteColorButton(currentColor);
            }
        }
        #endregion

        #region Action Buttons
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedColor = ((SolidColorBrush)ColorPreview.Fill).Color;
            ColorSelected?.Invoke(this, selectedColor);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Canceled?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Public Methods
        public void SetInitialColor(Color color)
        {
            RgbToHsv(color.R, color.G, color.B, out _hue, out _saturation, out _value);
            UpdateColor(color.R, color.G, color.B);
            
            HueSlider.Value = _hue;
            RenderColorWheel();
            RenderSaturationBrightnessPicker();
        }

        public Color GetSelectedColor()
        {
            return ((SolidColorBrush)ColorPreview.Fill).Color;
        }
        #endregion
    }
} 