using System;
using System.Globalization;
using System.Windows.Data;

namespace BP.Plankton.Converters
{
    /// <summary>
    /// Converts between a Double representing a panels height and a Double representing the maximum height of a zoom preview, which is half the value provided as the value parameter rounded down to the nearest 10. If an element other than a Double is provided as the value parameter Double.NaN is returned.
    /// </summary>
    [ValueConversion(typeof (double), typeof (double))]
    public class ZoomPreviewMaxHeightConverter : IValueConverter
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
            double v;
            if ((value == null) || (!double.TryParse(value.ToString(), out v)))
                return double.NaN;

            var flooredHeight = Math.Floor(v / 2);
            return flooredHeight - flooredHeight % 10;
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