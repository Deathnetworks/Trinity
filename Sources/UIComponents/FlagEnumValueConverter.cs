using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace GilesTrinity.UIComponents
{
    public class FlagsEnumValueConverter : IValueConverter
    {
        private int targetValue;

        public FlagsEnumValueConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int mask = (int)Enum.Parse(value.GetType(), (string)parameter);
            this.targetValue = (int)value;
            return ((mask & this.targetValue) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.targetValue ^= (int)Enum.Parse(targetType, (string)parameter);
            return Enum.Parse(targetType, this.targetValue.ToString());
        }
    }

}
