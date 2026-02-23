using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StylusCore.App.Shared.Converters
{
    /// <summary>
    /// Converts boolean to Visibility (True -> Visible, False -> Collapsed)
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean to Visibility (True -> Collapsed, False -> Visible)
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts null to Visibility (Null -> Collapsed, Not Null -> Visible)
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts WindowState to Visibility.
    /// Visible when Maximized by default, otherwise Collapsed.
    /// Pass "Inverse" parameter to invert behavior.
    /// </summary>
    public class WindowStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not WindowState windowState)
            {
                return Visibility.Collapsed;
            }

            if (parameter is string targetStateStr && Enum.TryParse<WindowState>(targetStateStr, out var targetState))
            {
                return windowState == targetState ? Visibility.Visible : Visibility.Collapsed;
            }

            // Fallback legacy Inverse logic
            var isInverse = string.Equals(parameter as string, "Inverse", StringComparison.OrdinalIgnoreCase);
            var isVisible = windowState == WindowState.Maximized;

            if (isInverse)
            {
                isVisible = !isVisible;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
