# Algorithm Explanation

## BlockDrawRule

Given a block's cells, decide which mesh piece goes where. Source: `Assets/Prototype/BlockDrawRule.cs`.

### The idea in one line
Every cell is split into 4 quadrants. Each quadrant looks at its neighbor cells and picks one piece: `OuterCorner`, `Edge`, `Center` or `InnerCorner`.

### Terms
- **Cell:** one grid square (2 × 2 units). A block is a list of cells.
- **Quadrant:** one quarter of a cell (1 × 1 unit). Each cell has 4: top-right, bottom-right, bottom-left, top-left.
- **Direction:** a quadrant written as a sign pair: top-right `(1, 1)`, bottom-right `(1, -1)`, bottom-left `(-1, -1)`, top-left `(-1, 1)`. The `Quadrants` array holds these four.
- **Corner point:** the outer corner of a quadrant. Up to 4 cells meet there.
- **Neighbors X / Z / Diag:** the 3 other cells around that corner point (see the diagram below).
- **Piece:** one mesh placed on the block: a kind, a position and a rotation.

### Neighbors of a quadrant
For the top-right quadrant (`q`) of a cell, the corner point is the `+` in the middle, and the 3 neighbors are:

```
+---------+---------+
|         |         |
|    Z    |  Diag   |
|         |         |
+---------+---------+
|         |         |
|  cell q |    X    |
|         |         |
+---------+---------+
```

- **X:** the cell beside the quadrant (right, for top-right)
- **Z:** the cell above or below the quadrant (above, for top-right)
- **Diag:** the cell across the corner point (up-right, for top-right)

For the other quadrants, the same picture is mirrored. The direction's signs tell which way to look.

### Step by step

1. **Store the cells in a `HashSet`.** The loop asks "is this neighbor part of the block?" many times. A `HashSet` answers that instantly.
2. **Create `pieces`.** This is the result list.
3. **Loop over every cell.**
4. **Loop over the cell's 4 quadrants**, using the `Quadrants` array.
5. **Look up the 3 neighbors** X, Z and Diag in that direction.
6. **Pick the piece:**

| Neighbors in the block | Piece | Rotation |
|---|---|---|
| X + Z, but no Diag (3 cells around the corner point) | `InnerCorner`, placed on the corner point | faces the empty Diag cell |
| neither X nor Z | `OuterCorner` | faces the quadrant's direction |
| both X and Z (and Diag) | `Center` | 0, it is flat |
| only one of X / Z | `Edge` | wall faces the empty side |

7. **Add the piece to `pieces`.** When every cell is done, return the list.

### Worked example: L shape
Block cells: `(0,0)`, `(0,1)`, `(1,0)`. Cell `(1,1)` is empty.
Each box is one cell split into its 4 quadrants. `.` = empty.

```
            x = 0        x = 1
          +---+---+    +---+---+
  y = 1   | O | O |    | . | . |
          | E | I |    | . | . |
          +---+---+    +---+---+
  y = 0   | E | I |    | I | O |
          | O | E |    | E | O |
          +---+---+    +---+---+

O = OuterCorner   E = Edge   C = Center   I = InnerCorner
```

How some quadrants get their piece:
- **`(0,0)` top-right:** X = `(1,0)` is in the block, Z = `(0,1)` is in the block, Diag = `(1,1)` is empty. Three cells meet at the corner point, so this cell places the `InnerCorner`.
- **`(0,1)` bottom-right and `(1,0)` top-left:** they touch the same corner point and also count 3 cells, but they are not diagonal to the empty cell. They skip, because the `InnerCorner` above already covers them. That is why the three `I`s are one mesh.
- **`(0,0)` bottom-left:** X = `(-1,0)` and Z = `(0,-1)` are both outside the block → `OuterCorner`.
- **`(0,0)` bottom-right:** X = `(1,0)` is in the block, Z = `(0,-1)` is not → `Edge`, wall facing down.
- There is no `C` here. A cell needs both neighbors and the diagonal on one corner to get a `Center`, which takes at least a 2 × 2 block.

### Two details

