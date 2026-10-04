# PrototypeV1 — ColorBlockJamCaseStudy

## 1. Project
- Unity case study that rebuilds the core loop of **Color Block Jam** (Rollic / Gybe Games, 2024, mobile puzzle).
- V1 is a **prototype**: the output is knowledge, the code is disposable (see `.claude/modes/prototype.md`).
- **Prototype question:** Can the existing model kit be used to build a level grid with blocks and doors, then move blocks and exit them through matching-color doors?

## 2. Color Block Jam — Core Logic
- **Board:** a rectangular grid of cells, enclosed by walls. Some wall segments on the edges are **colored doors**.
- **Blocks:** polyomino shapes (1×1, 1×N, N×M, L, T…). One color per block. A block always covers whole cells.
- **Move:** the player drags a block. It slides through empty cells and stops at walls and other blocks. Blocks never overlap.
- **Axis lock:** some blocks carry a double-headed arrow on top and can only move along that one axis.
- **Exit:** a block that reaches a door of **the same color**, where the door opening fully covers the block's span along that edge, leaves the board and frees its cells.
- **Win:** every block has exited.
- **The puzzle:** move order. Blocks block each other; the player must clear paths in the right sequence.
- Mechanics of the original that are **out of V1 scope:** time limit, obstacles, gate traps with buttons, other special blocks.

## 3. V1 Scope
- **In:** grid, blocks of any polyomino shape (via `BlockDrawRule`, including L/T), board walls, doors, drag, grid collision, axis lock, exit, win.
- **Deferred:** see section 7.

## 4. Asset Analysis
Source: `Models/` — `GroundGrid.fbx`, `BlockParts.fbx`, `WallAndDoor.fbx`, `Arrows.fbx`.
Values below are measured from the mesh data and given in Unity import scale (meters).

### Source files rule
- `GroundGrid.fbx`, `BlockParts.fbx` and `Arrows.fbx` are **not modified**. Their orientation and offset fixes live in Unity, on the prefab's `Mesh` level.
- `WallAndDoor.fbx` was **cleaned up once** to match this plan (see "WallAndDoor (cleaned)" below). It needs no orientation fix in Unity.

### Cell size = 2 units
Supporting evidence:
- `GroundGrid` tile is 1.953 × 1.953, which means a pitch of 2 with a 0.047 gap.
- `Door` (formerly `Door_1x1`) is 2 long.
- Block parts are 1×1 quadrants, so one cell is made of 4 quadrants and has 4 studs.
- `Arrow_1/2/3` are 1.60 / 3.51 / 5.41 long, matching blocks of 1, 2 and 3 cells.

### Models

| Model | Footprint | Height | Pivot | Up axis in FBX |
|---|---|---|---|---|
| `GroundGrid` / `Grid` | 1.953 × 1.953, corner radius ~0.25 | flat | tile center | +Y (correct) |
| `BlockParts` / `ModularBlock_Center`, `_Edge`, `_OuterCorner` | 1 × 1 quadrant | top +0.656, stud tip +0.963, skirt to −0.656 | cell center (inner corner of the quadrant), at ground level | **−Z** |
| `BlockParts` / `ModularBlock_InnerCorner` | 2 × 2 (3 quadrants filled) | same as above | cell-corner vertex (the concave corner), at ground level | **−Z** |
| `Arrows` / `Arrow_1`, `_2`, `_3` | width 0.91, length 1.60 / 3.51 / 5.41 | thickness 0.37 | center of the arrow, at its base | **−Z** |
| `WallAndDoor` (cleaned) | see below | top +0.611, skirt to −2.965 | ground level | +Y (correct) |

Notes:
- **How "up" was determined:** from the direction of the closed top face, the open (uncapped) bottom, and which end carries the bevels.
- **Pivot convention (verified in Editor):** block and wall pivots sit at **ground level**. The meshes extend below the ground as a hidden skirt, so no seam shows between a piece and the floor.
  - Block: top surface +0.656, stud tips +0.963, skirt down to −0.656. The bottom is open.
  - Wall: top +0.61, skirt down to −2.97. The bottom is open.
  - Block top and wall top are almost level (+0.656 vs +0.61).
