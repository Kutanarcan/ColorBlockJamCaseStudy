<div align="center">

# 🏗️ ProductionV1

**The production plan for the Color Block Jam case study — logic first**

![Mode](https://img.shields.io/badge/Mode-🏗️_Production-1f6feb)
![Layer](https://img.shields.io/badge/Layer-Logic_+_Level_Data-8250df)
![Phases](https://img.shields.io/badge/Phases-5/10_done-1f6feb)
![Decisions](https://img.shields.io/badge/Brainstorm-settled-2ea043)

<sub>[README](../../README.md) · [FINDINGS (prototype)](../prototype/FINDINGS.md) · [PrototypeV1](../prototype/PrototypeV1.md)</sub>

</div>

> [!IMPORTANT]
> **Production V1 goal:** The whole game is playable and verified **in the logic layer alone**, with tests, before any presentation work starts. That covers the grid, walls and doors, movement, exit, block modifiers, level data and the Level Editor.

| # | Section | What is in it |
|---|---|---|
| 1 | [📋 Scope](#-1-scope) | What V1 covers and what it leaves to later |
| 2 | [🎮 Game Rules](#-2-game-rules) | The rules the logic must implement |
| 3 | [🧱 Grid & Entities](#-3-grid--entities) | One occupancy array; blocks, walls and doors are all entities |
| 4 | [🚶 Movement](#-4-movement) | One step, one rule |
| 5 | [🚪 Exit](#-5-exit) | The column scan rule |
| 6 | [🧩 Modifiers](#-6-modifiers) | Gate / Reactor / Board command, counters with filters |
| 7 | [🔁 Exit Resolution](#-7-exit-resolution) | What happens, in which order, after an exit |
| 8 | [🪆 Tangled Blocks](#-8-tangled-blocks) | Carrier and carried blocks |
| 9 | [⏱️ Game State](#-9-game-state) | Timer, win, fail, continue, restart |
| 10 | [💾 Level Data & IO](#-10-level-data--io) | DTO, polymorphic JSON, Addressables readiness, palette |
| 11 | [🛠️ Level Editor](#-11-level-editor) | Editor-only, data-oriented, validation |
| 12 | [📦 Assemblies & Tests](#-12-assemblies--tests) | Assembly boundaries and test layout |
| 13 | [🔬 From FINDINGS](#-13-from-findings) | How prototype costs are answered here |
| 14 | [🪜 Phase Plan](#-14-phase-plan) | Phases 0–9 with sub-steps and their tests |
| 15 | [🧭 Ready For, Not Built](#-15-ready-for-not-built) | Extension points kept open on purpose |
| 16 | [📝 Decision Log](#-16-decision-log) | Every brainstorm decision in one table |

---

## 📋 1. Scope
- **Logic first.** Every rule below is implemented in `Game.Core` and proven by EditMode tests.
- **Presentation comes after.** Visuals, `BlockDrawRule`, drag feel, exit animation and audio are not part of V1's logic work. Wherever a decision here affects visuals, that is handled in the presentation phase.
- **Level pipeline is part of V1.** `LevelData` (JSON), `Game.LevelIO` and the Level Editor are designed together with the logic, so the logic loads exactly what the editor saves.
- **Prototype code is not carried over.** Only the knowledge in [`FINDINGS.md`](../prototype/FINDINGS.md) is.

## 🎮 2. Game Rules
- **Grid:** a W × H grid painted in the Level Editor. Walls and doors are painted cells; there is no separate board border.
- **Blocks:** polyomino shapes, one color each. A block slides cell by cell into empty cells.
- **Walls:** static cells. They block movement. They can sit anywhere, including inside the grid.
- **Doors:** static cells with a **color** and a **direction**. A door accepts a block of its color moving in its direction. From any other side, a door behaves like a wall.
- **Exit:** a block pushed into doors of its color, across its full width, leaves the grid.
- **Modifiers:** blocks may carry features (Ice, Key/Lock, Arrow, Rope/Scissors, Tangled). Features stack.
- **Win:** every block has exited, including blocks that started inside another block.
- **Fail:** the timer runs out. A continue can add time and resume the game.
- **Restart:** the level is rebuilt from its `LevelData`. There is no undo.

---

## 🧱 3. Grid & Entities

### One occupancy array
```
occupancy[W * H]     // index = y * W + x, value = entity id, -1 = empty
```
- There is **no edge layer.** Walls and doors take up cells, so a single lookup answers "what is in this cell?".
- The grid is exactly the painted W × H. There is **no +2 border ring**. The outermost cells are walls or doors (an Editor rule, see §11), so a move can never leave the array and the logic needs **no bounds check**.
- Unreachable holes inside the walls may stay empty. Nothing can reach them.

### Entities
Every occupant of the grid is an entity with the same base:

| Kind | Shape + position | Moves | Extra data | Modifiers |
|---|---|---|---|---|
| **Block** | ✔ | ✔ | `colorId` | ✔ used today |
| **Wall** | ✔ | ✘ | — | ✔ supported, empty today |
| **Door** | ✔ | ✘ | `colorId`, `direction` | ✔ supported, empty today |

- **Shape** = cells relative to an anchor, immutable. **Position** = the anchor's cell.
- A move changes only the position: clear the old cells, write the new ones.
- An exit clears the entity's cells as one batch.
- **Grouping is the designer's choice.** A wall entity or a door entity may span many cells. A wall or door can carry modifiers later (for example a locked door), so its unit is set by design, not merged automatically.
- `Direction` is one shared type, used for movement steps, door directions and the Arrow modifier.

---

## 🚶 4. Movement
- **A step is valid** when every target cell of the block is empty or belongs to the block itself, **and** no gate on the block vetoes the direction (§6).
- Walls, doors and other blocks are all simply "occupied". There is no separate wall or edge check.
- Movement is always one cell on one axis. The logic has **no diagonal step**. Any half-cell lean belongs to presentation.
- A blocked step triggers the exit check (§5).

## 🚪 5. Exit

> [!TIP]
> **In one line:** For each column the block covers in the push direction, scan forward from its front cell; the first occupied cell must be a door of the block's color facing that direction.

**Rule, for a blocked step in direction `d`:**
1. For every line along `d` that the block covers, start at the block's front cell on that line.
2. Scan forward. Empty cells are skipped: this covers recesses such as a U shape open toward the door.
3. The first occupied cell must be a **door** with `direction == d` and the **block's color**. Anything else rejects the exit: another block, a wall, a wrong color or a wrong direction.
4. All lines pass → the block exits.

- **No "pressed" flag is needed.** The step was blocked, so at least one line touches something directly. If every line's first occupied cell is a matching door, what it touches is a door.
- **Width rule comes for free.** A block wider than the door hits a wall on at least one line.
- **The U-recess bug is closed.** A block sitting inside the recess is the first occupied cell on its line, so the exit is rejected.
- **Door entities do not matter, only cells.** A block may exit across two separate door entities as long as both have the same color and direction.
- **Inner doors** work the same way: a door with direction `Up` accepts blocks arriving from below.

```
Blue U open toward the door, pushed down (d = Down). Door cells on row 0, direction Down:

y2   B B B          column 1: front (1,1) → (1,0) is a blue Down door ✔
y1   B . B          column 2: front (2,2) → (2,1) empty, skip → (2,0) is a blue Down door ✔
y0   D D D          column 3: front (3,1) → (3,0) is a blue Down door ✔
     1 2 3          → exits. Put a red block at (2,1): column 2 hits it first → no exit.
```

---

## 🧩 6. Modifiers

### The contract
An entity is a base plus a list of modifiers. A modifier takes part in one or more of three roles:

| Role | Question / job | Examples |
|---|---|---|
| **Gate** | Vetoes an action: "may this block move in `d`?" | Ice (count > 0), Lock, Rope, Arrow, Carried |
| **Reactor** | Reacts to an exit event and updates its own state | Ice, Lock and Rope counters |
| **Board command** | Changes the grid through a narrow API | Carrier → `ActivateBlock(inner)` |

- **Veto, not capability.** A block can move unless some gate says no. Stacking is therefore an automatic AND, and adding a modifier never changes the block or the other modifiers.
- **Board commands go through a narrow API.** A reactor never touches the board directly. Today the API has one command, `ActivateBlock`. A future mechanic that reshapes or spawns entities adds a command; gates, reactors and the exit flow stay as they are.
- **Interfaces stay small.** Each role is its own interface; a modifier implements only the roles it needs.

### Counters with filters
Most locks are the same thing: **a counter, and a filter that says how much an exit counts.**

| Modifier | Counter | Filter → amount per exit |
|---|---|---|
| **Ice** `{ count }` | `count` | any exit → 1 |
| **Lock** `{ keyId, count = 1 }` | keys required | number of matching keys on the exiting block |
| **Rope** `{ colorId }` | 1 | number of matching scissors on the exiting block |

- The counter is the gate: it vetoes movement while `count > 0`.
- **Broadcast is natural.** A block with `Key { red, count: 2 }` exits → every red Lock drops by 2 → they all open. A Lock with `count: 2` needs two keys.
- **New lock types are usually new filters.** For example, a future *Colored Ice* is Ice with a color filter.

### Passive modifiers
Data only. Filters and gates read them.

| Modifier | Data | Read by |
|---|---|---|
| **Key** | `{ keyId, count }` | Lock filter |
| **Scissors** | `{ colorId }` | Rope filter (a rope is cut when the scissors block exits) |
| **Arrow** | `{ direction }` | Gate: the block moves only in that one direction |

- `keyId` and `colorId` are separate id spaces. How a key is colored on screen is a presentation mapping.

### Stacking
- **Parallel:** independent locks on one block progress at the same time. A roped block with an Ice counter counts down and gets its ropes cut in any order; it moves when all are resolved.
- **Ordered:** a lock can hide another. Ice on a Tangled outer must open before the pair can move; only then can the outer exit and release the inner.
- Both come from the same model: a gate belongs to the entity it sits on. The data and editor complexity this brings is accepted.

---

## 🔁 7. Exit Resolution
An **exit is the only trigger.** It can lower counters, open locks and release tangled blocks. Nothing else does.

```
TryMove(id, d) blocked → exit check passes
  1. clear the block's cells, mark it exited
  2. build ExitEvent { id, colorId, modifiers of the exiting block }
  3. reactors of the exiting entity itself      (e.g. Carrier → ActivateBlock)
  4. reactors of every other non-exited entity  (active AND carried), fixed order
```

- **Single pass, fixed order.** No reaction produces another exit, so there are no chains or loops.
- **Carried entities react too.** An Ice on a carried inner block counts every exit while it is carried.
- **Resolver + filters, no event bus.** The resolver walks the reactors explicitly; there are no C# events or observer layers.

---

## 🪆 8. Tangled Blocks
- **Two blocks, two ids.** The outer block carries `Carrier { innerId }`. The inner block is a full block in the **carried** state.
- **Carried means:**
  - not written to the occupancy array,
  - positioned relative to the outer block,
  - movement and exit are closed by the `Carried` gate.
- **Moving:** the pair moves with the outer shape. The inner shape is a **subset** of the outer one (the same size at most, smaller allowed), so a single collision check covers both.
- **Exit:** the outer block exits through its own color. Its Carrier reacts to its own exit (§7 step 3) and calls `ActivateBlock(inner)`. The inner block is written to the occupancy at the outer block's last position; those cells were just freed, so nothing collides.
- **Depth:** two levels are the baseline, three must work. Data is **flat**: every block is one entry in the list, and nesting is expressed only by `Carrier { innerId }`. A three-level stack is an inner block that has its own Carrier.

---

## ⏱️ 9. Game State
- **States:** `Playing → Won` or `Playing → Failed`.
- **Timer:** the initial time comes from `LevelData`. Core ticks it through `ITickable.Tick(dt)`. Time is never read from Unity.
- **Win:** every block has exited (carried blocks included).
- **Fail:** time reaches zero.
- **Continue:** Core allows `Failed → Playing` with added time (e.g. `AddTime(seconds)`). When and how often it is offered is Runtime's decision, not level data.
- **Restart:** state is rebuilt from `LevelData`. Definition (level data, immutable) and state (positions, counters, timer) are kept apart, so no snapshot is needed.

---

## 💾 10. Level Data & IO
- **`LevelData` DTOs live in `Game.Core`**, plain C#, no serializer attribute dependencies.
- **JSON lives in `Game.LevelIO`** (Core + Newtonsoft), shared by Runtime and the Level Editor.
- **Polymorphic modifiers:** a `type` discriminator plus a registry that maps it to a DTO. **No `TypeNameHandling`**: it is unsafe and breaks when a class is renamed.
- **Doors** are stored as entities with their own cells, color and direction.
- **Shape only, not final:**

```json
{
  "schemaVersion": 1,
  "width": 8, "height": 10,
  "timeLimit": 90,
  "walls":  [ { "id": 0, "cells": [[0,0],[1,0],[2,0]] } ],
  "doors":  [ { "id": 1, "colorId": 2, "direction": "Down", "cells": [[3,0],[4,0]] } ],
  "blocks": [
    { "id": 5, "colorId": 2, "cells": [[3,1],[4,1]],
      "modifiers": [ { "type": "ice", "count": 3 } ] },
    { "id": 6, "colorId": 1, "cells": [[3,1]],
      "modifiers": [ { "type": "carried" } ] }
  ]
}
```

### Ready for Addressables (not built now)
- Levels are reached by **key** (address), never by path.
- Loading sits behind `ILevelSource`: a `TextAsset` today, Addressables or a download later.
- `schemaVersion` on every file. An **unknown modifier type rejects the level**, so an old client that downloads a newer level fails cleanly.

### Palette
- Data and Core see only `colorId` (int) and only compare for equality.
- Real colors live in a palette asset in Runtime, and can move to remote config later without a build.

### Load-time validation (Core)
- Supported `schemaVersion`, no unknown modifier types.
- Every entity cell is inside the grid; no two active entities overlap.
- `Carrier`: the inner shape is a subset of the outer shape, and the carry chain has no cycles.
- **The border rule is not checked here.** It lives only in the Editor, which refuses to save a level that breaks it.

---

## 🛠️ 11. Level Editor
- **Editor-only**, under an `Editor/` folder, in its own assembly. It does not depend on Runtime.
- **Visual JSON editing:** open, create, save and overwrite levels.
- **Fast to use is the priority.** Its working model is **data-oriented**: flat arrays, id → cell indices and lookups by type or color, so queries stay fast. It converts to and from `LevelData` only on load and save.
- **Nothing leaks into Core.** Editor code and editor tests stay in their own assemblies.

### Validation rules
Saving is blocked when a rule fails.

| Rule | Where |
|---|---|
| Edge cells of the grid are only `Wall` or `Door` (never empty, never a block) | Editor |
| A door on the edge points outward; a **corner cell cannot be a door** | Editor |
| A door entity is a straight line perpendicular to its direction | Editor |
| Every `colorId` exists in the palette | Editor |
| Every block shape is connected | Editor |
| Every block color has a door wide enough | Editor |
| Every Lock `keyId` has a Key, every Rope color has Scissors | Editor |
| Carried inner ⊆ outer, no carry cycles | Editor + Core (load time) |

- More rules are added when the editor phase starts. A BFS solver is a candidate for later, not V1.

---

## 📦 12. Assemblies & Tests

| Assembly | Contents | References |
|---|---|---|
| `Game.Core` | Logic, runtime model, `LevelData` DTOs. **No UnityEngine.** | — |
| `Game.LevelIO` | JSON serialization of `LevelData` | Core, Newtonsoft |
| `Game.Runtime` | Presentation, `ILevelSource` implementation, palette | Core, LevelIO |
| `Game.LevelEditor` (Editor-only) | DOD model, queries, validation, window | Core, LevelIO |
| `Game.Tests.EditMode` (per tested assembly) | EditMode tests | The assembly under test |
| `Game.Tests.PlayMode` | Kept separate, only when the Unity runtime is required | Runtime |

- `Game.Prototype` and production assemblies never reference each other.
- Hand-written fakes over mocks (`ITimeProvider` and similar).
- Every phase ends by naming the test that turns green.
- `com.unity.nuget.newtonsoft-json` is installed.

---

## 🔬 13. From FINDINGS
How the prototype's costs and edge cases are answered by this design:

| FINDINGS cost | Production answer |
|---|---|
| Exit passes through a block in a U recess | Column scan: the first occupied cell must be the door (§5) |
| Recess cells must also face the door | Still true: every covered line must reach a matching door, empty cells in between are fine |
| Diagonal `CanPlace` skips the edge check | No diagonal step in logic; no edges at all (§4) |
| Dormant edge check | Gone: walls are occupied cells |
| `DoorData.length` not bounds-checked | Doors are entities with explicit cells; load-time validation checks bounds |
| `blocks[id].cells` changed in place | Shape is immutable; only the position changes |
| Logic finishes before the visual | Presentation concern; the logic exit is immediate |
| Exit allocates per call | Logic hot paths follow the allocation rules; visuals handled in presentation |

---

## 🪜 14. Phase Plan
One phase per answer, following the production process. Every sub-step ends with a green EditMode test. **Files touched** for each phase are decided when that phase starts.

**Order:** skeleton → grid → core game (move, exit, win/fail) → modifiers → data & tools (validation, IO, editor). Presentation comes after V1.

| # | Phase | Sub-steps | Done when | Status |
|---|---|---|---|---|
| 0 | Skeleton | 0.1 `Game.Core` (no UnityEngine) + `Game.Tests.EditMode` asmdefs · 0.2 first test | `CoreAssembly_Compiles_WithoutUnityEngine` | ✅ |
| 1 | Grid & Entities | 1.1 `Direction` + cell ↔ index math · 1.2 entity base (shape + position; Block / Wall / Door) · 1.3 occupancy fill and cell query · 1.4 `LevelData` DTO + Core builder · 1.5 ASCII map → `LevelData` test helper | `Builder_PlacesEntities_IntoOccupancy` | ✅ |
| 2 | Movement | 2.1 step into empty cells · 2.2 blocked by wall / door / block, own cells free · 2.3 multi-cell shapes (bar, L, U) · 2.4 `TryMove` result `Moved / Blocked` | `TryMove_StopsAt_OccupiedCell` | ✅ |
| 3 | Exit | 3.1 straight door exit, batch clear · 3.2 rejects: wrong color, wrong direction, too wide · 3.3 recess: open U passes, U with a block inside fails · 3.4 several door entities, inner door direction | `UShape_WithBlockInRecess_DoesNotExit` | ✅ |
| 4 | Game State | 4.1 win · 4.2 timer driven by `Tick(dt)` → fail · 4.3 continue `AddTime` · 4.4 restart from definition | `Restart_RestoresInitialState` | ✅ |
| 5 | Modifier Framework | 5.1 Gate contract + Arrow · 5.2 Reactor contract + `ExitResolver` + Ice · 5.3 amount filters + Key/Lock · 5.4 Rope/Scissors · 5.5 parallel stacking (Rope + Ice) | `RopeAndIce_BothMustResolve_BeforeMove` | ⏳ |
| 6 | Tangled | 6.1 carried state + `Carried` gate, pair moves with the outer shape · 6.2 `ActivateBlock` board command, exit order · 6.3 ordered stacking (Ice on outer / inner), carried reactors count · 6.4 three-level chain | `OuterExit_ActivatesInner_AtLastPosition` | ⏳ |
| 7 | Load-time Validation | 7.1 bounds and overlap · 7.2 carrier subset, no cycles · 7.3 unsupported `schemaVersion`, unknown modifier type | `Load_Rejects_CarrierCycle` | ⏳ |
| 8 | LevelIO | 8.1 `Game.LevelIO` asmdef + tests · 8.2 discriminator registry, polymorphic modifiers · 8.3 round-trip, unknown type rejected · 8.4 `ILevelSource` contract (key-based) | `RoundTrip_PreservesAllModifiers` | ⏳ |
| 9 | Level Editor | 9.1 `Game.LevelEditor` asmdef + DOD model, `LevelData` ↔ model · 9.2 validation rules · 9.3 window: painting, entity grouping · 9.4 save / load / overwrite | `EditorModel_RoundTrip_EqualsLevelData` + rule tests | ⏳ |

- **ASCII test helper (1.5) is test-only.** It lives in the test assembly and produces a plain `LevelData`. Core, LevelIO, the Editor and Runtime never see it. JSON stays the only level format.
- **After V1 (headline only):** Presentation: `GameInstaller`, palette, `BlockDrawRule`, drag, exit visual, timer / continue UI.

---

## 🧭 15. Ready For, Not Built
- **Addressables / downloadable levels:** `ILevelSource`, keys, `schemaVersion`.
- **Modifiers on walls and doors** (e.g. a locked door): entities already carry a modifier list.
- **Three-level tangled stacks:** flat data with `Carrier { innerId }`.
- **Colored Ice and other filtered counters:** new filter, same counter.
- **New board-changing mechanics:** new Board command, same contract.
- **Remote palette:** colors live outside data and Core.
- **Level solver:** later, on top of the editor's data model.

---

## 📝 16. Decision Log

| # | Topic | Decision |
|---|---|---|
| D1 | Grid | 1D `occupancy` of entity ids; no edge layer |
| D2 | Walls & doors | Occupied cells, may sit inside the grid; doors have color and direction |
| D3 | Border | No border concept or +2 ring; edge cells must be Wall/Door, enforced by the Editor only |
| D4 | Holes | Unreachable empty cells are allowed |
| D5 | Inner door | Accepts blocks moving in its direction (`Up` accepts from below) |
| D6 | Exit | Column scan to the first occupied cell; must be a matching door |
| D7 | Multiple door entities | A block may exit across several door entities of the same color and direction |
| D8 | Block model | Entity base + modifier list |
| D9 | Modifier contract | Gate (veto), Reactor (exit → amount → counter), Board command (narrow API); revisited if the code asks for it |
| D10 | Exit flow | Resolver + filters; exit is the only trigger; single pass |
| D11 | Exit order | Exiting entity's own reactors first, then all other non-exited entities |
| D12 | Carried reactors | Run while carried |
| D13 | Stacking | Parallel and ordered, both supported; data/editor complexity accepted |
| D14 | Ice | Any exit counts; immobile until 0; colored variant must be possible |
| D15 | Key/Lock | Separate `keyId`; `Key { keyId, count }`; matching locks all drop by the key count |
| D16 | Rope/Scissors | A rope is cut when a block with matching scissors exits |
| D17 | Tangled | Outer exits first, then the inner is activated; inner ⊆ outer; flat data; 2 levels baseline, 3 supported |
| D18 | Arrow | A modifier; one direction only |
| D19 | Wall/door grouping | Designer-defined entities |
| D20 | Game state | Timer from `LevelData`; fail on timeout; continue adds time (Runtime decides); restart from `LevelData`; no undo |
| D21 | JSON | Polymorphic with a `type` discriminator, Newtonsoft, in `Game.LevelIO` |
| D22 | Addressables | Prepared (keys, `ILevelSource`, `schemaVersion`), not built |
| D23 | Palette | `colorId` in data and Core; colors in Runtime, remote-config ready |
| D24 | Validation | Load-time structural rules in Core; design and border rules in the Editor |
| D25 | Direction | One shared `Direction` type |
| D26 | Level Editor | Editor-only assembly, data-oriented, independent of Runtime |
| D27 | Phase order | Validation before IO; Level Editor last |
| D28 | ASCII test helper | Test-only; produces `LevelData`; never a level format |
| D29 | Timer input | Time enters Core only through `ITickable.Tick(dt)`; no `ITimeProvider`, tests pass `dt` directly |
