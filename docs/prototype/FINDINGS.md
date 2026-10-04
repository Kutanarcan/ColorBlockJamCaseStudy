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

## Tuning

## Cost

## Rejected
- Tile gap via tile scale — writes scale on the root and drifts from the 2-unit kit.
- Extra cell gap (pitch = 2 + gap) — seams inside multi-cell blocks; the mesh's own 0.047 tile gap is enough, pitch stays 2.
- Physics colliders for movement — snap and tunneling problems; a grid check is enough.
- Quadrant-resolution grid (2W×2H) — adds nothing to the logic; quadrants are visual only.
- BlockPiece +0.656 offset — lifted the block off the ground; the "bottom rests on the ground" assumption was wrong.
- Toggle prefab (all variants as children, enable one) — InnerCorner (2×2, on a vertex) does not fit a quadrant slot; leaves dead objects.
- One prefab per piece — repeats the same orientation fix ~10 times.
- Door inside WallPiece via mesh swap — DoorArrow had to be placed separately; DoorPiece places door + arrow in one step.
- Corner_1, corner_4, corner_3 — same function as corner_5 with worse topology (corner_4 had a triangle fan) or redundant (corner_3 = Corner + 2 Walls).

## Shape
- BlockDrawRule = block cells → list of (mesh, local position, Y rotation); pass 1 vertices (InnerCorner), pass 2 quadrants (OuterCorner/Edge/Center). Reads only the block's own cells.
- Piece placement = rotation picks the wall/corner direction, pivot offset puts the footprint on its quadrant: pos = cellCenter + quadrant − R(θ)·(0.5, 0.5). Rotation alone reaches only 4 of the 8 Edge cases.
- Prefabs GroundTile, BlockPiece, ArrowPiece, WallPiece (Wall/Corner/WallNotch swap, fixed material), DoorPiece (Door + DoorArrow fixed, door color).
- WallNotch = 1-cell wall tooth; longer teeth add 2 Walls per cell. No T-junction piece.
- One GroundGrid tile = one cell = 4 BlockPiece quadrants; InnerCorner sits on the corner shared by 4 tiles.
- Single grid int[,] (blockId, -1 empty) + Block list (id, color, axisLock, cells) + Door list (side, index, length, color).
