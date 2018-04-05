using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace DICOMUI.Converters
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.IsAssignableFrom(typeof(Boolean)) && targetType.IsAssignableFrom(typeof(String)))
                throw new ArgumentException("EnumConverter can only convert to boolean or string.");
            if (targetType == typeof(String))
                return value.ToString();

            return String.Compare(value.ToString(), (String)parameter, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType.IsAssignableFrom(typeof(Boolean)) && targetType.IsAssignableFrom(typeof(String)))
                throw new ArgumentException("EnumConverter can only convert back value from a string or a boolean.");
            if (!targetType.IsEnum)
                throw new ArgumentException("EnumConverter can only convert value to an Enum Type.");

            if (value.GetType() == typeof(String))
            {
                return Enum.Parse(targetType, (String)value, true);
            }

            //We have a boolean, as for binding to a checkbox. we use parameter
            if ((Boolean)value)
                return Enum.Parse(targetType, (String)parameter, true);

            return Binding.DoNothing;
        }
    }

}
