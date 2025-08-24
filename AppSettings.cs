using System;
using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace Shaghouri
{
    public class AppSettings
    {
        public double GridRectWidth { get; set; } = 10;
        public double GridRectHeight { get; set; } = 10;
        public string GridLinesColor { get; set; } = "#FFD3D3D3";
        public double GridOpacity { get; set; } = 1.0;
        public bool ShowGrid { get; set; } = true;
        public double ZoomLevel { get; set; } = 1.0;
        public string LeftCanvasBackgroundColor { get; set; } = "#FFFFFFFF"; // Default: White
        public double LeftCanvasBackgroundOpacity { get; set; } = 1.0; // Default: fully opaque
        public bool LeftCanvasIsGradient { get; set; } = false;
        public string LeftCanvasGradientColor { get; set; } = "#FF41D8CE"; // Default gradient color

        public static string SettingsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static AppSettings Load()
        {
            if (!File.Exists(SettingsFilePath))
            {
                var defaultSettings = new AppSettings();
                defaultSettings.Save();
                return defaultSettings;
            }
            try
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                // If file is corrupted, reset to default
                var defaultSettings = new AppSettings();
                defaultSettings.Save();
                return defaultSettings;
            }
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }

        public static Color ParseColor(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
        public static string ColorToHex(Color color)
        {
            return color.ToString();
        }
    }
} 