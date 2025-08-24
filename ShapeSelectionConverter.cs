using System;
using System.Globalization;
using System.Windows.Data;

namespace Shaghouri
{
    public class ShapeSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value: DataContext (Create window), parameter: the shape
            var create = value as Shaghouri.Create;
            var shape = parameter as Shape;
            return create != null && shape != null && create.selectedShape == shape;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
} 