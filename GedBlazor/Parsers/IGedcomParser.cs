using GedBlazor.Models;

namespace GedBlazor.Parsers;

public interface IGedcomParser
{
    /// <summary>
    /// Parses GEDCOM content and returns collections of individuals and families.
    /// </summary>
    /// <param name="content">The GEDCOM file content to parse</param>
    /// <returns>A tuple containing dictionaries of individuals and families</returns>
    /// <exception cref="ArgumentNullException">Thrown when content is null</exception>
    /// <exception cref="FormatException">Thrown when content is not valid GEDCOM format</exception>
    (Dictionary<string, Individual> individuals, Dictionary<string, Family> families) Parse(string content);

    void AssignAnenummer(Dictionary<string, GedBlazor.Models.Individual> individuals, string? probandId);
}