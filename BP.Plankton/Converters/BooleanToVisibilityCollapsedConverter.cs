using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BP.Plankton.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    internal class BooleanToVisibilityCollapsedConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a value
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!bool.TryParse(value?.ToString() ?? string.Empty, out var b))
                return Visibility.Visible;

            if (parameter == null)
                return b ? Visibility.Visible : Visibility.Collapsed;

            if (!bool.TryParse(parameter.ToString(), out var p))
                return b == p ? Visibility.Visible : Visibility.Collapsed;

            throw new ArgumentException();
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