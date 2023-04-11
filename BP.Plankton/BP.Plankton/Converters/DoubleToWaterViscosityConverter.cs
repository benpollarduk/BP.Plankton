using System;
using System.Globalization;
using System.Windows.Data;

namespace BP.Plankton.Converters
{
    /// <summary>
    /// Converts between a Double and a String representing a water viscosity value. If a value other than 1 is provided it is returned as it is, if it is equal to 1 then infine (∞) is returned. 
    /// </summary>
    [ValueConversion(typeof (double), typeof (string))]
    public class DoubleToWaterViscosityConverter : IValueConverter
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
            double convertedValue;
            if ((value != null) && (double.TryParse(value.ToString(), out convertedValue)))
                return Math.Abs(convertedValue - 1) < 0.0 ? "∞" : value.ToString();

            return value;
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