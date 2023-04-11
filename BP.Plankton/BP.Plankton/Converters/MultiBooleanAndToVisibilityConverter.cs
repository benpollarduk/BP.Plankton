using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace BP.Plankton.Converters
{
    /// <summary>
    /// Converts multiple boolean values to one Visibility. If all the booleans are true then Visibility.Visible is returned, else Visibility.Collapsed is returned.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (Visibility))]
    public class MultiBooleanAndToVisibilityConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        /// <summary>
        /// Converts source values to a target value for the binding target. The data binding engine calls this method when it propagates the values from the source bindings to the binding target.
        /// </summary>
        /// <param name="values">The array of values that the source bindings in the System.Windows.Data.MultiBinding produces. The value System.Windows.DependencyProperty.UnsetValue indicates that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue;

            if (values.Any(element => (element != null) && (bool.TryParse(element.ToString(), out booleanValue))))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        /// <summary>
        /// Converts a binding target value to the source binding values.
        /// </summary>
        /// <param name="value">The value that the target binidng produces.</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}