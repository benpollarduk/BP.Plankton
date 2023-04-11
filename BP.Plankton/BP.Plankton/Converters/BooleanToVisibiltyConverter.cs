using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Plankton.Converters
{
    /// <summary>
    /// Represents a class for converting between Boolean and Visibility values. A boolean value can be specified as the paramater - this will deifne the state that Visibility.Visible is returned, if the boolean provided as the value parameter doesn't match this value then Visibilty.Hidden is returned.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (Visibility))]
    public class BooleanToVisibiltyConverter : IValueConverter
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
            bool v;
            if ((value == null) || (!bool.TryParse(value.ToString(), out v)))
                throw new ArgumentException();

            if (parameter == null)
                return v ? Visibility.Visible : Visibility.Hidden;

            bool p;
            if (bool.TryParse(parameter.ToString(), out p))
                return v == p ? Visibility.Visible : Visibility.Hidden;

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
            if (value == null)
                throw new ArgumentException();

            var v = (Visibility)Enum.Parse(typeof (Visibility), value.ToString());
            if (parameter == null)
                return v == Visibility.Visible;

            bool p;
            if (bool.TryParse(parameter.ToString(), out p))
                return v == Visibility.Visible ? p : !p;

            throw new ArgumentException();
        }

        #endregion
    }
}