- **Arrow mesh:** spans 0.005–0.378 above its pivot. The heads point both ways along the arrow's length.
- **Materials:**
  - `BlockParts` uses a single material, `ModularBlock_Base`, so block color comes from the material.
  - `WallAndDoor`: `Wall`, `Corner` and `WallNotch` use `Base_Side`; `Door` uses `Gate_Orange`; `DoorArrow` uses `Arrow_Gate_Orange`. One material slot per mesh.
  - `GroundGrid` and `Arrows` have no material.
- **GroundGrid mesh:** a single 20-vertex n-gon. All UVs are 0, so it can take a flat color but no texture.
- **Unit export inconsistency:** `GroundGrid` and `Arrows` were exported in cm (`UnitScaleFactor` = 1, vertex values ×100). `BlockParts` and `WallAndDoor` were exported in m (`UnitScaleFactor` = 100). Unity's `Use File Scale` (on in all `.meta` files) normalizes this. **Do not turn it off**, or `GroundGrid` and `Arrows` will import 100× too large.

### WallAndDoor (cleaned)
All values in Unity space. Every piece: up = +Y, pivot at ground level, top +0.611, hidden skirt down to −2.965, same 12-segment bevel profile (1 unit wide, bevel radius 0.26).

| Mesh | Footprint (Unity) | Rotation 0 means |
|---|---|---|
| `Wall` | x ±0.5, z ±0.5 | straight segment running along **X** |
| `Door` | x ±1.0, z ±0.5 | straight 1-cell segment running along **X** |
| `DoorArrow` | ~0.40 × 0.42, base on the door top (+0.611) | same pivot as `Door`; tip points **−Z** (outward on the bottom edge) |
| `Corner` | x ±0.5, z ±0.5 | rounded outer corner at **(−X, −Z)**; arms connect to walls toward **+X** and **+Z** (bottom-left board corner) |
| `WallNotch` | x ±1.0, z ±1.0 (one cell) | two parallel arms along Z with a rounded U-cap at **+Z**; open ends at −Z. For a missing cell on the bottom edge (deferred) |

What changed from the original file:
- Upside-down geometry flipped (top was −Y).
- `Corner_1`, `corner_4` and `corner_3` removed. `corner_5` (clean topology) kept as `Corner`.
- `Wall` and `Door` rebuilt with the same 12-segment profile as `Corner`, so seams match (max deviation 0.008).
- `Wall` now runs along X, same as `Door`.
- `DoorArrow` moved onto the door top and centered.
- `corner_3.002` renamed `WallNotch`, pivot moved to its cell center.
- Leftover shape key, emission texture and unused material slots removed.
- Shading: smooth by angle (30°) on every piece.

### Board edge placement
Board origin = bottom-left corner of cell (0, 0). Cell (i, j) center = (2i + 1, 0, 2j + 1). Board size = 2W × 2H. Edge pieces sit in a 1-unit ring outside the board.

| Edge | Wall / Door center | Root Y rotation |
|---|---|---|
| Bottom | z = −0.5 | 0 |
| Top | z = 2H + 0.5 | 180 |
| Left | x = −0.5 | 90 |
| Right | x = 2W + 0.5 | 270 |

- **Wall segments:** 2 per cell edge, centered at 2i + 0.5 and 2i + 1.5 along the edge.
- **Door:** 1 `DoorPiece` per cell, centered at 2i + 1 along the edge. `DoorArrow` lives inside `DoorPiece`, so its tip points out of the board on every edge automatically.
- **Corners:**

| Corner | Position | Root Y rotation |
|---|---|---|
| Bottom-left | (−0.5, 0, −0.5) | 0 |
| Bottom-right | (2W + 0.5, 0, −0.5) | 270 |
| Top-right | (2W + 0.5, 0, 2H + 0.5) | 180 |
| Top-left | (−0.5, 0, 2H + 0.5) | 90 |

- Positions and rotations were checked by assembling a 3 × 2 board from the exported data. Verify once in the Editor.

### Prefabs
Every prefab has the same three levels:
```
<Prefab> (root)        → position + Y rotation on the grid; gameplay components go here
└─ Visuals             → effects and animation only (punch, shake, fade…)
   └─ Mesh             → MeshFilter + MeshRenderer; import fix (rotation/offset) lives here
```
- **Root** is model-independent. The script writes position and rotation **only** here.
- **Visuals** is free for effects. It stays at identity when nothing plays, and no placement logic writes to it.
- **Mesh** holds the fixed import correction. The script only swaps `MeshFilter.sharedMesh` and `MeshRenderer.sharedMaterial` here, never the transform.
- Mesh and material can be swapped freely because every mesh has a single material slot.