- **InnerCorner is one mesh for 3 quadrants.** Only the cell diagonal to the empty one places it. The other two skip that quadrant (`continue`), so the piece is placed once.
- **Position needs an offset, not just rotation.** At rotation 0, a piece sits in the top-right quadrant of its pivot. An `Edge` can be needed in any quadrant with its wall on either outer side, which is 8 cases. Rotation alone reaches only 4 of them, so `PivotOffset` shifts the piece into its quadrant after rotating.

### Cost
- **Time:** O(n) for n cells (4 quadrants × 3 `HashSet` lookups per cell).
- **Memory:** one `HashSet` plus the result list, about 4n pieces.

### Edge cases
- **Cells touching only diagonally:** only 2 cells around the corner point, so each gets an `OuterCorner` there. They look like separate cells.
- **Ring with a hole:** each of the 4 corner points around the hole has 3 cells, giving 4 `InnerCorner`s.
- **Full 2 × 2:** 4 cells around the middle corner point, so all four quadrants there become `Center`.
- **Assumes no gap between cells:** pieces are exactly 1 unit and `InnerCorner` spans 3 cells, so any gap shows a seam.

---

## Movement

Can a block move by one cell, and if so, move it. Source: `Board.CanPlace`, `Board.TryStep`.

### The idea in one line
A block moves one cell at a time. A step is allowed when every cell of the block lands on a free spot and crosses an open edge.

### Terms
- **Delta:** how far to move, in cells. A **step** is a delta of exactly one cell on one axis: `(1,0)`, `(-1,0)`, `(0,1)`, `(0,-1)`.
- **Own cell:** a cell that already belongs to the moving block. It counts as free, because the block leaves it as it moves.
- **Crossed edge:** the edge between a cell and the cell it steps into (see Edge layer).

### Step by step: `CanPlace(id, delta)`
1. **Axis lock.** A `Horizontal` block rejects any Y movement; a `Vertical` block rejects any X movement.
2. **Loop over every cell `c` of the block**, and let `n = c + delta`.
3. **Inside the board?** If `n` is outside, reject.
4. **Free?** `cells[n]` must be `-1` (empty) or the block's own id. Otherwise reject.
5. **Edge open?** Only for a step: the edge between `c` and `n` must be `Open`. `Wall` and `Door` reject.
6. All cells passed → allowed.

### Step by step: `TryStep(id, dir)`
1. Already exited → do nothing.
2. `CanPlace` fails → try `CanExit` (see Exit). Either way, return `false`.
3. **Clear all** old cells to `-1`, **then write all** new cells with the id.
4. Add `dir` to `blockOffsets[id]`.

### Worked example: 1×2 bar stepping right
Block 4 = `(1,5)`, `(2,5)`. Step `(1,0)`.

```
before          after
. R R .         . . R R
  1 2 3           1 2 3
```

- `(1,5)` → `(2,5)`: it is block 4's own cell → free.
- `(2,5)` → `(3,5)`: empty → free.
- Clear `(1,5)` and `(2,5)`, then write `(2,5)` and `(3,5)`.

### Two details
- **Clear first, write second.** If the move cleared and wrote one cell at a time, writing `(2,5)` and then clearing it as an old cell would erase the block from a cell it still covers.
- **Own cells count as free.** Without this, any block longer than one cell could never move along its length.

### Cost
- **Time:** O(n) per step for n block cells.
- **Memory:** none; the cell list is moved in place.

### Edge cases
- **Diagonal delta skips the edge check.** Only `Lean` asks for diagonals. Today every inner edge is `Open`, so this is harmless in the prototype; with inner walls a diagonal lean could visually pass a wall corner. Production moves on the grid, and logic authored there must not allow a diagonal past a wall corner.
- **The edge check is effectively dormant.** Border edges are never reached because `n` is out of bounds first, and every inner edge is `Open`. Walls inside the board are a planned future feature; the check is kept for them and starts to matter once they exist.
- **`blocks[id].cells` is changed in place.** Play mode reverts it on stop; a runtime level reload would not.

---

## Exit

When does a blocked step make the block leave the board? Source: `Board.CanExit`, `Board.BorderEdge`, `Board.Exit`.

### The idea in one line
If a step toward the border is blocked, check whether every column (or row) the block covers faces a door of its own color on that border.

