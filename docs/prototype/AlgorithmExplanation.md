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
