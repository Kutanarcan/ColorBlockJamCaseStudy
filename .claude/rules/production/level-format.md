---
paths:
  - "Assets/Scripts/LevelIO/**"
  - "Assets/Scripts/Core/Level/**"
  - "Assets/Scripts/Core/Modifiers/**"
  - "Assets/Scripts/Tests/LevelIO/**"
---

# Level Format

Before changing anything under these paths, read `docs/production/LevelFormat.md` and follow it. It is mandatory.

- No runtime reflection in level IO (its §1 forbidden list).
- Every change follows its checklist in §4: new modifier, new field kind, new `LevelData` field, format change.
- Keys and `TypeName` strings are frozen (§3); changing one needs a `schemaVersion` bump (§4.4).
- Finish with the §6 review checklist and say which items applied.
- Update `LevelFormat.md` in the same change.