### Terms
- **Pressed:** at least one cell of the block is on the border row/column in the push direction, so the next step would leave the board.
- **Border edge:** the edge on the board's border in the push direction, on the cell's column (pushing up or down) or row (pushing left or right).

### Step by step: `CanExit(id, dir)`
1. **Axis lock.** Same check as `CanPlace`; a locked block cannot exit sideways.
2. **Loop over every cell `c`.**
3. If `c + dir` is outside the board, mark **pressed**.
4. Read the border edge on `c`'s column or row. If it is not the block's color, reject.
5. Return **pressed**.

### Step by step: `Exit(id, dir)`
1. Clear the block's cells to `-1`. From here the block cannot be clicked (`BlockAt` returns `-1`).
2. Mark `exited[id]`.
3. Find a world point on the border line and the exit direction in world space.
4. Hand off to `BlockExit.Play` for the visual (see Exit visual). The root is disabled when it finishes.

### Worked example: blue T pushed down
Blue T = `(2,1)`, `(3,1)`, `(4,1)`, `(3,2)`. Bottom door: blue, x = 2–4.

```
y2  .  .  .  B  .  .
y1  .  .  B  B  B  .
y0  .  .  .  .  .  .
    ----[ blue  ]----
    0  1  2  3  4  5
```

- First drag down: `(2,0)`, `(3,0)`, `(4,0)`, `(3,1)` are free → normal step.
- Next step down: `(2,-1)` is out of bounds → `CanPlace` fails → `CanExit`.
- Cells `(2,0)`, `(3,0)`, `(4,0)` are on the border → pressed.
- Columns 2, 3 and 4 (cell `(3,1)` also checks column 3) all face blue → exit.
- With the door only at x = 2–3, column 4 would face a `Wall` → no exit, the block just stops.

### Two details
- **Exit only runs on a blocked step.** There is no separate "exit" input; the same drag that moves a block also pushes it out.
- **Why `pressed` is needed.** Without it, a block stopped by another block in the middle of the board would exit as long as its columns face a matching door.

### Cost
- **Time:** O(n) for n block cells; each border lookup is O(1).

### Edge cases
- **Recess cells.** Every covered column is checked, not only the front cells. A U shape open toward the door needs the door under the recess column too.
- **Exit through a block in a recess.** Only the border edge is checked, not the cells between the block and the border. A block sitting inside a U's recess would be "passed through": the U exits while the other block stays. Known bug, accepted in the prototype; production exit must check the cells between every column and the border.
- **Exit mid-drag.** `Drag` sees `IsExited` and drops the hold; the rest of that press does nothing, and no Drop sound plays.
- **Logic finishes before the visual.** Cells are free while the block is still sliding into the door; another block may move into them during the short animation.

---

## Drag pipeline

Turn the pointer into block movement every frame. Source: `Drag.cs`, `DragCollision.cs`, `Placement.cs`.

### The idea in one line
The logic walks the block cell by cell toward the pointer; the visual leans up to half a cell further, then eases onto its cell on release.

### Terms
- **Offset:** how many cells the block has moved since it was built (`blockOffsets[id]`). All drag math is in offsets, not world units.
- **Pointer:** where the finger wants the block, as an offset with fractions, e.g. `(2.3, -0.6)`.
- **Target:** the pointer rounded to whole cells.
- **Lean:** the small visual shift from the block's cell toward the pointer, at most ±0.5 per axis.
- **Settle:** the ease from the lifted, leaning position onto the cell after release.

### Pipeline per frame
```
mouse → ray on ground plane → pointer (cells)
      → Walk(target)        logic: cells move, may exit
      → Lean(pointer)       visual offset, clamped
      → Placement.Hold      root position + lift
```

### Step by step
1. **Grab (mouse down).** Ray onto the ground plane, `BlockAt` → block id. Remember the hit point and the block's offset. Any block still settling is snapped onto its cell first. Plays the Select sound.
2. **Pointer.** `pointer = grabOffset + (hit − grabHit) / CellSize`, in the board's local space.
3. **Walk** toward `target = round(pointer)`, at most `maxStepsPerFrame` steps:
   - Remaining distance `rem = target − offset`. Zero → done.
   - Try a step on the axis with the larger `|rem|` (X wins a tie).
   - Blocked → try the other axis.
   - Both blocked → stop for this frame.
