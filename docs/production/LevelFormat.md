<div align="center">

# 💾 Level Format

**The level JSON format and the rules for changing it: required reading before touching level data, modifiers or LevelIO**

![Schema](https://img.shields.io/badge/schemaVersion-2-1f6feb)
![Reflection](https://img.shields.io/badge/Runtime_reflection-none-2ea043)

<sub>[README](../../README.md) · [ProductionV1](ProductionV1.md) · [Mechanics Reference](ColorBlockJamMechanics.md)</sub>

</div>

> [!IMPORTANT]
> **These rules are mandatory.** Level IO is mapped by hand, so the compiler cannot catch every mistake. A rule skipped here shows up later as a level that will not load, or as a field that is silently lost. Every change listed in §4 follows its checklist, and every pull request touching these files passes the review checklist in §6.

| # | Section | What is in it |
|---|---|---|
| 1 | [Why it is hand-written](#1-why-it-is-hand-written) | No runtime reflection, and what that costs |
| 2 | [Format v2](#2-format-v2) | Every key, its type, whether it is required, and the version history |
| 3 | [Frozen names](#3-frozen-names) | Strings that are the save format and must never change |
| 4 | [Change checklists](#4-change-checklists) | Step by step, for every kind of change |
| 5 | [Tests that guard the format](#5-tests-that-guard-the-format) | Which test catches which mistake |
| 6 | [Review checklist](#6-review-checklist) | For every pull request touching level data |
| 7 | [File map](#7-file-map) | Where each piece lives |

---

## 1. Why it is hand-written
- **No runtime reflection (D48).** IL2CPP managed stripping removes types and members that are only reached by reflection. A level that loads in the Editor could then fail on a device.
- **What replaces it:**
  - Each modifier DTO declares its saved name (`TypeName`) and writes and reads its own fields through `IModifierWriter` / `IModifierReader`.
  - `ModifierCatalog` lists every known modifier explicitly.
  - `LevelJsonReader` / `LevelJsonWriter` map `LevelData` by hand over Newtonsoft `JObject`.
- **Forbidden in LevelIO:**
  - `JsonConvert.DeserializeObject`, `JsonSerializer.Deserialize`, `JToken.ToObject` and `JObject.FromObject`
  - `StringEnumConverter` and `Enum.Parse`
  - attributes read through reflection
- **The cost:** every field is written twice, once in a writer and once in a reader. The checklists below exist to keep those two in step.

## 2. Format v2

### Level object
| Key | Type | Required | Notes |
|---|---|---|---|
| `schemaVersion` | integer | ✔ | Must be `2` (`LevelValidator.SupportedSchemaVersion`) |
| `width`, `height` | integer | ✔ | Above zero |
| `timeLimit` | number | ✔ | Seconds |
| `walls` | array of wall | — | Missing or `null` reads as empty |
| `doors` | array of door | — | Missing or `null` reads as empty |
| `blocks` | array of block | — | Missing or `null` reads as empty |

### Entities
| Entity | Keys |
|---|---|
| Wall | `cells` ✔ |
| Door | `colorId` ✔ integer · `direction` ✔ · `cells` ✔ |
| Block | `colorId` ✔ integer · `cells` ✔ · `modifiers` (array, optional) |

### Values
- **Cell:** `[x, y]`, two integers.
- **Direction:** one of the strings `"Up"`, `"Down"`, `"Left"`, `"Right"`. A number is rejected.
- **Axis:** one of the strings `"Horizontal"`, `"Vertical"`. A number or a direction name is rejected.
- **Modifier:** an object with `"type"` (its `TypeName`) plus the fields that modifier writes.

### Errors
- **Two kinds of check:**
  - Parsing rejects malformed JSON, missing or wrongly typed keys, and unknown modifier types.
  - `LevelSession.TryCreate` rejects structural problems: version, bounds, overlap and modifier values.
- **Every parse error names its path**, e.g. `level.blocks[0].modifiers[1] has no "count"`.

```json
{
  "schemaVersion": 2,
  "width": 8, "height": 10,
  "timeLimit": 90,
  "walls":  [ { "cells": [[0,0],[1,0],[2,0]] } ],
  "doors":  [ { "colorId": 2, "direction": "Down", "cells": [[3,0],[4,0]] } ],
  "blocks": [
    { "colorId": 2, "cells": [[3,1],[4,1]],
      "modifiers": [ { "type": "ice", "count": 3 }, { "type": "arrow", "axis": "Vertical" } ] }
  ]
}
```

### Version history
| Version | Change | Old shape | New shape | Existing levels |
|---|---|---|---|---|
| 1 | First format | — | — | — |
| 2 | Arrow is an axis lock, not one direction (D105) | `{ "type": "arrow", "direction": "Left" }` | `{ "type": "arrow", "axis": "Horizontal" }` | Re-saved by hand (none shipped): Left / Right → Horizontal, Up / Down → Vertical. A v1 file fails the version check |

## 3. Frozen names
Every key and every `TypeName` is part of the save format. **Renaming a C# class or field is safe; changing one of these strings breaks every saved level.** Change them only through a format change (§4.4).

| Modifier | `TypeName` | Fields |
|---|---|---|
| Ice | `"ice"` | `count`: integer, > 0 |
| Arrow | `"arrow"` | `axis`: Axis |

> Add a row here in the same pull request that adds a modifier.

## 4. Change checklists

### 4.1 Adding a modifier
1. **DTO** in the mechanic's own folder under `Core/Modifiers/` (D31). It overrides:
   - `TypeName`: lowercase and unique, never reused
   - `ToModifier`
   - `Write` / `Read`, using **the same key strings** in both
   - `Validate`, if the values have limits
2. **Catalog:** add one line to `ModifierCatalog.Default()`.
3. **Round-trip test:**
   - Add an instance with **non-default values** to `LevelJsonTests.SampleLevel()`.
   - Assert every field in `RoundTrip_PreservesAllModifiers`.
4. **Validation test:** if `Validate` is overridden, add a rejection test in `LevelValidatorTests`.
5. **Docs:** add the modifier's row to §3.

### 4.2 Adding a field kind (float, bool, cell, list…)
1. Add one method each to `IModifierWriter` and `IModifierReader` (Core).
2. Implement it in `JsonModifierWriter` / `JsonModifierReader`, with a typed read in `JsonRead`. The read must reject a wrong type with a path.
3. Make the Level Editor draw it: a `FieldKind` value, storage in `ModifierFields` (both interfaces), and a case in `SelectionPanel.DrawField`.
4. Add a rejection test for the wrong type in `LevelJsonTests`.
5. List the new value type in §2 (Values).

### 4.3 Adding a `LevelData` or entity field
1. Write it in `LevelJsonWriter` and read it in `LevelJsonReader`, with the same key.
2. Decide whether it is required.
   - **Adding a required key breaks every existing level**, so it is a format change: follow §4.4.
   - An optional key with a safe default does not need a version bump.
3. Assert it in `RoundTrip_PreservesGridAndEntities`. **A writer that forgets the field loses it silently**; only this assertion catches that.
4. If it has structural limits, check them in `LevelValidator` and add a test.
5. Add it to the tables in §2.

### 4.4 Changing the format (rename, remove, retype, new required key)
1. Increase `LevelValidator.SupportedSchemaVersion` and write the change down in this file, with the old and new shapes.
2. Decide what happens to existing levels:
   - **Migrate on load:** a migration step from version N to N + 1. Not built yet; it must be designed before the first breaking change.
   - **Re-save every level:** with the Level Editor, and only if no level has shipped.
3. Add a decision to the plan's Decision Log.

### 4.5 Adding a `Direction` value
- Update both `switch` statements in `DirectionNames` and add the string to §2.

### 4.6 Wall or door modifiers (documented edit point, D41)
- Treat the new `modifiers` key on walls and doors as §4.3: optional key, round-trip assertion, docs.

## 5. Tests that guard the format

| Test | Catches |
|---|---|
| `RoundTrip_PreservesAllModifiers` | A modifier field that is not written or not read back |
| `RoundTrip_PreservesGridAndEntities` | A level or entity field lost by the writer or reader |
| `RoundTrip_IsStable` | Output that changes between saves (noisy diffs) |
| `Parse_ReadsTheDocumentedFormat` | Drift between this document and the code |
| `Parse_Rejects_*` | A wrongly typed or missing value that is let through |
| `Serialize_ModifierNotInCatalog_Throws` | A modifier saved without being in the catalog |
| `ModifierCatalogTests` | Duplicate type names; unknown names |
| `LevelValidatorTests` | Structural and per-modifier value checks |

## 6. Review checklist
For every pull request that touches `Core/Level/`, `Core/Modifiers/` or `LevelIO/`:

- [ ] No runtime reflection added (§1 forbidden list).
- [ ] No existing key or `TypeName` changed, unless the change follows §4.4.
- [ ] `Write` and `Read` use the same keys.
- [ ] A new modifier is in `ModifierCatalog.Default()`.
- [ ] A new modifier has a round-trip assertion with non-default values.
- [ ] A new level or entity field has a round-trip assertion.
- [ ] A new required key bumps `schemaVersion`.
- [ ] This document is updated (§2, §3).

## 7. File map

| Piece | File |
|---|---|
| DTOs | `Core/Level/LevelData.cs`, `Core/Level/Data/*Data.cs` |
| Field IO contract | `Core/Level/Data/IModifierWriter.cs`, `IModifierReader.cs` |
| Catalog | `Core/Modifiers/ModifierCatalog.cs` |
| Structural checks | `Core/Level/Validation/LevelValidator.cs` |
| Entry point | `LevelIO/LevelJson.cs` |
| Reading | `LevelIO/Reading/` (`LevelJsonReader`, `JsonModifierReader`, `JsonRead`) |
| Writing | `LevelIO/Writing/` (`LevelJsonWriter`, `JsonModifierWriter`) |
| Direction and axis names | `LevelIO/DirectionNames.cs`, `LevelIO/AxisNames.cs` |
| Editor field editing | `Editor/Model/ModifierFields.cs`, `FieldKind.cs`, `Editor/Inspector/SelectionPanel.cs` |
| Tests | `Tests/LevelIO/`, `Tests/EditMode/Level/LevelValidatorTests.cs`, `Tests/EditMode/Modifiers/ModifierCatalogTests.cs` |
