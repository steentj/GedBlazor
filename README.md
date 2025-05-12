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
- Does not currently support multimedia, notes, or sources
- Names are parsed according to standard GEDCOM conventions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