| Prefab | `Mesh` rotation | `Mesh` offset (Y) | Mesh(es) | Material |
|---|---|---|---|---|
| `GroundTile` | — | 0 | `Grid` | fixed |
| `BlockPiece` | X = 90 | 0 | swapped: `ModularBlock_Center`, `_Edge`, `_OuterCorner`, `_InnerCorner` | block color |
| `ArrowPiece` | X = 90 | +0.656 (block top; covers the studs by ~0.07) | swapped: `Arrow_1`, `Arrow_2`, `Arrow_3` | fixed |
| `WallPiece` | — | 0 | swapped: `Wall`, `Corner`, `WallNotch` | `Base_Side`, fixed |
| `DoorPiece` | — | 0 | two fixed `Mesh` children under `Visuals`: `Door` + `DoorArrow` | door color |

- **Verified in Editor:** `BlockPiece` with X = 90 and offset 0 sits correctly on the ground.
- **Not yet verified:** `ArrowPiece` values. Check them when first used.
- **Wall height** (+0.611 vs. block top +0.656) is a feel knob.
- **Door arrow color:** whether the arrow takes the door color or stays a light tone is a feel decision for Phase 6.

### Scene hierarchy
```
Board
├─ Tiles      → GroundTile × W·H
├─ Walls      → WallPiece × (edge segments + corners) + DoorPiece × door cells
└─ Block_{id} (root, this is what moves)
   ├─ BlockPiece × quadrant count
   └─ ArrowPiece (if axisLock)
```
- Moving a block = moving `Block_{id}` only.
- Whether `Block_{id}` also gets a `Visuals` level (for block-wide effects such as the exit animation): see Q7.
- **Color:** one material per color, shared by all pieces of a block and by the doors of the same color.

### Block autotile (quadrant rule)
- **Layout:** one `GroundGrid` tile = one cell (2 × 2 units). The block's part on that cell is made of 4 quadrant pieces placed on the tile.
  - Each piece is 1 × 1 with one stud.
  - All 4 pieces have their pivot at the tile center. Rotating the root by 0 / 90 / 180 / 270 around Y turns each into a different quadrant.
  - In FBX space the unrotated piece covers X ∈ [−1, 0], Y ∈ [0, 1] relative to its pivot. Which Unity quadrant this is: see Q6.

```
┌─────────┬─────────┐
│  Q (◯)  │  Q (◯)  │
├─────────┼─────────┤   = 1 GroundGrid tile = 1 cell
│  Q (◯)  │  Q (◯)  │
└─────────┴─────────┘
```

- **Exception, `InnerCorner`:** it is not placed inside one tile but on the **corner where 4 tiles meet**. The 3 quadrants it covers belong to 3 different cells.

```
┌────┬────┐┌────┬────┐
│    │    ││    │    │   cell A        cell B
├────┼────┤├────┼────┤
│    │ ▓▓ ││ ▓▓ │    │
└────┴────┘└────┴────┘
┌────┬────┐┌────┬────┐
│    │ ▓▓ ││    │    │   cell C        cell D (empty)
├────┼────┤├────┼────┤
│    │    ││    │    │
└────┴────┘└────┴────┘
▓▓ = quadrants covered by InnerCorner, pivot = the shared corner in the middle
```

- **Rule:** each quadrant looks at its 2 orthogonal neighbor cells and 1 diagonal neighbor cell. Only cells of the **same blockId** count as neighbors.
  - Both orthogonal neighbors empty → `OuterCorner` (walls on both outer sides).
  - One orthogonal neighbor empty → `Edge`, rotated so its wall faces the empty side.
  - Both orthogonal neighbors filled, diagonal filled → `Center`.
  - Both orthogonal neighbors filled, diagonal empty → this vertex has exactly 3 filled cells. Place one `InnerCorner` on the vertex. It covers those 3 quadrants, so no other piece goes there.

