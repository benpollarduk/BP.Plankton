using System;
using System.Globalization;
using System.Windows.Data;

namespace BP.Plankton.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    internal class DoubleToAnimationBeginTimeConverterConverter : IValueConverter
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
            if (!double.TryParse(value?.ToString() ?? string.Empty, out var ratio))
                return TimeSpan.FromMilliseconds(0);

            if (TimeSpan.TryParse(parameter?.ToString() ?? string.Empty, out var baseTime))
                return TimeSpan.FromMilliseconds(baseTime.TotalMilliseconds / ratio);
            
            return TimeSpan.FromMilliseconds(0);
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