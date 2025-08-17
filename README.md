# GedBlazor - GEDCOM Parser Web Application

A client-side Blazor WebAssembly application for parsing and displaying genealogical data stored in the GEDCOM file format. The application runs entirely in the user's browser and performs all data handling without server interaction.

## Features

- Upload and parse GEDCOM 5.5/5.5.1 files directly in the browser
- View individual records including:
  - Birth name and alternate names
  - Birth date and place
  - Death date and place
  - Family relationships (parents)
- Interactive table view with all individuals
- Completion Status per individual (colored dot next to names: red <2, yellow =2, green >2)
- Select a proband (root person) to visualize ancestry:
  - View ancestral tree with collapsible nodes
  - Display traditional Anetavle (ancestor table) up to 4 generations
  - Export Anetavle as Word document (.docx) formatted for A4 printing
- Completely client-side processing with no server uploads
- Built with Blazor WebAssembly for modern web performance

## Prerequisites

- .NET 9.0 SDK or later
- A modern web browser with WebAssembly support

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/steentj/GedBlazor.git
   cd GedBlazor
   ```

2. Build and run the application:
   ```bash
   dotnet build
   dotnet run --project GedBlazor
   ```

3. Open your browser and navigate to the URL shown in the console (typically https://localhost:5001)

## Development

### Project Structure

- `GedBlazor/` - Main application project
  - `Components/` - Blazor components including the GEDCOM uploader
  - `Models/` - Data models for GEDCOM entities
  - `Parsers/` - GEDCOM parsing logic
- `GedBlazor.Tests/` - Test project
  - Unit tests for components, models, and parsers

### Running Tests

```bash
dotnet test
```

## Completion Status

Each individual has a computed completeness status (0–4) indicated by a colored dot before the name in the Personer tab and Ancestry Tree, and available programmatically via `Individual.CompletionStatus`:

- 0: No name present
- 1: Name present (either given name or surname or both)
- 2: Status 1 plus a birth or death date (full date or year)
- 3: Status 2 plus: at least one source for birth or death, and both birth place and death place are present
- 4: Status 3 plus: one or more residencies

Dot colors:
- Red for status < 2, Yellow for status = 2, Green for status > 2

Notes:
- Sources are detected from GEDCOM tags `BIRT.SOUR` or `DEAT.SOUR` captured in `Individual.RawPersonalData`.
- Residency is detected when any `RESI` or `RESI.*` personal tag exists in `Individual.RawPersonalData`.
- Birth/death dates accept a full date or just a year; approximate dates (e.g., `ABT 1900`) are supported by `GedcomDate`.

## Technical Details

- Built with .NET 9.0 and Blazor WebAssembly
- Implements GEDCOM 5.5/5.5.1 parsing
- Uses in-memory storage for parsed genealogical data
- Client-side document generation with OpenXML
- JavaScript interop for browser-based file downloads
- No external services or backend dependencies

## Limitations

- Handles one GEDCOM file per session
- Supports GEDCOM versions 5.5 and 5.5.1
- Sources are captured for status computation but not yet displayed in the UI
- Names are parsed according to standard GEDCOM conventions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
