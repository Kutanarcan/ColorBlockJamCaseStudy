<div align="center">

# 🔌 Extending

**How to add a mechanic: the recipe, a worked example, and the places where the design asks for an edit; how to add a service or saved data (V2)**

![Criterion](https://img.shields.io/badge/New_mechanic-new_files_only-2ea043)
![Proof](https://img.shields.io/badge/Proof-Turn_Based_Arrow-1f6feb)

<sub>[README](../../README.md) · [ProductionV1](ProductionV1.md) · [Level Format](LevelFormat.md) · [Mechanics Reference](ColorBlockJamMechanics.md)</sub>

</div>

> [!IMPORTANT]
> **The criterion:** adding a mechanic changes **no existing file**. The only accepted growth is one line in `ModifierCatalog.Default()` per modifier (D48), or a documented edit point (§4). If a mechanic seems to need anything else, stop: either it fits an edit point, or the design needs a decision first.

| # | Section | What is in it |
|---|---|---|
| 1 | [Find the mechanic](#1-find-the-mechanic) | Which seam or edit point it uses |
| 2 | [Recipe: a new modifier](#2-recipe-a-new-modifier) | Step by step, from parts to tests |
| 3 | [Worked example: Turn Based Arrow](#3-worked-example-turn-based-arrow) | The Phase 13 proof, file by file |
| 4 | [Edit points](#4-edit-points) | Mechanics that need a known change, and where it stays |
| 5 | [Before merging](#5-before-merging) | The checklist |
| 6 | [Recipe: a new service or saved data (V2)](#6-recipe-a-new-service-or-saved-data-v2) | Cross-cutting services in the game and the level test; a new save section |

---

## 1. Find the mechanic
1. Look the mechanic up in the [Mechanics Reference](ColorBlockJamMechanics.md) and note its **trigger** and the **state it holds**.
2. Find it in the research → seam map ([ProductionV1 §8](ProductionV1.md#-8-extensibility)).
3. **It maps to seams only** → follow §2. Nothing existing changes.
4. **It needs an edit point** → read §4 first. The edit is made once, in the listed files, and is a decision for the Decision Log.

Rules that always hold:
- **Entity kinds are closed** (D38). A crate is a block with modifiers; a laser emitter is a wall with modifiers.
- **Shapes never change.** Cells that keep blocking are per-cell state on a fixed shape; cells that must free up are separate single-cell entities (dormant edit point).
- **An occupied cell always blocks** (D44). Nothing makes an occupied cell passable.

## 2. Recipe: a new modifier

### 2.1 Pick the parts
A modifier only **declares**; central rules decide (D30). Build it from these parts, nothing else:

| Behavior | Part | Example |
|---|---|---|
| Turn a capability off while present | `ISuspender { Capability Suspends }`, listing each capability (D40) | Ice: `Move \| Exit` |
| Limit move directions | `IMoveConstraint.Allows(direction)` | Arrow |
| React to an exit | `IExitListener.OnExited(owner, exited, commands)` | Ice wears down |
| React to the end of a player move | `IMoveListener` | Dynamite (fail) |
| React to time | `ITickListener` | Time Capsule |
| Count down, then go away | `Durability` + `WearDown(owner, this, amount, commands)` from a listener | Ice, Turn Based Arrow |
| Change the effective color | `IColorSource` (read only through `Colors.Of`) | Color-Switching Block |
| Not count toward the win | `IWinExempt` | Moving Blocks |
| Change the board or session | `ILevelCommands`: `AddModifier`, `RemoveModifier`, `MoveEntity`, `Fail`, `AddTime` | Curtain, Bomb |

- Listeners never touch the board or the session directly; they combine command primitives (D32).
- Filtering (which exits count) lives in the modifier's own listener (D45).

### 2.2 Write the files
One folder per mechanic under `Core/Modifiers/` (D31), runtime type and DTO together:
1. **Runtime modifier:** implements the parts from §2.1. Holds its state (direction, `Durability`).
2. **DTO** (`ModifierData`): follow [Level Format §4.1](LevelFormat.md#41-adding-a-modifier) exactly: `TypeName`, `ToModifier`, `Write` / `Read` with the same keys, `Validate` for its limits.
3. **Catalog:** one line in `ModifierCatalog.Default()`.

A new field kind (float, bool, cell, list) is not part of this recipe: it follows [Level Format §4.2](LevelFormat.md#42-adding-a-field-kind-float-bool-cell-list), which also teaches the Level Editor to draw it.

### 2.3 What you get without touching anything else
- **LevelIO** saves and loads it through the DTO's own `Write` / `Read`; an unknown type is still rejected (D47).
- **Load-time validation** runs its `Validate` (`LevelSession.TryCreate`).
- **Level Editor** lists it from the catalog and edits its fields through `ModifierFields` (D52). No drawer.
- **Events** reach it: the dispatcher calls every listener on the board, in id order (§7 of the plan).

### 2.4 Tests
- **Logic:** EditMode tests through `LevelSession` (an ASCII level is enough): the capability before and after, and stacking with an existing modifier.
- **Data:** the round-trip and validation tests listed in [Level Format §4.1](LevelFormat.md#41-adding-a-modifier).
- **Look:** add the presenter to the expectations in `ModifierPresentersTests`.

### 2.5 Its look (V2)
A modifier without a look still plays; it is simply not drawn. To give it one ([ProductionV2 D104](ProductionV2.md#-14-decision-log)), in `Runtime/Modifiers/<Mechanic>/`:
1. **View** (`MonoBehaviour`, dumb): setters in Unity terms only, no `Game.Core` reference. Its prefab is a kit prefab or a small prefab of its own.
2. **Presenter** (pure C#, `IModifierPresenter`): `Accepts` checks the modifier's type; `Show` reads the modifier and the block and calls the view.
3. **Register:** one line in `ModifierPresenters.Default()` and one view field in `ModifierViews`, then assign the prefab in the asset.

These two lines are the presentation's accepted growth, like the catalog line in §2.2.

## 3. Worked example: Turn Based Arrow
The Phase 13 proof (D39, D60). It lives in its own test assembly, `Game.Tests.Additivity`, which references only **Game.Core** and **Game.LevelIO** and sees only their public API. Nothing in Core, LevelIO or the Level Editor changed for it.

**Mechanic:** starts locked to one direction; after a given number of exits it moves freely in every direction.

| File | What it is |
|---|---|
| `Tests/Additivity/TurnBasedArrow.cs` | `IMoveConstraint` (only its direction) + `IExitListener` wearing a `Durability` down by 1 per exit. Used up → removed → a plain block |
| `Tests/Additivity/TurnBasedArrowData.cs` | DTO `"turn-based-arrow"`: `direction`, `count`; `Validate` requires `count > 0` |
| `Tests/Additivity/TurnBasedArrowTests.cs` | The proof; the catalog gets it with `ModifierCatalog.Default().Add(() => new TurnBasedArrowData())` |

| Test | Proves |
|---|---|
| `TurnBasedArrow_AddedWithoutCoreChanges` | Saved to JSON, loaded, played: locked, still locked after one exit, free after two |
| `RoundTrip_KeepsDirectionAndCount` | LevelIO keeps its fields without a LevelIO change |
| `WithoutItsCatalogLine_ALevelUsingItIsRejected` | The catalog is the only registration |
| `ItsOwnValueCheck_RejectsTheLevelAtLoad` | Its `Validate` runs at load time |
| `StacksWithIce_FrozenFirst_ThenLocked_ThenFree` | Parallel stacking with an existing modifier (D13) |

**To ship it in the game:** move the two source files to `Core/Modifiers/<Mechanic>/`, change their namespace to `Game.Core`, add the catalog line to `Default()`, and move the tests next to the existing ones (round-trip in `LevelJsonTests`, logic in EditMode). That is the whole change.

## 4. Edit points
Not built in V1. Each needs a change in a known place, so it is a choice, not a surprise. Full table: [ProductionV1 §8](ProductionV1.md#documented-edit-points).

| Edit point | Opened by | Change stays in |
|---|---|---|
| Floor layer | Colorful Path, Button, Laser receiver | `Grid`, `Board`, builder, `Board.CanPlace` |
| Movement groups | Combined, Magnet | `BlockMover`, `Board.CanPlace`, `Board.MoveEntity` |
| A new primitive | A fourth event, or a new command | Dispatcher / command API |
| Wall / door modifiers | Door toggle, Iced Door, Locked Door, Colorful Door, Size-Changing Door, Jumping Single Door, Star | `WallData`, `DoorData`, builder; an `Accept` capability in `ExitRule` ([Level Format §4.6](LevelFormat.md#46-wall-or-door-modifiers-documented-edit-point-d41)) |
| Dormant entities | Crate, Hidden, Tangled, Barrier, Ivy, ordered stacking | `Entity` / `Board`, `BlockData` + builder, command API (`Activate` / `Deactivate`) |
| Listener inputs | Color Swapping Block, Laser Door, Locked Door… | Listener signatures gain a board view or the exited-through doors |

- Opening an edit point is done **once**; every later mechanic that needs it goes back to §2.
- Record it in the Decision Log, and move its row here from "not built" to built.

## 5. Before merging
- [ ] The mechanic maps to seams (§1), or the edit point it opens is recorded as a decision.
- [ ] `git diff --stat` shows **only new files**, plus the `Default()` line(s) (catalog, and presenters if it has a look), the `ModifierViews` field, or the edit point's files.
- [ ] [Level Format §4.1](LevelFormat.md#41-adding-a-modifier) checklist done; [§6 review checklist](LevelFormat.md#6-review-checklist) passes.
- [ ] Logic tests cover: before, after, and stacking with an existing modifier.
- [ ] The modifier shows up in the Level Editor's "Add" list and its fields can be edited.

---

## 6. Recipe: a new service or saved data (V2)
The game and the Level Editor's test are two starts that share the game's code (ProductionV2 D131). Rules: `.claude/rules/production/architecture.md` § Start services, § Saved data.

```
RootScope (Runtime) ── RootInstaller: shared, runs the same in the game and in a test
   ├─ RootLifetimeScope       (Bootstrap)  ── LiveServicesInstaller       real implementations
   └─ LevelTestLifetimeScope  (LevelTest)  ── LevelTestServicesInstaller  stand-ins (Game.LevelTest, Editor only)
```

### 6.1 Decide where it goes
| The service… | Goes in | Example |
|---|---|---|
| Behaves the same in a test, has no outside effect | `RootInstaller` | Asset loader, scene loader, config, `Progression` / `Settings` logic |
| Keeps data, talks to the outside, or must differ in a test | Both start installers | Save store, level choice, analytics, live events, ads, purchases, cloud save |
| Unsure | Both start installers | A shared registration runs for real in every level test |

### 6.2 A cross-cutting service (e.g. analytics)
1. **Contract** in live code, where its users live (Meta if pure C#, else Infrastructure): `IAnalytics`, small (ISP).
2. **Real implementation** in Infrastructure; register it in `LiveServicesInstaller`.
3. **Stand-in** in `Game.LevelTest`: `NullAnalytics` (does nothing) or an in-memory one when the test should still see its effect; register it in `LevelTestServicesInstaller`.
4. **Users** take the contract by constructor or `[Inject]`. In a child scope, it comes from the root by injection; never `new` it in `GameplayEntry` or a view.
5. **Tests:**
   - Unit tests use a hand-written fake in the test assembly (not the `Game.LevelTest` stand-in).
   - Add the contract to the list in `StartServices_RegisterTheSameContracts` (from M1): it fails while only one start installer registers it.

Forgetting step 3 is loud, not silent: the level test fails to resolve the contract when it starts. Registering in `RootInstaller` instead is silent: the test then runs the real service. That is why "unsure" means both start installers.

### 6.3 Saved data for a feature (e.g. boosters)
1. A data class beside the feature: `[Serializable] BoosterData { public int version = 1; … }`, public fields (the store writes fields), defaults as a fresh player has them.
2. A `SaveKey` constant on the feature (`"boosters"`); the key is frozen once shipped.
3. The feature takes `ISaveStore`, reads its section on first use (`Load<BoosterData>(SaveKey)`), writes only its section on change.
4. Nothing else changes: `JsonSaveStore` writes `Save/boosters.json`; a level test starts the section fresh in memory. Only if the test should start from the player's data, copy the section in `LevelTestServicesInstaller`, as it does for `settings`.
5. Tests: the Meta fake store keeps sections as JSON text, so "survives a restart" reads a copy.

### 6.4 Before merging
- [ ] Live code (Core, Meta, Infrastructure, Runtime) names no test type and checks no mode.
- [ ] A new start service is registered in **both** start installers and listed in `StartServices_RegisterTheSameContracts`.
- [ ] No cross-cutting service is created with `new` inside a child scope or its entry point.
- [ ] A new save section is its own data class and key; no field was added to another feature's data.
