using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using YHABudget.Data.Enums;

namespace YHABudget.WPF.Converters;

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