### BlockDrawRule
The place where the autotile rule above lives: given a block's shape, it decides which mesh goes where.
- **Input:** one block's cell list. Only the block's own cells matter, so it does not read the grid or any gameplay state.
- **Output:** a list of pieces, each = (mesh, local position, Y rotation). The block is dressed once from this list.
- **Pass 1, vertices:** for every vertex touching the block, count the filled cells among the 4 around it. Exactly 3 → one `InnerCorner` on the vertex, rotated so its empty quadrant faces the empty cell. Mark those 3 quadrants as consumed.
- **Pass 2, quadrants:** for every quadrant not consumed: both orthogonal neighbors empty → `OuterCorner`; one empty → `Edge`; both filled → `Center`. (Both filled with the diagonal empty cannot reach this pass; pass 1 consumed it.)
- **Rotation table:** which Y rotation sends a piece to which quadrant comes from Q6, verified in the Editor.
- **Prototype code:** a plain function, ugly is fine. Its value is in revealing which shapes break the rule.
- **Test shapes:** 1×1, 1×3, 2×2, 2×3, L, T, S/Z, plus (+), U, 3×3 ring with a hole, two cells of the same block touching only diagonally.

## 5. Data Shape (agreed)
```
int[,] cells            // W×H, value = blockId, -1 = empty
Block { id, color, axisLock, List<Vector2Int> cells }
Door  { side, index, length, color }
```
- **The grid is at cell resolution.** Quadrants exist only in the visuals.
- **Walls are not stored.** Everything outside the grid counts as wall.
- **Doors are edge data**, not grid cells.
- **Visuals:** one root GameObject per block, dressed **once** from same-blockId neighbors. A move only changes the root transform; no re-dressing.
- **Click:** cast a ray onto the ground plane, convert the hit point to a cell with `floor(x / 2), floor(z / 2)`, read `cells[x, y]` to get the `blockId`. No colliders.
- **Move:**
  - Advance the block one cell at a time. A step is valid when every target cell is inside the grid and is either empty or the block's own.
  - On a valid step, set the old cells to −1 and write the `blockId` into the new cells.
  - Step-by-step movement prevents tunneling.
  - During drag the visual position follows the finger, clamped to the furthest reachable cell. On release it snaps to the nearest cell.
- **Exit:** the block is removed when all three hold:
  - It is pressed against an edge that has a door.
  - The door's color matches the block's.
  - The door opening covers the block's full span along that edge.

## 6. Phase Plan
One phase per answer, following the prototype process. **Files touched** for each phase are decided when that phase starts.

| # | Phase | Done when (visible in Editor) | Depends on | Status |
|---|---|---|---|---|
| 1 | Grid renders | W×H `GroundGrid` tiles laid out at cell = 2 pitch | — | Done |
| 2 | 1×1 block | A single-cell block (4 × `OuterCorner`) appears with correct orientation and height, colored | Q2, Q6 | In progress |
| 3 | BlockDrawRule: convex shapes | `BlockDrawRule` dresses 1×N and N×M blocks with `OuterCorner` / `Edge` / `Center` (pass 2 only) | Q6 | Not started |
| 4 | BlockDrawRule: concave shapes | L, T, S/Z, +, U and ring blocks render correctly with `InnerCorner` (pass 1 added) | Q6 | Not started |
| 5 | Board walls | All four edges of the board closed with `Wall`, `Corner` on each outer corner | — | Not started |
| 6 | Doors | Colored `DoorPiece`s replace wall segments on an edge, arrows pointing out; placed side by side they form an N-cell door | — | Not started |
| 7 | Drag | A block can be selected and dragged, following the finger freely (no collision yet) | — | Not started |
| 8 | Grid collision | The block moves cell by cell and stops at other blocks and walls | Q5 | Not started |
| 9 | Axis lock | A locked block moves on one axis only, with an `Arrow` on top | — | Not started |
| 10 | Exit through door | A block reaching a matching-color door that is wide enough disappears | — | Not started |
| 11 | Win | A win message appears when all blocks have exited | — | Not started |

## 7. Deferred
- Holed and irregular board shapes (board mask).
- `WallNotch` and concave board edges.
- Time limit and all special mechanics of the original.

## 8. Open Questions

