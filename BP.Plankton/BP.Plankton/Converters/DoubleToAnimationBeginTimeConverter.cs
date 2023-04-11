using System;
using System.Globalization;
using System.Windows.Data;

namespace Plankton.Converters
{
    /// <summary>
    /// Converts between a Double representing a ratio and a TimeSpan. The TimeSpan to base the output on should be provided as the parameter in the form of a string hh:mm:ss.
    /// </summary>
    [ValueConversion(typeof (double), typeof (string))]
    public class DoubleToAnimationBeginTimeConverterConverter : IValueConverter
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
            double ratio;
            if ((value == null) || (!double.TryParse(value.ToString(), out ratio)))
                return TimeSpan.FromMilliseconds(0);

            TimeSpan baseTime;
            if ((parameter != null) && (TimeSpan.TryParse(parameter.ToString(), out baseTime)))
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