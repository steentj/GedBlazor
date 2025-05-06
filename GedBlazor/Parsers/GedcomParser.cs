using GedBlazor.Models;
using System.Text.RegularExpressions;

namespace GedBlazor.Parsers;

public partial class GedcomParser : IGedcomParser
{
    private Dictionary<string, Individual> _individuals = new();
    private Dictionary<string, Family> _families = new();

    public (Dictionary<string, Individual> individuals, Dictionary<string, Family> families) Parse(string content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        _individuals.Clear();
        _families.Clear();

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                          .Select(l => l.Trim())
                          .Where(l => !string.IsNullOrWhiteSpace(l))
                          .ToArray();
                          
        if (!IsValidGedcom(lines))
            throw new FormatException("Invalid GEDCOM format: Missing header or trailer");

        ParseRecords(lines);
        LinkFamilies();

        return (_individuals, _families);
    }

    private bool IsValidGedcom(string[] lines)
    {
        return lines.Length >= 2 && 
               lines[0].Equals("0 HEAD", StringComparison.OrdinalIgnoreCase) && 
               lines[^1].Equals("0 TRLR", StringComparison.OrdinalIgnoreCase);
    }

    private void ParseRecords(string[] lines)
    {
        Individual? currentIndividual = null;
        Family? currentFamily = null;
        string? currentTag = null;

        foreach (var line in lines)
        {
            try
            {
                var parts = line.Split(' ', 3);
                if (parts.Length < 2) continue;

                if (!int.TryParse(parts[0], out var level))
                    continue;

                if (level == 0)
                {
                    (currentIndividual, currentFamily) = ParseLevelZeroRecord(parts);
                    currentTag = null;
                }
                else if (level == 1)
                {
                    currentTag = ParseLevelOneRecord(parts, currentIndividual, currentFamily);
                }
                else if (level == 2)
                {
                    ParseLevelTwoRecord(parts, currentTag, currentIndividual);
                }
            }
            catch (Exception)
            {
                // Skip malformed lines
                continue;
            }
        }
    }

    private (Individual? individual, Family? family) ParseLevelZeroRecord(string[] parts)
    {
        if (parts.Length < 3) return (null, null);

        var id = parts[1];
        var tag = parts[2];

        if (id.StartsWith("@I") && tag == "INDI")
        {
            var individual = new Individual(id);
            _individuals[id] = individual;
            return (individual, null);
        }
        else if (id.StartsWith("@F") && tag == "FAM")
        {
            var family = new Family(id);
            _families[id] = family;
            return (null, family);
        }

        return (null, null);
    }

    private string? ParseLevelOneRecord(string[] parts, Individual? currentIndividual, Family? currentFamily)
    {
        if (parts.Length < 2) return null;
        var tag = parts[1];

        if (currentIndividual != null)
        {
            if (tag == "NAME" && parts.Length > 2)
            {
                var match = NameRegex().Match(parts[2]);
                if (match.Success)
                {
                    currentIndividual.SetName(
                        match.Groups["given"].Value.Trim(),
                        match.Groups["surname"].Value.Trim());
                }
            }
            return tag;
        }
        else if (currentFamily != null && parts.Length > 2)
        {
            ParseFamilyRecord(tag, parts[2], currentFamily);
        }

        return null;
    }

    private void ParseFamilyRecord(string tag, string value, Family family)
    {
        switch (tag)
        {
            case "HUSB":
                if (_individuals.TryGetValue(value, out var husband))
                    family.SetHusband(husband);
                break;
            case "WIFE":
                if (_individuals.TryGetValue(value, out var wife))
                    family.SetWife(wife);
                break;
            case "CHIL":
                if (_individuals.TryGetValue(value, out var child))
                    family.AddChild(child);
                break;
        }
    }

    private void ParseLevelTwoRecord(string[] parts, string? currentTag, Individual? currentIndividual)
    {
        if (parts.Length < 3 || currentIndividual == null || currentTag == null)
            return;

        var tag = parts[1];
        if (currentTag == "BIRT" && tag == "DATE")
        {
            try
            {
                currentIndividual.SetBirth(GedcomDate.Parse(parts[2]));
            }
            catch (Exception)
            {
                // Skip invalid date formats
            }
        }
    }

    private void LinkFamilies()
    {
        // Additional family linking logic can be added here
        // For example, adding back-references from individuals to their families
    }

    [GeneratedRegex(@"(?<given>[^/]+)/(?<surname>[^/]+)/")]
    private static partial Regex NameRegex();
}