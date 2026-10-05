<div align="center">

# 🔬 FINDINGS — PrototypeV1

**Everything the prototype settled, tuned, paid for, dropped and converged on.**
The required input for the first production phase.

![Rules](https://img.shields.io/badge/Rules-27-2ea043)
![Tuning](https://img.shields.io/badge/Tuning-5-1f6feb)
![Costs](https://img.shields.io/badge/Costs-8-d29922)
![Rejected](https://img.shields.io/badge/Rejected-15-cf222e)
![Shapes](https://img.shields.io/badge/Shapes-9-8250df)

<sub>[README](../../README.md) · [PrototypeV1](PrototypeV1.md) · [Algorithm Explanation](AlgorithmExplanation.md)</sub>

</div>

> [!IMPORTANT]
> **Question:** Can the existing model kit be used to build a level grid with blocks and doors, then move blocks and exit them through matching-color doors?

| Heading | Meaning |
|---|---|
| [📏 Rule](#-rule) | A mechanic or technical rule that is now settled |
| [🔧 Tuning](#-tuning) | A value that felt right, with the value |
| [💸 Cost](#-cost) | Something that turned out expensive, or a shortcut production must not copy |
| [❌ Rejected](#-rejected) | Something tried and dropped, and why |
| [🧬 Shape](#-shape) | A data shape the code kept converging on |

<sub>`(Qn)` = answers open question n in [PrototypeV1 §8](PrototypeV1.md#-8-open-questions). ✔️ = verified in the Editor.</sub>

---

## 📏 Rule

### Assets & prefabs
- GroundGrid, BlockParts and Arrows FBX are not modified; their fixes live on the prefab's Mesh level. WallAndDoor.fbx was cleaned once and needs no fix.
- Prefab = Root (grid position/rotation, gameplay) → Visuals (effects only) → Mesh (import fix; script swaps mesh/material only).
- Wall family shares one profile (1 wide, top +0.611, 12-segment bevel); Wall and Door run along X at rotation 0.
- Block and wall pivots sit at ground level; meshes extend below the ground. BlockPiece offset 0, ArrowPiece +0.656.
- Prototype colors are runtime materials — `new Material(template)` per color, cached, shared by blocks and doors. Authored `.mat` assets are a production concern.

### Grid & board
- Cell = 2 Unity units (4 quadrants, 4 studs per cell).
- Board edges: bottom 0, top 180, left 90, right 270. Corners: BL 0, BR 270, TR 180, TL 90. DoorArrow lives inside DoorPiece.
- Board edge table: Wall = 1-unit segments, 2 per cell edge, ring 0.5 outside the board; Corner rotations BL 0, BR 270, TR 180, TL 90. ✔️
- Adjacent same-color door edges merge into one door (logic and visuals). The original game shows no separate adjacent same-color doors.
- Border renders as runs: consecutive equal edges = one piece; Mesh_Wall scaled 2N, Mesh_Door scaled N, DoorArrow unscaled at the run center. ✔️
- Level authoring = inspector field list: `List<BlockData { color, cells }>` on `Board`; block id = list index. (Q2)

### Blocks
- Any polyomino block shape is in V1 scope, including L/T. (Q1)
- At rotation 0, OuterCorner / Edge / Center cover the pivot's top-right quadrant (+X, +Z); OuterCorner rounds toward (+X, +Z), Edge wall faces +Z. (Q6)
- InnerCorner pivot = vertex; at rotation 0 its empty quadrant is (+X, +Z); placed by the cell diagonal to the empty cell. (Q6)
- Axis lock is enforced only in `CanPlace` (drag code untouched). Arrow: nearest block cell to the bounding-box center → unbroken run along the locked axis → `Arrow_min(run, 3)` at the run center. `Arrow_N` lies along Z at rotation 0 (`arrowYawOffset = 90`). ✔️

### Input & movement
- Selection = ray onto the ground plane → `floor(local / 2)` → `cells[x, y]`; no colliders. ✔️
- Drag is free 2D (as in the original): the visual follows the pointer, clamped to reachable space; logic still walks cell by cell; release snaps to the nearest reachable cell. (Q5)
- Step valid = every target cell inside + empty or own + crossed edge Open. Logic walks toward the pointer's rounded cell, larger axis first, other axis if blocked. ✔️

### Exit
- Exit width rule: a block exits only if every cell on the pushed side faces a Door edge of its color; one Wall edge or wrong color blocks it.
- Exit = a blocked step checks `CanExit`: block pressed against the border on that side, every covered column/row faces a Door edge of its color; then its cells clear at once (no longer clickable) and the exit visual plays. Triggers mid-drag. ✔️ on a 6×8 level.
- Exit visual: snap onto the door cell → slide depth·cell + clipOffset along the exit dir → root disabled. The block is cut by a per-block world clip plane (`MaterialPropertyBlock`) inside the door, so InnerCorner and Arrow need no special handling. ✔️
- Exit cut face: BlockClip draws back faces too (`Cull Off`), flat and unlit (color × `_CapShade`), so the cut reads as solid; no cap mesh.
- `Block_{id}` has no Visuals level: exit and drag tweens move the root, because logic never reads an exited block's root. (Q7)

### Feedback & tooling
- Exit particles burst per row (front first) when the row center reaches the cut line; timed by travel distance, not time, so any ease works.
- Exit particles use a lit mesh material (Standard). Color comes from a per-instance `MaterialPropertyBlock` because Standard ignores particle vertex color; Color over Lifetime has no effect with it.
- SFX: Select on grab, Drop on release (skipped when the block exited mid-drag), Crunch when the exit slide starts. One shared 2D AudioSource on Board, `PlayOneShot`.
- Tweens use DOTween. `Game.Prototype` references `DOTween.Modules` by GUID; `Assets/Plugins` and `Assets/Resources` must be committed with their `.meta` files or the GUID breaks.

---

## 🔧 Tuning

| Area | Field | Value | Note |
|---|---|---|---|
| Doors | `doorArrowUsesDoorColor` | `false` | Light arrow (prefab default) reads better |
| Doors | `doorHeightScale` | `1` | The shaded cut face is enough; a taller door was not needed |
| Drag | `lift` | `0.3` | Feels fine; the original has almost none, 0 is a valid candidate |
| Drag | `snapSpeed` | `50` | Release settle |
| Exit | `snapDuration` · `speed` · `ease` · `clipOffset` · `_CapShade` | `0.08` · `10` u/s · `Linear` · `0.5` (door center) · `0.6` | |

---

## 💸 Cost

| Area | Cost | Production |
|---|---|---|
| Doors | `DoorData.length` is not bounds-checked; a door running past the board edge throws. | Accepted in the prototype |
| Exit | Every covered column/row is checked, so recess cells (e.g. a U open toward the door) must also face the door. | Not seen in the original; accepted |
| Exit | Only the border edge is checked, not the cells between a column and the border: a U exits through a block sitting in its recess. | Must check those cells |
| Movement | The `CanPlace` edge check is dormant (border is caught by bounds, inner edges all Open). | Kept on purpose: walls inside the board are planned |
| Movement | Diagonal `CanPlace` (only `Lean` uses it) skips the edge check, so with inner walls the visual could lean past a wall corner. | Grid logic must not allow it |
| Visuals | Run stretching and `doorHeightScale` write Mesh-level scale/position (the prefab rule says the Mesh transform is fixed). | Needs another approach |
| Rendering | Exit needs a custom block shader (BlockClip) instead of the FBX's Standard material; the per-block `MaterialPropertyBlock` breaks batching for that block while it exits. | Accepted in the prototype |
| Allocation | Exit allocates per call (`MaterialPropertyBlock`, row lists, tween closures, particle instance). | Pooled particles, cached property blocks, reused row buffers |

---

## ❌ Rejected

| Idea | Why it was dropped |
|---|---|
| Tile gap via tile scale | Writes scale on the root and drifts from the 2-unit kit |
| Extra cell gap (pitch = 2 + gap) | Seams inside multi-cell blocks; the mesh's own 0.047 tile gap is enough, pitch stays 2 |
| Physics colliders for movement | Snap and tunneling problems; a grid check is enough |
| One-axis-per-step drag with grid snap while held | The original game drags in free 2D |
| Quadrant-resolution grid (2W×2H) | Adds nothing to the logic; quadrants are visual only |
| BlockPiece +0.656 offset | Lifted the block off the ground; the "bottom rests on the ground" assumption was wrong |
| Toggle prefab (all variants as children, enable one) | InnerCorner (2×2, on a vertex) does not fit a quadrant slot; leaves dead objects |
| One prefab per piece | Repeats the same orientation fix ~10 times |
| Door inside WallPiece via mesh swap | DoorArrow had to be placed separately; DoorPiece places door + arrow in one step |
| `Corner_1`, `corner_4`, `corner_3` | Same function as `corner_5` with worse topology (`corner_4` had a triangle fan) or redundant (`corner_3` = Corner + 2 Walls) |
| Door list `{ side, index, length, color }` | Border-only and needs span math; per-edge doors make the width rule a per-cell check and allow doors on any edge |
| Exit by hiding meshes row by row | Chunky, and InnerCorner spans two rows so one of them always shows a hole or an overhang |
| Exit via depth mask outside the door | Camera clears with Skybox (artifacts in the masked area), view-dependent, also hides particles |
| Exit via stencil | Needs a block shader change anyway; no gain over a clip plane |
| Exit via squashing the root along the exit axis | Reads as crushing, not entering; studs deform |

---

## 🧬 Shape

| Shape | Definition |
|---|---|
| **BlockDrawRule** | Block cells → list of (mesh, local position, Y rotation); pass 1 vertices (InnerCorner), pass 2 quadrants (OuterCorner/Edge/Center). Reads only the block's own cells. |
| **Piece placement** | Rotation picks the wall/corner direction, pivot offset puts the footprint on its quadrant: `pos = cellCenter + quadrant − R(θ)·(0.5, 0.5)`. Rotation alone reaches only 4 of the 8 Edge cases. |
| **Prefabs** | GroundTile, BlockPiece, ArrowPiece, WallPiece (Wall/Corner/WallNotch swap, fixed material), DoorPiece (Door + DoorArrow fixed, door color). |
| **WallNotch** | 1-cell wall tooth; longer teeth add 2 Walls per cell. No T-junction piece. |
| **Tile ↔ quadrants** | One GroundGrid tile = one cell = 4 BlockPiece quadrants; InnerCorner sits on the corner shared by 4 tiles. |
| **Runtime grid** | Cell layer `int[,] cells` (blockId, -1 empty) + edge layer `hEdges[W,H+1]`, `vEdges[W+1,H]` (Open / Wall / Door(color)) + Block list (id, color, axisLock, cells). Editor, JSON and visuals may differ but load into this. |
| **Door authoring** | Prototype: `DoorData { cell, side, length, color }`. Production level editor and level data store doors per cell edge (one entry = one edge, deterministic, 1:1 with the edge layer); length stays a visual concern only. |
| **Materials** | Blocks and the axis-lock Arrow render with `Prototype/BlockClip` (Standard-like surface shader + world clip plane `(normal, distance)`, default off); doors stay on the template shader. One material per color, clip plane per block via `MaterialPropertyBlock`. |
| **Exit pipeline** | Board (logic exit + border point + normal) → BlockExit (clip plane, DOTween sequence snap → sound → slide, row bursts) → BlockExitParticles (per-row emit). |
