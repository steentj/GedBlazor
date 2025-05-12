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

### 2.3 Define the family tree
- One individual is selected as root person ("Proband") and given "Anenummer" = 1
- The individual is selected by the user after the parsing of the GEDCOM file.
- The rest of the persons are, where possible, given Anenummer according to the Kekule von Stadonitz system (see addendum). Cousins, siblings etc which cannot be numbered get a -1 as number.


### 2.4 User Interface

- The primary output is three pages:
  - The Individuals Table
  - The Ancestry Tree
  - The Anetavle (ancestor table)
- These three pages are displayed in a tab view, where the user may choose one.

#### 2.4.1 Individuals Table
- A table view is displayed with all individuals and their information:
  - Anenummer
  - Name
  - Date/place of birth
  - Date/place of death
  - Father and mother (names, if resolvable)
- The table is sorted on the first name of the individuals initially.
- The user may sort the table on any column by clicking on the column header. Clickin sorts alternately ascending and descending.

### 2.4.2 Proband
- The root person (proband) is selected in a drop down populated with the individuals names. 
- After selection the "anenummer" is displayed as the first column in the individuals table. 

### 2.4.3 The Ancestry tree
- Is available when the proband has been chosen
- Shows the ancestry tree from the proband in a collapsible tree view where each node is the anenummer and the ancestor name
- The nodes are ordered by anenummer
- Individuals without an anenummer are not displayed

### 2.4.4 The Anetavle (Ancestor Table)
- Is available when the proband has been chosen
- Shows direct ancestors up to 4 generations in a traditional table format:
  - Bottom row: Proband
  - Second row: Parents (Far/Mor)
  - Third row: Grandparents (Farfar/Farmor/Morfar/Mormor)
  - Top row: Great-grandparents (8 ancestors)
- Each cell displays:
  - Detailed view (parents and proband): Anenummer, name, birth/death dates and places
  - Compact view (grandparents and great-grandparents): Anenummer, name, birth/death dates only, displayed vertically
- The layout resembles a traditional printed Anetavle with ancestors organized by generation
- Uses the Kekulé von Stradonitz System for numbering:
  - Proband: 1
  - Father: 2 (2n for any individual's father)
  - Mother: 3 (2n+1 for any individual's mother)
  - Paternal Grandfather: 4
  - Paternal Grandmother: 5
  - Maternal Grandfather: 6
  - Maternal Grandmother: 7
  - And so on for great-grandparents (8-15)
- Cells are visually distinct by generation:
  - Proband's cell has a highlight color
  - Parents cells have a subtler highlight
  - Empty cells where an ancestor is unknown are clearly marked
- The display is responsive and adapts to different screen sizes:
  - On small screens, the layout restructures to accommodate narrower viewports
  - Vertical text is used for higher generations to optimize space
- The Anetavle can be exported as a Word document (.docx):
  - The exported document maintains the same format as the on-screen display
  - The document is formatted to fit on a single A4 page in portrait orientation
  - All export processing happens client-side in the browser
  - The user can download the document directly to their device

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

## Addendums
### Kekulé von Stradonitz Numbering System (Sosa–Stradonitz / Ahnentafel)

The **Kekulé von Stradonitz system** is a genealogical numbering method used to identify individuals in a **direct ancestral line** of a person (the "proband" or starting person). It does **not** include siblings, cousins, or other collateral relatives.

#### Basic Rules:
- The **proband** (starting person) is assigned **number 1**.
- The **father** of any person `n` is assigned **2 × n**.
- The **mother** of any person `n` is assigned **2 × n + 1**.
- This continues recursively for each generation.

### Example:
| Person             | Number |
|--------------------|--------|
| Proband            | 1      |
| Father             | 2      |
| Mother             | 3      |
| Paternal Grandfather | 4    |
| Paternal Grandmother | 5    |
| Maternal Grandfather | 6    |
| Maternal Grandmother | 7    |

- Person 1's father is 2, and mother is 3.
- Person 2 (the father) has father 4 and mother 5.
- Person 3 (the mother) has father 6 and mother 7.

#### Notes:
- The system forms a **binary tree**, where:
  - Even numbers are **fathers**.
  - Odd numbers (except 1) are **mothers**.
- **Siblings**, **cousins**, **aunts/uncles**, and **descendants** are **not numbered** in this system.
- It is ideal for compact and systematic representation of **ancestral lines** only.

#### Related Terms:
- Also known as the **Ahnentafel** (German for "ancestor table") or **Sosa–Stradonitz** system (named after Jerónimo de Sosa and Stephan Kekulé von Stradonitz).