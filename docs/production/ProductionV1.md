<div align="center">

# 🏗️ ProductionV1

**The production plan for the Color Block Jam case study — logic first, built to be extended**

![Mode](https://img.shields.io/badge/Mode-🏗️_Production-1f6feb)
![Layer](https://img.shields.io/badge/Layer-Logic_+_Level_Data-8250df)
![Phases](https://img.shields.io/badge/Phases-9/13_done-1f6feb)
![Mechanics](https://img.shields.io/badge/Mechanics-Block_·_Arrow_·_Ice-2ea043)

<sub>[README](../../README.md) · [FINDINGS (prototype)](../prototype/FINDINGS.md) · [PrototypeV1](../prototype/PrototypeV1.md) · [Mechanics Reference](../ColorBlockJamMechanics.md)</sub>

</div>

> [!IMPORTANT]
> **Production V1 goal:** The whole game is playable and verified **in the logic layer alone**, with tests, before any presentation work starts.
> **Architecture goal:** Any mechanic from the [Mechanics Reference](../ColorBlockJamMechanics.md) can be added later **without changing an existing file**, only by adding new ones.

| # | Section | What is in it |
|---|---|---|
| 1 | [📋 Scope](#-1-scope) | What V1 builds, and what it only prepares for |
| 2 | [🎮 Game Rules](#-2-game-rules) | The rules the logic must implement |
| 3 | [🧱 Grid & Entities](#-3-grid--entities) | One occupancy array; blocks, walls and doors are all entities |
| 4 | [🚶 Movement](#-4-movement) | One step, one rule |
| 5 | [🚪 Exit](#-5-exit) | The column scan rule |
| 6 | [🧩 Modifiers](#-6-modifiers) | Capabilities, declarative parts, central rules |
| 7 | [📣 Events & Commands](#-7-events--commands) | Three events, listeners, command primitives |
| 8 | [🔌 Extensibility](#-8-extensibility) | The additive criterion, the seams and the documented edit points |
| 9 | [⏱️ Game State](#-9-game-state) | Timer, moves, win, fail, continue, restart |
| 10 | [💾 Level Data & IO](#-10-level-data--io) | DTO, polymorphic JSON, Addressables readiness, palette |
| 11 | [🛠️ Level Editor](#-11-level-editor) | Editor-only, data-oriented, validation, discovery |
| 12 | [📦 Assemblies & Tests](#-12-assemblies--tests) | Assembly boundaries and test layout |
| 13 | [🔬 From FINDINGS](#-13-from-findings) | How prototype costs are answered here |
| 14 | [🪜 Phase Plan](#-14-phase-plan) | Phases 0–13 with sub-steps and their tests |
| 15 | [🧭 Ready For, Not Built](#-15-ready-for-not-built) | What is prepared but not implemented |
| 16 | [📝 Decision Log](#-16-decision-log) | Every decision in one table |

---

## 📋 1. Scope
- **Mechanics built in V1:** the normal block, the **Arrow** modifier and the **Ice** modifier. Nothing else is implemented.
- **Mechanics prepared for:** everything in the [Mechanics Reference](../ColorBlockJamMechanics.md). The architecture opens the seams those mechanics need (§8); each seam is proven by a hand-written fake in the test assembly, never by a speculative real mechanic.
- **Logic first.** Every rule is implemented in `Game.Core` and proven by EditMode tests.
- **Presentation comes after.** Visuals, `BlockDrawRule`, drag feel, exit animation and audio are not part of V1's logic work.
- **Level pipeline is part of V1.** `LevelData` (JSON), `Game.LevelIO` and the Level Editor are designed together with the logic, so the logic loads exactly what the editor saves.
- **Prototype code is not carried over.** Only the knowledge in [`FINDINGS.md`](../prototype/FINDINGS.md) is.

## 🎮 2. Game Rules
- **Grid:** a W × H grid painted in the Level Editor. Walls and doors are painted cells; there is no separate board border.
- **Blocks:** polyomino shapes, one color each. A block slides cell by cell into empty cells.
- **Walls:** static cells. They block movement. They can sit anywhere, including inside the grid.
- **Doors:** static cells with a **color** and a **direction**. A door accepts a block of its color moving in its direction. From any other side, a door behaves like a wall.
- **Exit:** a block pushed into doors of its color, across its full width, leaves the grid.
- **Modifiers:** a block may carry **Arrow** (moves in one direction only) and **Ice** (cannot move or exit until a number of blocks have exited). They stack.
- **Win:** every block that must exit has exited.
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
| **Block** | ✔ | ✔ | `colorId` | ✔ |
| **Wall** | ✔ | ✘ | — | ✘ in V1 (edit point, §8) |
| **Door** | ✔ | ✘ | `colorId`, `direction` | ✘ in V1 (edit point, §8) |

- **Shape** = cells relative to an anchor, immutable. **Position** = the anchor's cell.
- A move changes only the position: clear the old cells, write the new ones.
- An exit clears the entity's cells as one batch.
- **Grouping is the designer's choice.** A wall or door entity may span many cells.
- **The three kinds are closed.** New mechanics never add an entity kind; every variation is a modifier on one of the three (a crate is a block with modifiers, a laser emitter is a wall with modifiers).
- **Shapes never change.** A mechanic that seems to resize part of an entity is expressed in one of two ways:
  - **The cell keeps blocking:** per-cell state on a fixed shape. A size-changing door's closed cell simply rejects the exit, like a wall.
  - **The cell must become free:** several single-cell entities that leave the occupancy one by one. Ivy works this way (dormant edit point, §8).
- **Dormant entities** (start outside the occupancy, activated later) are not built in V1; they are a documented edit point (§8).
- `Direction` is one shared type, used for movement steps, door directions and the Arrow modifier.

---

## 🚶 4. Movement
- **A step is valid** when the block still has the `Move` capability in that direction (§6) **and** every target cell is empty or belongs to the block itself.
- Walls, doors and other blocks are all simply "occupied". There is no separate wall or edge check.
- Movement is always one cell on one axis. The logic has **no diagonal step**. Any half-cell lean belongs to presentation.
- A blocked step triggers the exit check (§5).

## 🚪 5. Exit

> [!TIP]
> **In one line:** For each column the block covers in the push direction, scan forward from its front cell; the first occupied cell must be a door of the block's color facing that direction.

**Rule, for a blocked step in direction `d`:**
1. The block must still have the `Exit` capability (§6).
2. For every line along `d` that the block covers, start at the block's front cell on that line.
3. Scan forward. Empty cells are skipped: this covers recesses such as a U shape open toward the door.
4. The first occupied cell must be a **door** with `direction == d` and the **block's color**. Anything else rejects the exit: another block, a wall, a wrong color or a wrong direction.
5. All lines pass → the block exits.

- **No "pressed" flag is needed.** The step was blocked, so at least one line touches something directly. If every line's first occupied cell is a matching door, what it touches is a door.
- **Width rule comes for free.** A block wider than the door hits a wall on at least one line.
- **The U-recess bug is closed.** A block sitting inside the recess is the first occupied cell on its line, so the exit is rejected.
- **Door entities do not matter, only cells.** A block may exit across two separate door entities as long as both have the same color and direction.
- **Inner doors** work the same way: a door with direction `Up` accepts blocks arriving from below.
- **From Phase 8:** colors are read through one place (effective color, `Colors.Of`).

```
Blue U open toward the door, pushed down (d = Down). Door cells on row 0, direction Down:

y2   B B B          column 1: front (1,1) → (1,0) is a blue Down door ✔
y1   B . B          column 2: front (2,2) → (2,1) empty, skip → (2,0) is a blue Down door ✔
y0   D D D          column 3: front (3,1) → (3,0) is a blue Down door ✔
     1 2 3          → exits. Put a red block at (2,1): column 2 hits it first → no exit.
```

---

## 🧩 6. Modifiers

### The model
- A **base entity always exists.** A block has the capabilities `Move` and `Exit` by default.
- **Modifiers only declare.** A modifier is built from small parts; none of them decides anything on its own.
- **Central rules decide.** `Capabilities` and the event dispatch (§7) apply the rules. When every modifier is gone, what is left is a plain block.

### Parts

| Part | Says | Interface |
|---|---|---|
| **Suspends** | "While I am here, these capabilities are off" | `ISuspender { Capability Suspends }` |
| **Durability** | Only a number, plus how much one exit counts | `IDurable { Durability, AmountFor(exited) }` |
| **Move constraint** | "Only these directions" | `IMoveConstraint { Allows(direction) }` |
| **Passive data** | Information other modifiers read | `IModifier` only |

### Central rules
1. **Move:** no modifier suspends `Move`, and every constraint allows the direction.
2. **Exit:** no modifier suspends `Exit`, then the column scan (§5).
3. **On every exit:** each durability drops by its `AmountFor(exited)`; a modifier that reaches zero is **removed**.

### V1 modifiers

| Modifier | Parts | Meaning |
|---|---|---|
| **Ice** `{ count }` | `Suspends = Move \| Exit` · `Durability = count` · `AmountFor = 1` | Frozen: cannot move or exit until `count` blocks have exited, then removed |
| **Arrow** `{ direction }` | `IMoveConstraint` | Moves, and therefore exits, in one direction only |

### Stacking
- **Parallel:** independent modifiers on one block all apply at once. Ice + Arrow: frozen until thawed, then moves only in the arrow's direction.
- **Ordered:** one thing must clear before another becomes active. Not in V1; it needs dormant entities (edit point, §8).

---

## 📣 7. Events & Commands
*Built in Phase 7.*

### Three events
The [Mechanics Reference](../ColorBlockJamMechanics.md) trigger taxonomy reduces to three events:

| Event | Raised when | Covers triggers |
|---|---|---|
| **Exited** | A block leaves the grid | Exit count, exit toggle, exit move, exit by specific block, exit color |
| **MoveCommitted** | A player move ends (`CommitMove`, §9) | Move count, position |
| **Ticked** | Runtime ticks the session | Time |

- Each event has its own small listener interface (one method). A modifier implements only the ones it needs.
- **Dispatch:** one pass per event, over every entity still on the board, in id order. No reaction raises another event in the same pass.
- The durability rule (§6) is the central listener for **Exited**.

### Command primitives
Listeners never touch the board or the session directly. They get a narrow command API:

| Primitive | Used for |
|---|---|
| `AddModifier(entity, modifier)` · `RemoveModifier(entity, modifier)` | Toggles, recolor (via a color modifier), locks |
| `MoveEntity(entity, offset)` | Jumping doors, moving locks |
| `Fail()` · `AddTime(seconds)` | Bomb, Dynamite, Time Capsule |

- New effects are written by **combining primitives**, not by adding commands.
- **No event bus, no C# events.** The session calls the dispatcher explicitly at three points.

---

## 🔌 8. Extensibility

### The criterion
> **"Which existing file must change to add mechanic X?"** The answer must be **none**.

### Seams

| Seam | What it opens | Phase | Proven by |
|---|---|---|---|
| **New modifier** | Data + runtime type, no builder change (`ModifierData.ToModifier`) | ✅ 5 | Ice, Arrow |
| **Events & listeners** | Exited · MoveCommitted · Ticked | 7 | Fake toggle, fake move counter |
| **Command primitives** | Effects composed from primitives | 7 | Fake listener calling `Fail` |
| **Move boundary** | `CommitMove` + `MoveCount` | 7 | Test |
| **Effective color** | `IColorSource` can change a color; one read point | 8 | Fake color modifier |
| **Win exemption** | A modifier can declare that its block does not count toward win | 8 | Fake never-exiting block |
| **Discovery** | JSON type names, editor drawing and validation rules found by attribute | 10–12 | Fake modifier round-trip and editor display |

### Documented edit points
These are **not** opened in V1. Each needs a change in a known place, written down so it is a choice, not a surprise:

| Edit point | Opened by | Change stays in |
|---|---|---|
| **Floor layer** (non-blocking cell features) | Colorful Path, Button, Laser receiver | `Grid`, `Board`, builder, `Board.CanPlace` |
| **Movement groups** | Combined, Magnet | `BlockMover`, `Board.CanPlace`, `Board.MoveEntity` |
| **A new primitive** | A fourth event type or a new command primitive | Dispatcher / command API |
| **Wall / door modifiers** | Door toggle, Iced Door, Locked Door, Colorful Door, Size-Changing Door, Jumping Single Door | `WallData`, `DoorData`, builder; an `Accept` capability checked in `ExitRule`; per-cell accept rules for partly closed doors |
| **Dormant entities** | Crate, Hidden, Tangled, Barrier, Ivy, ordered stacking | `Entity` / `Board` (state outside the occupancy), `BlockData` + builder (flag, stable ids), command API (`Activate` / `Deactivate`) |
| **Listener inputs** | Color Swapping Block, Colorful Crate, Moving Door Lock, Laser Door (read the board) · Colorful Door, Locked Door (which door the block exited through) | Listener signatures gain a read-only board view or the exited-through doors; `ExitRule` collects the doors it matched |

- **Occupied always blocks.** No modifier makes an occupied cell passable. A mechanic that opens cells (Barrier, Ivy) takes its entity out of the occupancy instead (dormant edit point).

### Research → seam map (examples)

| Mechanic | Built from |
|---|---|
| Turn Based Arrow | `IMoveConstraint` + `IDurable` |
| Moving Blocks | `ISuspender(Exit)` + win exemption |
| Curtain | Exited listener + `AddModifier` / `RemoveModifier` |
| Color-Switching Block | Exited listener + color modifier |
| Dynamite | MoveCommitted listener + `Fail` |
| Time Capsule | Own-exit listener + `AddTime` |
| Locked Door | A door modifier suspending `Accept` (after the wall / door edit point) |
| Iced Door | Same, plus durability: a new `IcedDoor` modifier, or `Accept` added to Ice's list (no effect on blocks) |
| Barrier | Exited listener + `Activate` / `Deactivate`: an open barrier is out of the occupancy (after the dormant edit point) |
| Ivy | One single-cell wall entity per tile; an Exited listener deactivates one per exit (after the dormant edit point) |
| Crate, Hidden, Tangled | Dormant entity + `Activate` (after the dormant edit point) |

---

## ⏱️ 9. Game State
- **States:** `Playing → Won` or `Playing → Failed`.
- **Timer:** the initial time comes from `LevelData`. Core ticks it through `ITickable.Tick(dt)`. Time is never read from Unity.
- **Moves** (Phase 7): Runtime calls `CommitMove()` when a player move ends (drag release). It increments `MoveCount` and raises **MoveCommitted**. A block that exits mid-drag ends the move.
- **Win:** every block that must exit has exited. A modifier can exempt its block (Phase 8).
- **Fail:** time reaches zero, or a listener calls `Fail()` (Phase 7).
- **Continue:** Core allows `Failed → Playing` with added time (`AddTime`). When and how often it is offered is Runtime's decision.
- **Restart:** state is rebuilt from `LevelData`. Definition (immutable) and state (positions, modifiers, timer, moves) are kept apart, so no snapshot is needed.

---

## 💾 10. Level Data & IO
- **`LevelData` DTOs live in `Game.Core`**, plain C#, no serializer attribute dependencies.
- **JSON lives in `Game.LevelIO`** (Core + Newtonsoft), shared by Runtime and the Level Editor.
- **Polymorphic modifiers:** a `type` discriminator per modifier. Each DTO declares its own type name, and the registry discovers them, so a new modifier needs no registry edit. **No `TypeNameHandling`**: it is unsafe and breaks when a class is renamed.
- **Doors** are stored as entities with their own cells, color and direction.
- **Shape only, not final:**

```json
{
  "schemaVersion": 1,
  "width": 8, "height": 10,
  "timeLimit": 90,
  "walls":  [ { "cells": [[0,0],[1,0],[2,0]] } ],
  "doors":  [ { "colorId": 2, "direction": "Down", "cells": [[3,0],[4,0]] } ],
  "blocks": [
    { "colorId": 2, "cells": [[3,1],[4,1]],
      "modifiers": [ { "type": "ice", "count": 3 }, { "type": "arrow", "direction": "Down" } ] }
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
- Every entity cell is inside the grid; no two entities overlap.
- Each modifier DTO can validate its own values (e.g. Ice count > 0), so a new modifier brings its own checks.
- **The border rule is not checked here.** It lives only in the Editor, which refuses to save a level that breaks it.

---

## 🛠️ 11. Level Editor
- **Editor-only**, under an `Editor/` folder, in its own assembly. It does not depend on Runtime.
- **Visual JSON editing:** open, create, save and overwrite levels.
- **Fast to use is the priority.** Its working model is **data-oriented**: flat arrays, id → cell indices and lookups by type or color, so queries stay fast. It converts to and from `LevelData` only on load and save.
- **Additive:** modifier editing and validation rules are discovered, so a new modifier shows up in the editor without editor changes.
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
| Modifier values are valid (each DTO's own checks) | Editor + Core (load time) |

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
- Hand-written fakes over mocks. Fakes in the test assembly also prove each seam (§8).
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

**Order:** skeleton → grid → core game → modifiers → scope trim → seams (events, rules) → data & tools (validation, IO, editor) → additivity proof. Presentation comes after V1.

| # | Phase | Sub-steps | Done when | Status |
|---|---|---|---|---|
| 0 | Skeleton | 0.1 `Game.Core` (no UnityEngine) + `Game.Tests.EditMode` asmdefs · 0.2 first test | `CoreAssembly_Compiles_WithoutUnityEngine` | ✅ |
| 1 | Grid & Entities | 1.1 `Direction` + cell ↔ index math · 1.2 entity base (shape + position; Block / Wall / Door) · 1.3 occupancy fill and cell query · 1.4 `LevelData` DTO + Core builder · 1.5 ASCII map → `LevelData` test helper | `Builder_PlacesEntities_IntoOccupancy` | ✅ |
| 2 | Movement | 2.1 step into empty cells · 2.2 blocked by wall / door / block, own cells free · 2.3 multi-cell shapes (bar, L, U) · 2.4 `TryMove` result `Moved / Blocked` | `TryMove_StopsAt_OccupiedCell` | ✅ |
| 3 | Exit | 3.1 straight door exit, batch clear · 3.2 rejects: wrong color, wrong direction, too wide · 3.3 recess: open U passes, U with a block inside fails · 3.4 several door entities, inner door direction | `UShape_WithBlockInRecess_DoesNotExit` | ✅ |
| 4 | Game State | 4.1 win · 4.2 timer driven by `Tick(dt)` → fail · 4.3 continue `AddTime` · 4.4 restart from definition | `Restart_RestoresInitialState` | ✅ |
| 5 | Modifier Framework | 5.1 capabilities + `ISuspender` / `IDurable` / `IMoveConstraint` · 5.2 `Capabilities` + `ExitResolver` (depleted → removed) · 5.3 Ice, Arrow · 5.4 Key/Lock, Rope/Scissors (removed in Phase 6) · 5.5 parallel stacking | `RopeAndIce_BothMustResolve_BeforeMove` | ✅ |
| 6 | Scope Trim | 6.1 remove Key/Lock and Rope/Scissors with their tests · 6.2 stacking proven with Ice + Arrow | `IceAndArrow_ArrowAppliesAfterThaw` | ✅ |
| 7 | Events & Commands | 7.1 Exited / MoveCommitted / Ticked + listener interfaces + dispatcher · 7.2 command primitives; durability removal through `RemoveModifier` · 7.3 `CommitMove` + `MoveCount` · 7.4 `Fail` / `AddTime` from listeners | `Listener_CallingFail_FailsTheLevel` | ✅ |
| 8 | Rule Seams | 8.1 effective color (`IColorSource`) · 8.2 ~~`Accept` capability + per-cell accept rules~~ moved to an edit point (D43) · 8.3 ~~modifiers on walls and doors (DTO + builder)~~ moved to an edit point (D41) · 8.4 win exemption | `ColorSource_DecidesWhichDoorTheBlockExitsThrough` | ✅ |
| 9 | ~~Dormant Entities~~ | Dropped from V1: dormant entities became a documented edit point (D42) | — | ➖ |
| 10 | Load-time Validation | 10.1 bounds and overlap · 10.2 unsupported `schemaVersion`, unknown modifier type · 10.3 per-DTO value checks | `Load_Rejects_OverlappingEntities` | ⏳ |
| 11 | LevelIO | 11.1 `Game.LevelIO` asmdef + tests · 11.2 discovered type registry, polymorphic modifiers · 11.3 round-trip, unknown type rejected · 11.4 `ILevelSource` contract (key-based) | `RoundTrip_PreservesAllModifiers` (incl. a test-assembly fake) | ⏳ |
| 12 | Level Editor | 12.1 `Game.LevelEditor` asmdef + DOD model, `LevelData` ↔ model · 12.2 validation rules (discovered) · 12.3 window: painting, entity grouping, modifier editing (discovered) · 12.4 save / load / overwrite | `EditorModel_RoundTrip_EqualsLevelData` + rule tests | ⏳ |
| 13 | Additivity Proof | 13.1 one mechanic from the reference added end to end in the test assembly (data, IO, logic), zero `Game.Core` changes · 13.2 `docs/production/Extending.md`: recipe + edit points | `TurnBasedArrow_AddedWithoutCoreChanges` | ⏳ |

- **ASCII test helper (1.5) is test-only.** It lives in the test assembly and produces a plain `LevelData`. Core, LevelIO, the Editor and Runtime never see it. JSON stays the only level format.
- **After V1 (headline only):** Presentation: `GameInstaller`, palette, modifier → view mapping as assets, `BlockDrawRule`, drag, exit visual, timer / continue UI.

---

## 🧭 15. Ready For, Not Built
- **Every mechanic in the [Mechanics Reference](../ColorBlockJamMechanics.md)** that maps to a seam in §8, without changing existing files.
- **Floor layer and movement groups:** documented edit points (§8).
- **Addressables / downloadable levels:** `ILevelSource`, keys, `schemaVersion`.
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
| D9 | Modifier contract | ~~Gate / Reactor / Board command~~ — superseded by D30 and D32 |
| D10 | Exit flow | ~~Exit is the only trigger~~ — superseded by D32 (three events) |
| D11 | Exit order | Exiting entity's own listeners first, then all other entities still on the board |
| D12 | Carried reactors | ~~Run while carried~~ — removed with Tangled (D33) |
| D13 | Stacking | Parallel in V1 (Ice + Arrow); ordered stacking needs dormant entities (edit point, D42) |
| D14 | Ice | Any exit counts; cannot move or exit until 0; colored variant must be possible |
| D15 | Key/Lock | ~~Separate `keyId`, broadcast by key count~~ — removed from scope (D33); still in the reference |
| D16 | Rope/Scissors | ~~Cut by matching scissors exit~~ — removed from scope (D33); still in the reference |
| D17 | Tangled | ~~Outer exits first, inner activated~~ — out of scope (D33); needs dormant entities (edit point, D42) |
| D18 | Arrow | A modifier; one direction only |
| D19 | Wall/door grouping | Designer-defined entities |
| D20 | Game state | Timer from `LevelData`; fail on timeout; continue adds time (Runtime decides); restart from `LevelData`; no undo |
| D21 | JSON | Polymorphic with a `type` discriminator, Newtonsoft, in `Game.LevelIO` |
| D22 | Addressables | Prepared (keys, `ILevelSource`, `schemaVersion`), not built |
| D23 | Palette | `colorId` in data and Core; colors in Runtime, remote-config ready |
| D24 | Validation | Load-time structural rules in Core; design and border rules in the Editor |
| D25 | Direction | One shared `Direction` type |
| D26 | Level Editor | Editor-only assembly, data-oriented, independent of Runtime |
| D27 | Phase order | Validation before IO; Level Editor before the additivity proof |
| D28 | ASCII test helper | Test-only; produces `LevelData`; never a level format |
| D29 | Timer input | Time enters Core only through `ITickable.Tick(dt)`; no `ITimeProvider`, tests pass `dt` directly |
| D30 | Modifier model | Base block always has `Move` and `Exit`; modifiers only declare parts (`ISuspender`, `IDurable`, `IMoveConstraint`, passive data). Central rules decide; a depleted modifier is removed |
| D31 | Folder layout | One folder per mechanic under `Modifiers/` (runtime + DTO together); modifier framework at `Modifiers/` root. Tests mirror only the first level |
| D32 | Events & commands | Three events (Exited, MoveCommitted, Ticked), one-method listener interfaces, explicit dispatch; listeners act only through command primitives |
| D33 | V1 mechanics | Normal block, Arrow, Ice. Key/Lock and Rope/Scissors are removed; Tangled is out of scope |
| D34 | Additive criterion | Adding a mechanic from the reference must change no existing file; exceptions are documented edit points |
| D35 | Move boundary | A single `CommitMove()` per player move; no begin/end pair |
| D36 | Win exemption | Declared by a modifier, consistent with the modifier model; no data flag |
| D37 | Board seams | ~~Only dormant entities are built~~ — superseded by D42: no board seam is built; floor layer, movement groups and dormant entities are documented edit points |
| D38 | Entity kinds | Closed at Block / Wall / Door; variation only through modifiers; shapes never change |
| D39 | Additivity proof | A final phase adds one reference mechanic in the test assembly with zero Core changes, plus `Extending.md` |
| D40 | Explicit suspension | No `Capability.All`: every modifier lists the capabilities it suspends, so a capability added later is never suspended by accident. Ice suspends `Move \| Exit`. Base colors renamed `BaseColorId`; effective color only via `Colors.Of` |
| D41 | Wall / door modifiers | Out of V1 scope in data: `WallData` / `DoorData` and the builder carry no modifiers. Every entity keeps its modifier list; adding wall / door modifiers to data is a documented edit point |
| D42 | Dormant entities | Out of V1 scope, Phase 9 dropped. No stable ids in DTOs (builder assigns them), no `Activate` primitive. Dormant entities are a documented edit point; Crate, Hidden, Tangled and ordered stacking open it |
| D43 | Door acceptance | `Accept` capability and per-cell accept rules (`IExitAcceptRule`) removed: nothing in V1 uses them once door modifiers are out of data (D41). They return with the wall / door edit point. `Board.RemainingBlockCount` removed too; `WinRule` alone decides the win |
| D44 | Occupancy | An occupied cell always blocks; no passability check in `CanPlace`. Mechanics that open cells (Barrier, Ivy) take entities out of the occupancy (dormant edit point). Listener inputs (board view, exited-through doors) are a documented edit point, added as parameters when first needed |
