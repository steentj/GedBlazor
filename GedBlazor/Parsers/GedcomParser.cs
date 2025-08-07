using GedBlazor.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing.Diagrams;

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

        // Split on any type of line ending
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                          .Select(l => l.Trim())
                          .Where(l => !string.IsNullOrWhiteSpace(l))
                          .ToArray();
                          
        var (isValid, reason) = IsValidGedcom(lines);
        if (!isValid)
            throw new FormatException($"Invalid GEDCOM format: {reason}");

        ParseRecords(lines);
        LinkFamilies();

        return (_individuals, _families);
    }

    private (bool isValid, string reason) IsValidGedcom(string[] lines)
    {
        // Clean and normalize lines to handle different line endings
        var normalizedLines = lines.Select(l => l.TrimEnd('\r', '\n', ' ', '\t')).ToList();
        
        if (!normalizedLines.Any())
            return (false, "GEDCOM file must contain at least a header and trailer");
            
        if (!normalizedLines[0].Equals("0 HEAD", StringComparison.OrdinalIgnoreCase))
            return (false, "GEDCOM file must start with '0 HEAD'");
            
        if (normalizedLines.Count < 2)
            return (false, "GEDCOM file must contain at least a header and trailer");
            
        // Look for TRLR record near the end - allow for some trailing whitespace/empty lines
        var lastNonEmptyLine = normalizedLines.FindLast(l => !string.IsNullOrWhiteSpace(l));
        if (lastNonEmptyLine == null || !lastNonEmptyLine.StartsWith("0 TRLR", StringComparison.OrdinalIgnoreCase))
            return (false, "GEDCOM file must end with '0 TRLR'");
        
        return (true, string.Empty);
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

        if (tag == "INDI")
        {
            var individual = new Individual(id);
            _individuals[id] = individual;
            return (individual, null);
        }
        else if (tag == "FAM")
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

        if (currentTag == "BIRT" || currentTag == "DEAT")
        {
            SetEventData(currentIndividual, currentTag, parts[1], parts[2]);
        }
    }

    private void SetEventData(Individual individual, string eventType, string tag, string value)
    {
        // eventType: "BIRT" or "DEAT"
        if (tag == "DATE")
        {
            try
            {
                if (eventType == "BIRT")
                    individual.SetBirth(GedcomDate.Parse(value));
                else if (eventType == "DEAT")
                    individual.SetDeath(GedcomDate.Parse(value));
            }
            catch (Exception)
            {
                // Skip invalid date formats
            }
        }
        else if (tag == "PLAC")
        {
            if (eventType == "BIRT")
                individual.BirthPlace = value;
            else if (eventType == "DEAT")
                individual.DeathPlace = value;
        }
        else if (tag == "AGNC")
        {
            if (eventType == "BIRT" && string.IsNullOrEmpty(individual.BirthPlace))
                individual.BirthPlace = value;
            else if (eventType == "DEAT" && string.IsNullOrEmpty(individual.DeathPlace))
                individual.DeathPlace = value;
        }
    }

    private void LinkFamilies()
    {
        foreach (var family in _families.Values)
        {
            var fatherId = family.Husband?.Id;
            var motherId = family.Wife?.Id;
            
            foreach (var child in family.Children)
            {
                if (!string.IsNullOrEmpty(fatherId))
                {
                    child.FatherId = fatherId;
                }

                if (!string.IsNullOrEmpty(motherId))
                {
                    child.MotherId = motherId;
                }
            }
        }
    }

    public void AssignAnenummer(Dictionary<string, Individual> individuals, string? probandId)
    {
        // Reset all anenummer values
        foreach (var ind in individuals.Values)
            ind.Anenummer = -1;
        if (string.IsNullOrEmpty(probandId) || !individuals.ContainsKey(probandId))
            return;
        AssignAnenummerRecursive(individuals, probandId, null, 1);
    }

    private void AssignAnenummerRecursive(Dictionary<string, Individual> individuals, string? id, string? childLink, int anenummer)
    {
        if (string.IsNullOrEmpty(id) || !individuals.TryGetValue(id, out var ind) || ind.Anenummer > 0)
            return;
        ind.Anenummer = anenummer;
        ind.ChildLink = childLink;
        if (!string.IsNullOrEmpty(ind.FatherId))
            AssignAnenummerRecursive(individuals, ind.FatherId, ind.Id, 2 * anenummer);
        if (!string.IsNullOrEmpty(ind.MotherId))
            AssignAnenummerRecursive(individuals, ind.MotherId, ind.Id, 2 * anenummer + 1);
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"(?<given>[^/]+)/(?<surname>[^/]+)/")]
    private static partial Regex NameRegex();
}