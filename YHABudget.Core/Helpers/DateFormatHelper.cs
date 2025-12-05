using System.Globalization;

namespace YHABudget.Core.Helpers;

public static class DateFormatHelper
{
    /// <summary>
    /// Formats a date as "Month Year" with Swedish culture (e.g., "December 2025")
    /// </summary>
    public static string FormatMonthYear(DateTime date)
    {
        // Format as "December 2025" with Swedish culture
        var culture = new CultureInfo("sv-SE");
        var formatted = date.ToString("MMMM yyyy", culture);

        // Capitalize first letter
        if (!string.IsNullOrEmpty(formatted))
        {
            formatted = char.ToUpper(formatted[0]) + formatted.Substring(1);
        }

        return formatted;
    }
}