4. **Exited?** Drop the hold and return.
5. **Lean:**
   - `f = pointer − offset`, each axis clamped to ±0.5.
   - If a step in `f`'s X direction is not allowed, `f.x = 0`. Same for Y.
   - If both are non-zero and the diagonal is not allowed, drop the smaller axis.
6. **Hold.** Root goes to `offset + f`, lifted by `lift`, eased by `followSpeed` (0 = instant).
7. **Release (mouse up).** Settle onto the current offset with `snapSpeed`. Plays the Drop sound. A block that exited in step 4 is no longer held, so it never reaches this step.

### Worked example: pointer behind a block
Red bar at offset `(0,0)`, another block one cell to its right. Pointer at `(1.4, 0.2)`.

- Target `(1, 0)`. X step blocked, Y remaining is 0 → Walk stops.
- Lean: `f = (1.4, 0.2)` → clamped `(0.5, 0.2)` → X step blocked → `(0, 0.2)`. Y step up allowed → stays.
- The block stays on its cell, leans slightly up, and does not slide into the neighbor.

### Two details
- **Logic and visual are separate.** Collision only ever sees whole cells. The half-cell lean is only on screen, so the block never visually enters a cell the logic refused.
- **The ease is frame-rate independent.** `Lerp(from, to, 1 − e^(−speed·dt))` gives the same feel at 30 and 60 fps; a plain `Lerp(…, speed·dt)` would not.

### Cost
- **Walk:** up to `maxStepsPerFrame` × O(n) per frame.
- **Lean:** up to 3 `CanPlace` calls, O(n) each.
- No colliders, no per-frame allocation.

### Edge cases
- **Fast flick.** More than `maxStepsPerFrame` cells in one frame → the block catches up over the next frames.
- **Routing around obstacles.** Larger axis first, other axis when blocked: a block can slide around a corner instead of stopping. This matches the original's feel.
- **Ray parallel to the ground.** No hit → the frame is skipped.

---

## Exit visual

Make an exited block look like it slides into the door. Source: `BlockExit.cs`, `BlockClip.shader`, `BlockExitParticles.cs`.

### The idea in one line
Slide the block through the door and let the shader throw away every pixel past a plane inside the door.

### Terms
- **Cut plane:** a world-space plane `(normal, distance)` inside the door. Pixels on its far side are not drawn.
- **Front row:** the block cells nearest the door. Rows are counted along the exit direction, front = 0.
- **Depth:** the number of rows along the exit direction.
- **Cap shade:** how dark the inside of the block looks through the cut.

### Step by step: `BlockExit.Play`
1. **Rows.** Group the block's cells by `dot(cell, dir)`; the largest value is row 0 (front).
2. **Cut plane.** Border point + `clipOffset` along the exit normal (0.5 = door center). `distance = dot(cutPoint, normal)`.
3. **Apply it to this block only.** Put the plane in a `MaterialPropertyBlock` on every renderer under the root (pieces and arrow). Color materials are shared, so writing the material would cut every block of that color.
4. **Sequence (DOTween):**
   - **Snap:** move onto the cell in front of the door in `snapDuration` (drops the drag lift and lean).
   - **Sound:** Crunch, once.
   - **Slide:** move `depth · cellSize + clipOffset` along the normal at `speed`. That is exactly how far the rear edge is from the cut.
   - **Every frame of the slide:** `traveled = dot(root − rest, normal)`. Row `k` bursts when `traveled ≥ k · cellSize + cellSize / 2 + clipOffset`, which is when its center is on the cut line.
   - **Complete:** burst any row not yet burst, disable the root.

### Step by step: `BlockClip.shader`
1. `clip(distance − dot(worldPos, normal))`: drop pixels past the plane. A zero plane `(0,0,0,0)` keeps everything, so normal play is unaffected.
2. **Front face:** lit like Standard (color, texture, smoothness, metallic).
3. **Back face** (`Cull Off`, `VFACE < 0`): the inside of the block, seen only through the cut. Drawn flat and unlit as `color × _CapShade`.
4. `addshadow`: the shadow pass clips too, so the cut part casts no shadow.

