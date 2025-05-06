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
            return new GedcomDate(null, null, int.Parse(parts[1]), true);
        }

        // Handle exact dates
        if (parts.Length == 3)
        {
            var day = int.Parse(parts[0]);
            var month = DateTime.ParseExact(parts[1], "MMM", CultureInfo.InvariantCulture).Month;
            var year = int.Parse(parts[2]);
            return new GedcomDate(day, month, year);
        }

        throw new FormatException("Invalid GEDCOM date format");
    }

    public override string ToString()
    {
        if (IsApproximate)
            return $"Abt {Year}";

        if (Day.HasValue && Month.HasValue)
            return $"{Day.Value} {CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(Month.Value)} {Year}";

        return Year.ToString();
    }
}