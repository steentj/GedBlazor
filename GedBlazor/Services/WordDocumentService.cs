using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using GedBlazor.Models;

namespace GedBlazor.Services;

/// <summary>
/// Service for generating Word documents from genealogical data
/// </summary>
public class WordDocumentService : IWordDocumentService
{
    const int AntalGenerationer = 5;
    const int SideBredde = 12240; // A4 width in twentieths of a point
    /// <summary>
    /// Generates a Word document (.docx) containing the Anetavle (ancestor table)
    /// </summary>
    /// <param name="proband">The root person (proband) of the Anetavle</param>
    /// <param name="individuals">Dictionary of all individuals in the GEDCOM file</param>
    /// <returns>A byte array representing the Word document</returns>
    public byte[] GenerateAnetavleDocument(Individual proband, Dictionary<string, Individual> individuals)
    {
        using var memoryStream = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(
            memoryStream, 
            WordprocessingDocumentType.Document))
        {
            // Add a main document part
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            
            mainPart.Document.AddNamespaceDeclaration("w", 
                "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            var body = mainPart.Document.AppendChild(new Body());
            
            // Create styles
            AddStyles(wordDocument);
            
            // Add title
            body.AppendChild(
                new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = "Title" }),
                    new Run(
                        new Text($"Anetavle for {proband.FullName}")
                    )
                )
            );

            // Create a table with 16 rows and 8 columns
            var table = CreateAnetavleTable(proband, individuals);
            body.AppendChild(table);
            
            // Add page orientation to be Portrait
            AddPageOrientation(wordDocument, true);
            
            // Save the document
            mainPart.Document.Save();
        } // wordDocument is properly closed here when the using block ends

        // Reset stream position to beginning before reading
        memoryStream.Position = 0;
        
        return memoryStream.ToArray();
    }
    
    /// <summary>
    /// Creates the Anetavle table structure
    /// </summary>
    private Table CreateAnetavleTable(Individual proband, Dictionary<string, Individual> individuals)
    {
        var table = new Table();
        // var table = new Table();
        // Define table properties with clear borders and width
        var tableProperties = new TableProperties(
            new TableLayout() { Type = TableLayoutValues.Fixed },
            new TableWidth() { Width = (SideBredde - (2 * 720)).ToString(), Type = TableWidthUnitValues.Dxa },
            new TableBorders(
                new TopBorder() { Val = BorderValues.Single, Size = 6 },
                new BottomBorder() { Val = BorderValues.Single, Size = 6 },
                new LeftBorder() { Val = BorderValues.Single, Size = 6 },
                new RightBorder() { Val = BorderValues.Single, Size = 6 },
                new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 6 },
                new InsideVerticalBorder() { Val = BorderValues.Single, Size = 6 }
            ),
            new TableLook() { Val = "04A0", FirstRow = true, LastRow = false, FirstColumn = true, LastColumn = false, NoHorizontalBand = false, NoVerticalBand = true }
        );
        table.AppendChild(tableProperties);
    
        // Define grid columns - create 16 equal columns
        var tableGrid = new TableGrid();
        int columnWidth = (SideBredde - (2 * 720)) / 16;
        for (int i = 0; i < 16; i++)
        {
            tableGrid.AppendChild(new GridColumn() { Width = columnWidth.ToString() });
        }
        table.AppendChild(tableGrid);
        
        // Create rows and cells for great-grandparents (16 cells)
        var gggRow = new TableRow
        {
            TableRowProperties = new TableRowProperties(
                new TableRowHeight() { Val = 3000, HeightType = HeightRuleValues.AtLeast }
            )
        };

        for (int i = 16; i <= 31; i++)
        {
            var cell = CreateAnetavleCell(GetAncestor(i, individuals), i, true);
            gggRow.Append(cell);
        }
        table.Append(gggRow);
        
        // Create rows and cells for great-grandparents (8 cells)
        var ggRow = new TableRow
        {
            TableRowProperties = new TableRowProperties(
                new TableRowHeight() { Val = 3000, HeightType = HeightRuleValues.AtLeast }
            )
        };

        for (int i = 8; i <= 15; i++)
        {
            var cell = CreateAnetavleCell(GetAncestor(i, individuals), i, true);
            ggRow.Append(cell);
        }
        table.Append(ggRow);
        
        // Create rows and cells for grandparents (4 cells)
        var gRow = new TableRow
        {
            TableRowProperties = new TableRowProperties(
                new TableRowHeight() { Val = 288, HeightType = HeightRuleValues.AtLeast }
            )
        };
        
        for (int i = 4; i <= 7; i++)
        {
            var cell = CreateAnetavleCell(GetAncestor(i, individuals), i, false);
            gRow.Append(cell);
        }
        table.Append(gRow);
        
        // Create row and cells for parents (2 cells)
        var pRow = new TableRow
        {
            TableRowProperties = new TableRowProperties(
                new TableRowHeight() { Val = 288, HeightType = HeightRuleValues.AtLeast }
            )
        };
        
        for (int i = 2; i <= 3; i++)
        {
            var cell = CreateAnetavleCell(GetAncestor(i, individuals), i, false);
            pRow.Append(cell);
        }
        table.Append(pRow);
        
        // Create row and cell for proband (1 cell)
        var pbRow = new TableRow
        {
            TableRowProperties = new TableRowProperties(
                new TableRowHeight() { Val = 288, HeightType = HeightRuleValues.AtLeast }
            )
        };
        
        var probandCell = CreateAnetavleCell(proband, 1, false);
        pbRow.Append(probandCell);
        table.Append(pbRow);
        
        return table;
    }
    
    /// <summary>
    /// Creates a single cell for the Anetavle table
    /// </summary>
    private TableCell CreateAnetavleCell(Individual? individual, int anenummer, bool minimal)
    {
        var cell = new TableCell();
    
        // Add cell properties
        var cellProperties = new TableCellProperties(
            new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center },
            new Shading() { Val = ShadingPatternValues.Clear, Color = "auto", Fill = GetCellBackgroundColor(anenummer) }
        );

        switch (anenummer)
        {
            // Set the GridSpan based on anenummer (only set it once)
            case 1:
                // Proband spans all 16 columns
                cellProperties.AppendChild(new GridSpan() { Val = 16 });
                break;
            case <= 3:
                // Parents span 8 columns each
                cellProperties.AppendChild(new GridSpan() { Val = 8 });
                break;
            case <= 7:
                // Grandparents span 4 columns each
                cellProperties.AppendChild(new GridSpan() { Val = 4 });
                break;
            case <= 15:
                // Great-grandparents span 2 columns each
                cellProperties.AppendChild(new GridSpan() { Val = 2 });
                break;
        }
    
        cellProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
        
        if (minimal)
        {
            cellProperties.AppendChild(
                new TextDirection() { Val = TextDirectionValues.BottomToTopLeftToRight });
        }
        
        cell.AppendChild(cellProperties);
        
        if (individual != null)
        {
            // Add Anenummer
            var anenummerPara = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = "AnenummerStyle" },
                    new Justification() { Val = JustificationValues.Center }
                ),
                new Run(new Text($"#{anenummer}"))
            );
            cell.AppendChild(anenummerPara);
            
            // Add name
            var namePara = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = "NameStyle" },
                    new Justification() { Val = JustificationValues.Center }
                ),
                new Run(new Text(individual.FullName))
            );
            cell.AppendChild(namePara);
            
            // Add birth info
            if (individual.BirthDate != null)
            {
                var birthDatePara = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = "DateStyle" },
                        new Justification() { Val = JustificationValues.Center }
                    ),
                    new Run(new Text($"*{individual.BirthDate}"))
                );
                cell.AppendChild(birthDatePara);
                
                // Add birthplace if not minimal and place exist
                if (!minimal && !string.IsNullOrEmpty(individual.BirthPlace))
                {
                    var birthPlacePara = new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId() { Val = "PlaceStyle" },
                            new Justification() { Val = JustificationValues.Center }
                        ),
                        new Run(new Text(individual.BirthPlace))
                    );
                    cell.AppendChild(birthPlacePara);
                }
            }
            
            // Add death info
            if (individual.DeathDate != null)
            {
                var deathDatePara = new Paragraph(
                    new ParagraphProperties(
                        new ParagraphStyleId() { Val = "DateStyle" },
                        new Justification() { Val = JustificationValues.Center }
                    ),
                    new Run(new Text($"†{individual.DeathDate}"))
                );
                cell.AppendChild(deathDatePara);
                
                // Add death place if not minimal and place exist
                if (!minimal && !string.IsNullOrEmpty(individual.DeathPlace))
                {
                    var deathPlacePara = new Paragraph(
                        new ParagraphProperties(
                            new ParagraphStyleId() { Val = "PlaceStyle" },
                            new Justification() { Val = JustificationValues.Center }
                        ),
                        new Run(new Text(individual.DeathPlace))
                    );
                    cell.AppendChild(deathPlacePara);
                }
            }
        }
        else
        {
            // Empty cell
            var emptyPara = new Paragraph(
                new ParagraphProperties(
                    new ParagraphStyleId() { Val = "EmptyStyle" },
                    new Justification() { Val = JustificationValues.Center }
                ),
                new Run(new Text($"Ingen ane ({anenummer})"))
            );
            cell.AppendChild(emptyPara);
        }
        
        return cell;
    }
    
    /// <summary>
    /// Gets an individual by their Anenummer
    /// </summary>
    private Individual? GetAncestor(int anenummer, Dictionary<string, Individual> individuals)
    {
        // Debug check - log or check if any individuals have Anenummer set
        // Console.WriteLine($"Looking for anenummer {anenummer}, total individuals: {individuals.Count}");
        
        var ancestor = individuals.Values.FirstOrDefault(i => i.Anenummer == anenummer);
        
        // If not found by Anenummer, try to find by relationships
        if (ancestor == null && anenummer > 1)
        {
            // For fathers (even numbers), look for father of anenummer/2
            // For mothers (odd numbers), look for mother of (anenummer-1)/2
            int childAnenummer = anenummer % 2 == 0 ? anenummer / 2 : (anenummer - 1) / 2;
            
            var child = individuals.Values.FirstOrDefault(i => i.Anenummer == childAnenummer);
            if (child != null)
            {
                if (anenummer % 2 == 0 && !string.IsNullOrEmpty(child.FatherId))
                {
                    // Even anenummer = father
                    individuals.TryGetValue(child.FatherId, out ancestor);
                    if (ancestor != null) ancestor.Anenummer = anenummer;
                }
                else if (anenummer % 2 == 1 && !string.IsNullOrEmpty(child.MotherId))
                {
                    // Odd anenummer = mother
                    individuals.TryGetValue(child.MotherId, out ancestor);
                    if (ancestor != null) ancestor.Anenummer = anenummer;
                }
            }
        }
        
        return ancestor;
    }
    
    /// <summary>
    /// Gets the background color for a cell based on the Anenummer
    /// </summary>
    private string GetCellBackgroundColor(int anenummer)
    {
        return anenummer switch
        {
            1 => "E6F7FF", // Proband
            2 or 3 => "F0F5FF", // Parents
            _ => "FFFFFF"  // Others
        };
    }
    
    /// <summary>
    /// Adds document styles for consistent formatting
    /// </summary>
    private void AddStyles(WordprocessingDocument document)
    {
        // Check if MainDocumentPart is null
        if (document.MainDocumentPart == null)
        {
            return;
        }
        
        // Get the StyleDefinitionsPart or create it if it doesn't exist
        var stylesPart = document.MainDocumentPart.StyleDefinitionsPart;
        if (stylesPart == null)
        {
            stylesPart = document.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles();
        }
        
        // Ensure Styles is not null
        stylesPart.Styles ??= new Styles();
        
        // Create Title style
        var titleStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Title",
            CustomStyle = true
        };
        titleStyle.AppendChild(new StyleName { Val = "Title" });
        titleStyle.AppendChild(new BasedOn { Val = "Normal" });
        titleStyle.AppendChild(new ParagraphProperties(
            new SpacingBetweenLines { After = "240", Line = "288", LineRule = LineSpacingRuleValues.Auto },
            new Justification { Val = JustificationValues.Center }
        ));
        titleStyle.AppendChild(new RunProperties(
            new Bold(),
            new FontSize { Val = "32" }
        ));
        stylesPart.Styles.AppendChild(titleStyle);
        
        // Create Anenummer style
        var anenummerStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "AnenummerStyle",
            CustomStyle = true
        };
        anenummerStyle.AppendChild(new StyleName { Val = "AnenummerStyle" });
        anenummerStyle.AppendChild(new BasedOn { Val = "Normal" });
        anenummerStyle.AppendChild(new ParagraphProperties(
            new SpacingBetweenLines { After = "60", Before = "60", Line = "240", LineRule = LineSpacingRuleValues.Auto }
        ));
        anenummerStyle.AppendChild(new RunProperties(
            new FontSize { Val = "18" },
            new Color { Val = "666666" }
        ));
        stylesPart.Styles.AppendChild(anenummerStyle);
        
        // Create Name style
        var nameStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "NameStyle",
            CustomStyle = true
        };
        nameStyle.AppendChild(new StyleName { Val = "NameStyle" });
        nameStyle.AppendChild(new BasedOn { Val = "Normal" });
        nameStyle.AppendChild(new ParagraphProperties(
            new SpacingBetweenLines { After = "60", Before = "60", Line = "240", LineRule = LineSpacingRuleValues.Auto }
        ));
        nameStyle.AppendChild(new RunProperties(
            new Bold(),
            new FontSize { Val = "22" }
        ));
        stylesPart.Styles.AppendChild(nameStyle);
        
        // Create Date style
        var dateStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "DateStyle",
            CustomStyle = true
        };
        dateStyle.AppendChild(new StyleName { Val = "DateStyle" });
        dateStyle.AppendChild(new BasedOn { Val = "Normal" });
        dateStyle.AppendChild(new ParagraphProperties(
            new SpacingBetweenLines { After = "60", Before = "60", Line = "240", LineRule = LineSpacingRuleValues.Auto }
        ));
        dateStyle.AppendChild(new RunProperties(
            new FontSize { Val = "20" }
        ));
        stylesPart.Styles.AppendChild(dateStyle);
        
        // Create Place style
        var placeStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "PlaceStyle",
            CustomStyle = true
        };
        placeStyle.AppendChild(new StyleName { Val = "PlaceStyle" });
        placeStyle.AppendChild(new BasedOn { Val = "Normal" });
        placeStyle.AppendChild(new ParagraphProperties(
            new SpacingBetweenLines { After = "60", Before = "60", Line = "240", LineRule = LineSpacingRuleValues.Auto }
        ));
        placeStyle.AppendChild(new RunProperties(
            new FontSize { Val = "18" },
            new Italic(),
            new Color { Val = "666666" }
        ));
        stylesPart.Styles.AppendChild(placeStyle);
        
        // Create Empty style
        var emptyStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "EmptyStyle",
            CustomStyle = true
        };
        emptyStyle.AppendChild(new StyleName { Val = "EmptyStyle" });
        emptyStyle.AppendChild(new BasedOn { Val = "Normal" });
        emptyStyle.AppendChild(new ParagraphProperties(
            new SpacingBetweenLines { After = "60", Before = "60", Line = "240", LineRule = LineSpacingRuleValues.Auto },
            new Justification { Val = JustificationValues.Center }
        ));
        emptyStyle.AppendChild(new RunProperties(
            new FontSize { Val = "18" },
            new Italic(),
            new Color { Val = "999999" }
        ));
        stylesPart.Styles.AppendChild(emptyStyle);
        
        stylesPart.Styles.Save();
    }
    
    /// <summary>
    /// Sets page orientation (portrait or landscape)
    /// </summary>
    private void AddPageOrientation(WordprocessingDocument document, bool isPortrait)
    {
        var sectionProperties = new SectionProperties();
        var pageSize = new PageSize();
        
        if (isPortrait)
        {
            pageSize.Width = SideBredde; // A4 width in twentieths of a point
            pageSize.Height = 15840; // A4 height in twentieths of a point
        }
        else
        {
            pageSize.Width = 15840; // A4 height in twentieths of a point
            pageSize.Height = 12240; // A4 width in twentieths of a point
            pageSize.Orient = PageOrientationValues.Landscape;
        }
        
        sectionProperties.AppendChild(pageSize);
        
        // Add page margins
        var pageMargin = new PageMargin
        {
            Top = 720, // 0.5 inch (720 twentieths of a point)
            Right = 720,
            Bottom = 720,
            Left = 720,
            Header = 0,
            Footer = 0
        };
        sectionProperties.AppendChild(pageMargin);
        
        // Check if MainDocumentPart and Document and Body exist before appending
        document.MainDocumentPart?.Document.Body?.AppendChild(sectionProperties);
    }
}