### Worked example: blue T through the bottom door
Cells at exit: `(2,0)`, `(3,0)`, `(4,0)`, `(3,1)`. Exit dir `(0,-1)`.

```
y1  .  .  .  B  .  .     row 1 (1 cell)
y0  .  .  B  B  B  .     row 0 (3 cells)
    ----[ blue  ]----    border z = 0, cut z = -0.5
```

- Normal = `(0,0,-1)`, cut point z = −0.5 → `distance = 0.5`. Pixels with z < −0.5 are dropped.
- Depth = 2 → slide `2 · 2 + 0.5 = 4.5` units, `0.45 s` at speed 10.
- Row 0 center starts at z = 1 → reaches the cut after `1.5` → first burst (3 cells).
- Row 1 center starts at z = 3 → reaches the cut after `3.5` → second burst (1 cell).

### Two details
- **The clip plane solves InnerCorner for free.** Hiding meshes row by row breaks on `InnerCorner`, which spans two rows. A per-pixel cut does not care which mesh a pixel belongs to.
- **Bursts are timed by distance, not time.** Any `ease` keeps the bursts on the cut line.

### Cost
- **Per exit:** one `MaterialPropertyBlock`, row lists, a DOTween sequence, one particle instance per row. Prototype allocation; production would pool these.
- **Per frame:** one dot product in `OnUpdate`.
- **Rendering:** the exiting block loses batching while it carries a property block.

### Edge cases
- **Particles use Standard.** It ignores particle vertex color, so `BlockExitParticles` tints each spawned copy with a `MaterialPropertyBlock`. Color over Lifetime has no effect.
- **Door height.** The door top (+0.611) is below the block top (+0.656). `doorHeightScale` can raise it to hide the cut edge; at 1 the shaded cut face is enough.
- **Two blocks exiting at once.** Each has its own plane, sequence and particles; sounds overlap via `PlayOneShot`.

---

## Edge layer

Where walls and doors live in the logic. Source: `Board.BuildEdges`, `Board.EdgeBetween`.

### The idea in one line
Walls and doors are not cells; they sit on the lines between cells, stored in two arrays.

### Terms
- **hEdges[x, y]:** the horizontal edge **below** cell `(x, y)`. Size `[W, H+1]`: H+1 lines for H rows.
- **vEdges[x, y]:** the vertical edge **left of** cell `(x, y)`. Size `[W+1, H]`: W+1 lines for W columns.
- **Values:** `-1` Open, `-2` Wall, `≥ 0` Door of that color.

### The four edges of a cell
```
          hEdges[x, y+1]
         +--------------+
         |              |
vEdges   |   cell x,y   |   vEdges
[x, y]   |              |   [x+1, y]
         +--------------+
           hEdges[x, y]
```

### Step by step: `BuildEdges`
1. Every border edge = `Wall` (`y == 0` or `y == H` for hEdges, `x == 0` or `x == W` for vEdges).
2. Every inner edge = `Open`.
3. Each door writes its color into `length` consecutive edges:
   - Bottom → `hEdges[x+i, y]`, Top → `hEdges[x+i, y+1]`
   - Left → `vEdges[x, y+i]`, Right → `vEdges[x+1, y+i]`

### Two details
- **Below and left are the "own" edges.** The edge above a cell is the edge below the next cell, so each edge is stored once.
- **A door is just edges with a color.** The width rule becomes a per-edge check (see Exit); no span math.

### Cost
- **Memory:** `W(H+1) + (W+1)H` ints.
- **Lookup:** O(1).

### Edge cases
- **`DoorData.length` is not bounds-checked.** A door running past the board throws.
- **Inner walls and doors** fit the same arrays but are not drawn yet (deferred).

---

## Coordinates

Short reference used by the sections above. Source: `Board.cs`.

| What | Formula |
|---|---|
| Board origin | bottom-left corner of cell `(0,0)` |
| Cell center | `(2x + 1, 0, 2y + 1)` |
| World → cell | `floor(local.x / 2)`, `floor(local.z / 2)` |
| Block root position | `offset × 2` (the root is built at the board origin; pieces sit at their cell positions inside it) |
| Board Y ↔ world Z | cell `y` runs along world `+Z` |

