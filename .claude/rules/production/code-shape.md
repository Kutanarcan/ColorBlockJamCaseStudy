---
paths:
  - "Assets/Scripts/**"
---

# Code Shape

## Size & layout
- If a class exceeds 150-200 lines, split it and state why.
- If a method exceeds 30 lines or 3 levels of nesting, extract.
- One public type per file; the file is named after the type.

## Folders
- Folders group by domain concept, never by technical kind. `Enums/`, `Interfaces/`, `Structs/`, `Managers/`, `Helpers/`, `Misc/` are not concepts.
- A subfolder is named after a concept, never after a type it holds (`Level/Validation/`, not `Level/LevelValidator/`).
- Every folder holds at least two files. A single-file folder is a label — move the file up.
- **A folder holds at most 6 code files.** The change that would add the 7th splits the folder into concept subfolders in the same change, never later:
  - The folder root keeps its central types: the ones the rest of the assembly uses (e.g. `BoardView`, `BoardLayout`).
  - Subfolders hold what serves them, grouped by what they do together (e.g. `Board/Drawing/` for the draw rules and their output).
  - Move each `.meta` with its file so GUIDs survive.
- **Plan folders before writing.** When a phase's *Files touched* puts more than 6 files in one folder, the plan names the subfolders up front.
- **Check at the end of every phase:** count the code files in every folder the phase touched; a folder over 6 is not done.
- Folder depth stops at 2 under an assembly root. Deeper means the assembly should be split.
- Tests mirror the source folders down to the first level (V1 D31); a test folder follows the same 6-file limit.
- Namespaces stay at the asmdef root namespace and do not mirror folders.
- Before adding a file, name the concept it belongs to. If you cannot, the type is in the wrong assembly.

## SOLID — practical checks
- **SRP:** If describing the class requires "and", it is two classes.
- **OCP:** A `switch (enumType)` is a polymorphism candidate — but only if it will actually grow.
- **LSP:** Overriding to throw `NotImplementedException` means the hierarchy is wrong.
- **ISP:** An interface with 5+ members is suspect.
- **DIP:** Core never looks at Runtime (asmdef already prevents it).
