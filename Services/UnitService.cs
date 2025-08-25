using System;
using System.Windows;
using System.Windows.Media;

namespace Shaghouri.Services
{
    /// <summary>
    /// خدمة مركزية لإدارة الوحدات مع دعم DPI
    /// </summary>
    public static class UnitService
    {
        #region Constants
        private const double MM_PER_PIXEL = 1.0;
        private const double CM_PER_PIXEL = 0.1; // 10 بكسل = 1 سم
        private const double INCH_PER_PIXEL = 0.0393700787; // 25.4 بكسل = 1 بوصة
        #endregion

        #region Properties
        /// <summary>
        /// الوحدة المحددة حالياً
        /// </summary>
        public static UnitType CurrentUnit { get; set; } = UnitType.CM;

        /// <summary>
        /// معامل DPI الحالي
        /// </summary>
        public static double CurrentDpiScale { get; private set; } = 1.0;
        #endregion

        #region Unit Types
        public enum UnitType
        {
            MM,     // ملليمتر
            CM,     // سنتيمتر
            INCH    // بوصة
        }
        #endregion

        #region DPI Management
        /// <summary>
        /// تحديث معامل DPI
        /// </summary>
        public static void UpdateDpiScale(Visual visual)
        {
            try
            {
                var dpi = VisualTreeHelper.GetDpi(visual);
                CurrentDpiScale = dpi.DpiScaleX; // نستخدم X أو Y (عادة متساويان)
            }
            catch
            {
                CurrentDpiScale = 1.0; // قيمة افتراضية في حالة الخطأ
            }
        }

        /// <summary>
        /// الحصول على معامل DPI من نافذة
        /// </summary>
        public static void UpdateDpiScaleFromWindow(Window window)
        {
            try
            {
                var dpi = VisualTreeHelper.GetDpi(window);
                CurrentDpiScale = dpi.DpiScaleX;
            }
            catch
            {
                CurrentDpiScale = 1.0;
            }
        }
        #endregion

        #region Conversion Methods
        /// <summary>
        /// تحويل من البكسل إلى الوحدة المحددة
        /// </summary>
        public static double ConvertFromPixels(double pixels, UnitType? unit = null)
        {
            var targetUnit = unit ?? CurrentUnit;
            var adjustedPixels = pixels / CurrentDpiScale;

            return targetUnit switch
            {
                UnitType.MM => adjustedPixels * MM_PER_PIXEL,
                UnitType.CM => adjustedPixels * CM_PER_PIXEL,
                UnitType.INCH => adjustedPixels * INCH_PER_PIXEL,
                _ => adjustedPixels
            };
        }

        /// <summary>
        /// تحويل من الوحدة المحددة إلى البكسل
        /// </summary>
        public static double ConvertToPixels(double value, UnitType? unit = null)
        {
            var sourceUnit = unit ?? CurrentUnit;
            var pixels = sourceUnit switch
            {
                UnitType.MM => value / MM_PER_PIXEL,
                UnitType.CM => value / CM_PER_PIXEL,
                UnitType.INCH => value / INCH_PER_PIXEL,
                _ => value
            };

            return pixels * CurrentDpiScale;
        }

        /// <summary>
        /// تحويل من وحدة إلى وحدة أخرى
        /// </summary>
        public static double ConvertBetweenUnits(double value, UnitType fromUnit, UnitType toUnit)
        {
            if (fromUnit == toUnit) return value;

            // تحويل إلى البكسل أولاً
            var pixels = ConvertToPixels(value, fromUnit);
            // ثم تحويل إلى الوحدة المطلوبة
            return ConvertFromPixels(pixels, toUnit);
        }
        #endregion

        #region Grid Spacing Methods
        /// <summary>
        /// تحويل مسافات الشبكة من الوحدة إلى البكسل
        /// </summary>
        public static double ConvertGridSpacingToPixels(double spacing, UnitType? unit = null)
        {
            return ConvertToPixels(spacing, unit);
        }

        /// <summary>
        /// تحويل مسافات الشبكة من البكسل إلى الوحدة
        /// </summary>
        public static double ConvertGridSpacingFromPixels(double pixels, UnitType? unit = null)
        {
            return ConvertFromPixels(pixels, unit);
        }
        #endregion

        #region Formatting Methods
        /// <summary>
        /// تنسيق القيمة مع الوحدة
        /// </summary>
        public static string FormatValue(double value, UnitType? unit = null, int decimalPlaces = 2)
        {
            var targetUnit = unit ?? CurrentUnit;
            var unitSymbol = GetUnitSymbol(targetUnit);
            return $"{value.ToString($"F{decimalPlaces}")} {unitSymbol}";
        }

        /// <summary>
        /// الحصول على رمز الوحدة
        /// </summary>
        public static string GetUnitSymbol(UnitType unit)
        {
            return unit switch
            {
                UnitType.MM => "mm",
                UnitType.CM => "cm",
                UnitType.INCH => "inch",
                _ => ""
            };
        }

        /// <summary>
        /// الحصول على اسم الوحدة بالعربية
        /// </summary>
        public static string GetUnitName(UnitType unit)
        {
            return unit switch
            {
                UnitType.MM => "ملليمتر",
                UnitType.CM => "سنتيمتر",
                UnitType.INCH => "بوصة",
                _ => ""
            };
        }
        #endregion

        #region Validation Methods
        /// <summary>
        /// التحقق من صحة القيمة للوحدة
        /// </summary>
        public static bool IsValidValue(double value, UnitType unit)
        {
            return unit switch
            {
                UnitType.MM => value >= 0 && value <= 10000, // 0 إلى 10 متر
                UnitType.CM => value >= 0 && value <= 1000,  // 0 إلى 10 متر
                UnitType.INCH => value >= 0 && value <= 400, // 0 إلى 10 متر تقريباً
                _ => false
            };
        }

        /// <summary>
        /// تقييد القيمة ضمن الحدود المسموحة
        /// </summary>
        public static double ClampValue(double value, UnitType unit)
        {
            return unit switch
            {
                UnitType.MM => Math.Max(0, Math.Min(10000, value)),
                UnitType.CM => Math.Max(0, Math.Min(1000, value)),
                UnitType.INCH => Math.Max(0, Math.Min(400, value)),
                _ => value
            };
        }
        #endregion
    }
} 