using GedBlazor.Models;

namespace GedBlazor.Services;

/// <summary>
/// Service interface for generating Word documents from genealogical data
/// </summary>
public interface IWordDocumentService
{
    /// <summary>
    /// Generates a Word document (.docx) containing the Anetavle (ancestor table)
    /// </summary>
    /// <param name="proband">The root person (proband) of the Anetavle</param>
    /// <param name="individuals">Dictionary of all individuals in the GEDCOM file</param>
    /// <returns>A byte array representing the Word document</returns>
    byte[] GenerateAnetavleDocument(Individual proband, Dictionary<string, Individual> individuals);
}
