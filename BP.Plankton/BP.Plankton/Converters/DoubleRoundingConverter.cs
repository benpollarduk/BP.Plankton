using System;
using System.Globalization;
using System.Windows.Data;

namespace BP.Plankton.Converters
{
    /// <summary>
    /// Convert between a Double representing value and a rounded form returned as a Double. Specify a double as the parameter to control the precision.
    /// </summary>
    [ValueConversion(typeof (double), typeof (double))]
    public class DoubleRounderConverter : IValueConverter
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
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v;
            int p;
            if ((parameter == null) || (!int.TryParse(parameter.ToString(), out p)))
                p = 3;

            if ((value != null) && (double.TryParse(value.ToString(), out v)))
            {
                return Math.Abs(v) > 0.0d ? Math.Round(v, p) : v;
            }

            if (string.IsNullOrEmpty(value?.ToString()))
                return 0d;

            throw new ArgumentException("The value provided as the value parameter is not a double");
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v;

            if ((value != null) && (double.TryParse(value.ToString(), out v)))
                return v;

            if (string.IsNullOrEmpty(value?.ToString()))

                return 0d;

            throw new ArgumentException("The value provided as the value parameter is not a double");
        }

        #endregion
    }
}