# User Requirements Specification – Blazor GEDCOM Parser

## 1. Project Overview

This is a client-side Blazor WebAssembly application for parsing and displaying genealogical data stored in the GEDCOM file format. The application runs entirely in the user's browser and performs all data handling without server interaction.

---

## 2. Functional Requirements

### 2.1 File Loading
- The user selects a `.ged` file using a standard file input (`<input type="file">`).
- The file is read locally using JavaScript interop or .NET file APIs in the browser.

### 2.2 GEDCOM Parsing

#### 2.2.1 Individuals
- All `INDI` entries are parsed and stored in a `Dictionary<string, Individual>`, where the key is the GEDCOM ID (e.g. `@I123@`).
- For each individual, the following data is extracted:
  - **Name**:
    - If multiple names exist:
      - Display the *birth name* as the main name.
      - Add any alternate name (e.g., married name) in parentheses.
  - **Birth**:
    - Date and place of birth.
  - **Death**:
    - Date and place of death.

#### 2.2.2 Family Links
- Family (`FAM`) entries are parsed to determine relationships.
- Each `Individual` is updated with:
  - `FatherId`: reference to the father's GEDCOM ID.
  - `MotherId`: reference to the mother's GEDCOM ID.

### 2.3 User Interface

#### 2.3.1 Individuals Table
- A table view is displayed with all individuals and their information:
  - Name
  - Date/place of birth
  - Date/place of death
  - Father and mother (names, if resolvable)

- The table supports sorting by name or birth date (optional).

---

## 3. Non-functional Requirements

- Entirely browser-based (Blazor WebAssembly).
- No server communication or file uploads.
- Parsing should handle well-formed GEDCOM 5.5 files.
- UI should remain responsive, even for large family trees (~1000 individuals).

---

## 4. Technical Constraints

- The application must be built using Blazor WebAssembly.
- All parsing must be implemented in .NET code, with no reliance on external services.
- The dictionary of individuals should remain in memory only (not persisted).
- No backend or cloud storage is used.

---

## 5. Open Questions / Assumptions

- Only one GEDCOM file is handled per session.
- Assumes GEDCOM files follow version 5.5 or 5.5.1 format.
- No support yet for multimedia, notes, or sources.
- Names from `NAME` tags are assumed to follow GEDCOM conventions.

---

## 6. Glossary

| Term | Meaning |
|------|---------|
| GEDCOM | A plain-text format used to encode genealogical data. |
| INDI | A GEDCOM tag denoting an individual person. |
| FAM  | A GEDCOM tag defining a family group (parents and children). |
| GEDCOM ID | A unique identifier like `@I123@` for individuals or `@F45@` for families. |