| # | Question | Needed by | Answer |
|---|---|---|---|
| Q1 | Are L/T blocks (`InnerCorner`) in V1 scope? | Phase 4 | **Answered:** yes, via `BlockDrawRule` (Phases 3–4). |
| Q2 | Level authoring format: inspector field list, ASCII text, or hand-placed in scene? (Fastest for a prototype: inspector field list.) | Phase 2 | **Answered:** inspector field list (`List<BlockData>` on `Board`). |
| Q3 | Which piece goes on the outer corners: `Corner_1`, `corner_4` or `corner_5`? | Phase 5 | **Answered:** `corner_5`, kept as the only `Corner`. |
| Q4 | Where should `Door_Arrow` be placed? | Phase 6 | **Answered:** on the door top, centered; shares the `Door` pivot. |
| Q5 | Drag style: free 2D, or one axis per step? Settle by trying it. | Phases 7–8 | — |
| Q6 | At root rotation 0, which Unity quadrant does a single `BlockPiece` cover, and which quadrant is `InnerCorner`'s empty one? Mesh data says they should be the same. Early screenshots showed top-right vs. bottom-right, possibly due to a Y rotation on the test object. | Phases 2–4 | — |
| Q7 | Does `Block_{id}` also follow the root → `Visuals` pattern, so block-wide effects (exit, selection) play on `Visuals` while the root keeps the logical position? | Phase 7 (drag) or Phase 10 (exit) | — |

## 9. FINDINGS.md Seed
When opening Phase 1, create `docs/prototype/FINDINGS.md` with the prototype question (section 1) and these lines:
- `Rule: GroundGrid, BlockParts and Arrows FBX are not modified; their fixes live on the prefab's Mesh level. WallAndDoor.fbx was cleaned once and needs no fix.`
- `Rule: Prefab = Root (grid position/rotation, gameplay) → Visuals (effects only) → Mesh (import fix; script swaps mesh/material only).`
- `Rule: Wall family shares one profile (1 wide, top +0.611, 12-segment bevel); Wall and Door run along X at rotation 0.`
- `Rule: Board edges: bottom 0, top 180, left 90, right 270. Corners: BL 0, BR 270, TR 180, TL 90. DoorArrow lives inside DoorPiece.`
- `Rule: Cell = 2 Unity units (4 quadrants, 4 studs per cell).`
- `Rule: Any polyomino block shape is in V1 scope, including L/T (Q1).`
- `Shape: BlockDrawRule = block cells → list of (mesh, local position, Y rotation); pass 1 vertices (InnerCorner), pass 2 quadrants (OuterCorner/Edge/Center). Reads only the block's own cells.`
- `Rule: Block and wall pivots sit at ground level; meshes extend below the ground. BlockPiece offset 0, ArrowPiece +0.656.`
- `Shape: Prefabs GroundTile, BlockPiece, ArrowPiece, WallPiece (Wall/Corner/WallNotch swap, fixed material), DoorPiece (Door + DoorArrow fixed, door color).`
- `Shape: WallNotch = 1-cell wall tooth; longer teeth add 2 Walls per cell. No T-junction piece.`
- `Shape: One GroundGrid tile = one cell = 4 BlockPiece quadrants; InnerCorner sits on the corner shared by 4 tiles.`
- `Shape: Single grid int[,] (blockId, -1 empty) + Block list (id, color, axisLock, cells) + Door list (side, index, length, color).`
- `Rejected: Physics colliders for movement — snap and tunneling problems; a grid check is enough.`
- `Rejected: Quadrant-resolution grid (2W×2H) — adds nothing to the logic; quadrants are visual only.`
- `Rejected: BlockPiece +0.656 offset — lifted the block off the ground; the "bottom rests on the ground" assumption was wrong.`
- `Rejected: Toggle prefab (all variants as children, enable one) — InnerCorner (2×2, on a vertex) does not fit a quadrant slot; leaves dead objects.`
- `Rejected: One prefab per piece — repeats the same orientation fix ~10 times.`
- `Rejected: Door inside WallPiece via mesh swap — DoorArrow had to be placed separately; DoorPiece places door + arrow in one step.`
- `Rejected: Corner_1, corner_4, corner_3 — same function as corner_5 with worse topology (corner_4 had a triangle fan) or redundant (corner_3 = Corner + 2 Walls).`
