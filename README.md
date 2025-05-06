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
- No external services or backend dependencies

## Limitations

- Handles one GEDCOM file per session
- Supports GEDCOM versions 5.5 and 5.5.1
- Does not currently support multimedia, notes, or sources
- Names are parsed according to standard GEDCOM conventions

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License

Copyright (c) 2025 Steen Thrane Jacobsen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.