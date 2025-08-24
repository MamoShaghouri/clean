using System;
using System.Globalization;
using System.Windows.Data;
using System.Diagnostics;

namespace Shaghouri
{
    public class ShapeSelectionMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var create = values[0] as Shaghouri.Create;
            var shape = values[1] as Shape;
            var selectedShapes = create?.SelectedShapes;
            bool isSelected = selectedShapes != null && shape != null && selectedShapes.Contains(shape);
            Debug.WriteLine($"[ShapeSelectionMultiConverter] selectedShape: {create?.selectedShape}, shape: {shape}, equal: {isSelected}");
            return isSelected;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
} 