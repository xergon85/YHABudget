using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using YHABudget.Data.Enums;

namespace YHABudget.WPF.Converters;

public class DecimalToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            // Green for positive, red for negative, primary color for zero
            if (decimalValue > 0)
                return Application.Current.Resources["IncomeBrush"] as SolidColorBrush ?? Brushes.Green;
            else if (decimalValue < 0)
                return Application.Current.Resources["ExpenseBrush"] as SolidColorBrush ?? Brushes.Red;
            else
                return Application.Current.Resources["TextPrimaryBrush"] as SolidColorBrush ?? Brushes.Black;
        }
        return Application.Current.Resources["TextPrimaryBrush"] as SolidColorBrush ?? Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        var enumValue = value.ToString();
        var parameterValue = parameter.ToString();

        return enumValue?.Equals(parameterValue, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter == null || !(bool)value)
            return Binding.DoNothing;

        return Enum.Parse(targetType, parameter.ToString()!);
    }
}

public class EditModeTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditMode)
        {
            return isEditMode ? "Redigera transaktion" : "Ny transaktion";
        }
        return "Ny transaktion";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RecurringEditModeTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditMode)
        {
            return isEditMode ? "Redigera återkommande transaktion" : "Ny återkommande transaktion";
        }
        return "Ny återkommande transaktion";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RecurrenceTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RecurrenceType recurrenceType && recurrenceType == RecurrenceType.Yearly)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RecurrenceTypeToSwedishConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RecurrenceType recurrenceType)
        {
            return recurrenceType switch
            {
                RecurrenceType.Monthly => "Månadsvis",
                RecurrenceType.Yearly => "Årligen",
                _ => value.ToString() ?? string.Empty
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
