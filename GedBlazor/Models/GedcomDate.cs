using System.Globalization;

namespace GedBlazor.Models;

public readonly record struct GedcomDate
{
    public int? Day { get; }
    public int? Month { get; }
    public int Year { get; }
    public bool IsApproximate { get; }

    public GedcomDate(int? day, int? month, int year, bool isApproximate = false)
    {
        Day = day;
        Month = month;
        Year = year;
        IsApproximate = isApproximate;
    }

    public static GedcomDate Parse(string gedcomDate)
    {
        if (string.IsNullOrWhiteSpace(gedcomDate))
            throw new ArgumentNullException(nameof(gedcomDate));

        var parts = gedcomDate.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Handle approximate dates
        if (parts[0].Equals("ABT", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length == 2 && int.TryParse(parts[1], out var abtYear))
                return new GedcomDate(null, null, abtYear, true);
            throw new FormatException("Invalid GEDCOM approximate date format");
        }

        // Handle exact dates (support both '1 JAN 1900' and '01.01.1900' and '1900')
        if (parts.Length == 3)
        {
            // Try '1 JAN 1900' format
            if (int.TryParse(parts[0], out var day) &&
                DateTime.TryParseExact(parts[1], new[] { "MMM", "MMMM" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate) &&
                int.TryParse(parts[2], out var year))
            {
                return new GedcomDate(day, monthDate.Month, year);
            }
            // Try '01.01.1900' format
            if (DateTime.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fullDate))
            {
                return new GedcomDate(fullDate.Day, fullDate.Month, fullDate.Year);
            }
        }
        // Try 'dd.MM.yyyy' as a single part
        if (parts.Length == 1 && DateTime.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var singleDate))
        {
            return new GedcomDate(singleDate.Day, singleDate.Month, singleDate.Year);
        }
        // Try just year
        if (parts.Length == 1 && int.TryParse(parts[0], out var yearOnly))
        {
            return new GedcomDate(null, null, yearOnly);
        }
        throw new FormatException($"Invalid GEDCOM date format: '{gedcomDate}'");
    }

    public override string ToString()
    {
        if (IsApproximate)
            return $"Ca {Year}";

        // Always format as dd-MM-yyyy (e.g., 01-01-1900)
        if (Day.HasValue && Month.HasValue)
            return $"{Day.Value:D2}-{Month.Value:D2}-{Year}";

        return Year.ToString();
    }
}