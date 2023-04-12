﻿using System;
using System.Globalization;
using System.Windows.Data;
using BP.Plankton.Model.Settings;

namespace BP.Plankton.Converters
{
    [ValueConversion(typeof(QuickSetting), typeof(string))]
    internal class QuickSettingToStringConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        /// <summary>Converts a value. </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Enum.TryParse(value?.ToString() ?? string.Empty, out QuickSetting setting))
                return string.Empty;

            switch (setting)
            {
                case QuickSetting.Random:
                    return "Random\n(Ctrl+N)";
                default:
                    return string.Empty;
            }
        }

        /// <summary>Converts a value. </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}