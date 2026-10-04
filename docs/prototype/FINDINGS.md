# FINDINGS — PrototypeV1

**Question:** Can the existing model kit be used to build a level grid with blocks and doors, then move blocks and exit them through matching-color doors?

## Rule
- GroundGrid, BlockParts and Arrows FBX are not modified; their fixes live on the prefab's Mesh level. WallAndDoor.fbx was cleaned once and needs no fix.
- Prefab = Root (grid position/rotation, gameplay) → Visuals (effects only) → Mesh (import fix; script swaps mesh/material only).
- Wall family shares one profile (1 wide, top +0.611, 12-segment bevel); Wall and Door run along X at rotation 0.
- Board edges: bottom 0, top 180, left 90, right 270. Corners: BL 0, BR 270, TR 180, TL 90. DoorArrow lives inside DoorPiece.
- Cell = 2 Unity units (4 quadrants, 4 studs per cell).
- Any polyomino block shape is in V1 scope, including L/T (Q1).
- Block and wall pivots sit at ground level; meshes extend below the ground. BlockPiece offset 0, ArrowPiece +0.656.
- Prototype colors are runtime materials — new Material(template) per color, cached, shared by blocks and doors. Authored .mat assets are a production concern.
- Level authoring = inspector field list: `List<BlockData { color, cells }>` on `Board`; block id = list index (Q2).
- At rotation 0, OuterCorner / Edge / Center cover the pivot's top-right quadrant (+X, +Z); OuterCorner rounds toward (+X, +Z), Edge wall faces +Z (Q6).
- InnerCorner pivot = vertex; at rotation 0 its empty quadrant is (+X, +Z); placed by the cell diagonal to the empty cell (Q6).
- Exit width rule: a block exits only if every cell on the pushed side faces a Door edge of its color; one Wall edge or wrong color blocks it.
- Board edge table verified in Editor: Wall = 1-unit segments, 2 per cell edge, ring 0.5 outside the board; Corner rotations BL 0, BR 270, TR 180, TL 90.
- Adjacent same-color door edges merge into one door (logic and visuals). The original game shows no separate adjacent same-color doors.
- Border renders as runs: consecutive equal edges = one piece; Mesh_Wall scaled 2N, Mesh_Door scaled N, DoorArrow unscaled at the run center. Verified in Editor.
- Selection = ray onto the ground plane → floor(local / 2) → cells[x, y]; no colliders. Verified in Editor.
- Drag is free 2D (as in the original): the visual follows the pointer, clamped to reachable space; logic still walks cell by cell; release snaps to the nearest reachable cell (Q5).
- Step valid = every target cell inside + empty or own + crossed edge Open. Logic walks toward the pointer's rounded cell, larger axis first, other axis if blocked. Verified in Editor.
- Axis lock is enforced only in CanPlace (drag code untouched). Arrow: nearest block cell to the bounding-box center → unbroken run along the locked axis → Arrow_min(run, 3) at the run center. Arrow_N lies along Z at rotation 0 (arrowYawOffset = 90). Verified in Editor.
- Exit = a blocked step checks CanExit: block pressed against the border on that side, every covered column/row faces a Door edge of its color; then its cells clear and the root hides. Triggers mid-drag. Verified in Editor on a 6×8 level.

## Tuning
- Door arrow color = light (prefab default), `doorArrowUsesDoorColor = false`.
- Drag lift = 0.3 feels fine; original has almost none, 0 is a valid candidate.

## Cost
- `DoorData.length` is not bounds-checked; a door running past the board edge throws. Accepted in the prototype.
- Exit checks every covered column/row, so recess cells (e.g. a U open toward the door) must also face the door. Not seen in the original; accepted.
- Exit checks only the border edge, not the cells between a column and the border: a U exits through a block sitting in its recess. Accepted in the prototype; production exit must check those cells.
- CanPlace edge check is dormant (border is caught by bounds, inner edges all Open). Kept on purpose: walls inside the board are a planned feature.
- Diagonal CanPlace (only Lean uses it) skips the edge check, so with inner walls the visual could lean past a wall corner. Fine in the prototype; production moves on the grid and its logic must not allow it.
- Run stretching writes Mesh-level scale (prefab rule says the Mesh transform is fixed). Prototype shortcut; production needs another approach.

## Rejected
- Tile gap via tile scale — writes scale on the root and drifts from the 2-unit kit.
- Extra cell gap (pitch = 2 + gap) — seams inside multi-cell blocks; the mesh's own 0.047 tile gap is enough, pitch stays 2.
- Physics colliders for movement — snap and tunneling problems; a grid check is enough.
- One-axis-per-step drag with grid snap while held — the original game drags in free 2D.
- Quadrant-resolution grid (2W×2H) — adds nothing to the logic; quadrants are visual only.
- BlockPiece +0.656 offset — lifted the block off the ground; the "bottom rests on the ground" assumption was wrong.
- Toggle prefab (all variants as children, enable one) — InnerCorner (2×2, on a vertex) does not fit a quadrant slot; leaves dead objects.
- One prefab per piece — repeats the same orientation fix ~10 times.
- Door inside WallPiece via mesh swap — DoorArrow had to be placed separately; DoorPiece places door + arrow in one step.
- Corner_1, corner_4, corner_3 — same function as corner_5 with worse topology (corner_4 had a triangle fan) or redundant (corner_3 = Corner + 2 Walls).
- Door list `{ side, index, length, color }` — border-only and needs span math; per-edge doors make the width rule a per-cell check and allow doors on any edge.

## Shape
- BlockDrawRule = block cells → list of (mesh, local position, Y rotation); pass 1 vertices (InnerCorner), pass 2 quadrants (OuterCorner/Edge/Center). Reads only the block's own cells.
- Piece placement = rotation picks the wall/corner direction, pivot offset puts the footprint on its quadrant: pos = cellCenter + quadrant − R(θ)·(0.5, 0.5). Rotation alone reaches only 4 of the 8 Edge cases.
- Prefabs GroundTile, BlockPiece, ArrowPiece, WallPiece (Wall/Corner/WallNotch swap, fixed material), DoorPiece (Door + DoorArrow fixed, door color).
- WallNotch = 1-cell wall tooth; longer teeth add 2 Walls per cell. No T-junction piece.
- One GroundGrid tile = one cell = 4 BlockPiece quadrants; InnerCorner sits on the corner shared by 4 tiles.
- Runtime grid = cell layer `int[,] cells` (blockId, -1 empty) + edge layer `hEdges[W,H+1]`, `vEdges[W+1,H]` (Open / Wall / Door(color)) + Block list (id, color, axisLock, cells). Editor, JSON and visuals may differ but load into this.
- Door authoring: prototype uses `DoorData { cell, side, length, color }`. Production level editor and level data store doors per cell edge (one entry = one edge, deterministic, 1:1 with the edge layer); length stays a visual concern only.
