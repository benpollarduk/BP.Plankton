using System;
using System.Globalization;
using System.Windows.Data;
using BP.Plankton.Classes;
using Plankton.Classes;

namespace Plankton.Converters
{
    /// <summary>
    /// Converts a Current to a Double representing an opacity. The Current.GetCurrentStrengthOfTotalStrength() method is used to obtain an opacity value between 0.25 and 1.0 where stronger values are more opaque.
    /// </summary>
    [ValueConversion(typeof (Current), typeof (double))]
    public class CurrentToOpacityConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Current))
                return 0.1d;

            var c = (Current)value;
            var convertedValue = (1d / 100d) * c.GetCurrentStrengthOfTotalStrength();
            return Math.Min(1.0d, Math.Max(convertedValue, 0.25d));
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}