- **`blockOffsets` vs `cells`:** both move together. `cells` says which cells the block covers now; the offset says how far the root moved, so the visual never needs re-dressing.
- **Fractional offsets** exist only in the visual (`Lean`, settle). Logic is always whole cells.

---

## Border runs

Draw the board's border from the edge layer with as few pieces as possible. Source: `Board.BuildWalls`, `SpawnSide`, `SpawnWallRun`, `SpawnDoorRun`.

### The idea in one line
Walk along each side; consecutive equal edges form a run, and each run is one stretched piece.

### Step by step
1. Copy each side's edges into an array (bottom, top, left, right).
2. `SpawnSide` walks the array:
   - `start` = first edge of a run; move `end` while the value stays the same.
   - Run length `N = end − start`, center = `origin + along × (start + N/2) × 2`.
   - `Wall` → `WallPiece`, mesh scaled to `2N` (the mesh is 1 unit long).
   - Door → `DoorPiece`, mesh scaled to `N` (the mesh is 2 units long). The arrow is not scaled and stays at the center.
3. Place the 4 corners: BL 0, BR 270, TR 180, TL 90.

### Worked example: bottom side of the 6×8 level
Edges: `orange, wall, blue, blue, blue, wall`.

```
[O][ wall ][   blue   ][ wall ]
 0    1      2  3  4      5
```

→ 4 pieces: door N=1, wall N=1 (scale 2), door N=3 (one arrow at x=3), wall N=1.

### Two details
- **Same-color neighbors merge.** Two adjacent blue door edges become one 2-cell door with one arrow, in logic and visuals.
- **Side rotation does the outward flip.** Top uses 180, so the door arrow points out of the board on every side without extra code.

### Cost
- **Time:** O(W + H).
- **Pieces:** one per run plus 4 corners.

### Edge cases
- **Scale on the Mesh level.** The prefab rule says that transform is fixed; prototype shortcut (logged in FINDINGS).
- **An `Open` border edge is not drawn**, leaving a gap. Not possible today because borders default to `Wall`.

---

## Arrow placement

Put a fitting arrow on an axis-locked block. Source: `Board.SpawnArrow`.

### The idea in one line
Find the block's most central cell, measure the straight run through it along the locked axis, and place `Arrow_min(run, 3)` at that run's center.

### Step by step
1. Compute the bounding box and its center (in cell units).
2. **Anchor** = the block cell whose center is nearest to the bounding-box center (the first cell wins a tie).
3. From the anchor, walk backward and forward along the locked axis while the cells are in the block → `runStart`, `runEnd`.
4. `run = runEnd − runStart + 1`. Mesh = `Arrow_min(run, 3)`.
5. Position = center of the run. Rotation = 0 for Horizontal, 90 for Vertical, plus `arrowYawOffset`.

### Worked example: vertical L
Cells `(0,0)`, `(0,1)`, `(0,2)`, `(1,0)`. Lock: Vertical.

```
y2  #  .
y1  #  .      bounding box x 0–1, y 0–2 → center (1, 1.5)
y0  #  #
    0  1
```

- Distances² from the center: `(0,1)` → 0.25; all others → 1.25. Anchor = `(0,1)`.
- Run up/down from `(0,1)`: `(0,0)` to `(0,2)` → run = 3 → `Arrow_3`.
- Center of the run: `(1, 0, 3)` in local units.

### Two details
- **Why not the bounding box.** The first version sized the arrow from the bounding box; on an L it reached over the empty cell.
- **Why the nearest cell.** The bounding-box center of a concave shape can be an empty cell; snapping to the nearest block cell keeps the arrow on the block.

### Cost
- **Time:** O(n) for n cells (one pass for the box, one for the anchor, the run walk is ≤ n).

### Edge cases
- **Run longer than 3.** The arrow stays `Arrow_3` and is centered on the run, shorter than the block.
- **Ties.** An even-sized shape (e.g. 2 × 2) has several equally near cells; the first in the list wins, so the arrow may sit off-center.
