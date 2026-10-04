using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    // Horizontal = moves along X only, Vertical = along Y (board Z) only.
    public enum AxisLock { None, Horizontal, Vertical }

    [Serializable]
    public class BlockData
    {
        public int color;
        public AxisLock axisLock;
        public List<Vector2Int> cells = new List<Vector2Int>();
    }

    public enum Side { Bottom, Top, Left, Right }

    // One door = starts at `cell` on `side`, runs `length` cells (+X on Bottom/Top, +Y on Left/Right).
    // At runtime it is expanded into one Door edge per cell.
    [Serializable]
    public class DoorData
    {
        public Vector2Int cell;
        public Side side;
        public int length = 1;
        public int color;
    }

    public class Board : MonoBehaviour
    {
        const float CellSize = 2f;

        // Edge layer values: >= 0 is a door of that color.
        public const int EdgeOpen = -1;
        public const int EdgeWall = -2;

        [SerializeField] int width = 6;
        [SerializeField] int height = 8;
        [SerializeField] GameObject groundTilePrefab;

        [Header("Blocks")]
        [SerializeField] GameObject blockPiecePrefab;
        [SerializeField] Mesh outerCornerMesh;
        [SerializeField] Mesh edgeMesh;
        [SerializeField] Mesh centerMesh;
        [SerializeField] Mesh innerCornerMesh;
        [SerializeField] GameObject arrowPiecePrefab;
        [SerializeField] Mesh[] arrowMeshes = new Mesh[3]; // Arrow_1, Arrow_2, Arrow_3
        [SerializeField] float arrowYawOffset = 0f;        // set to 90 if Arrow_N lies along Z at rotation 0
        [SerializeField] Color[] palette =
        {
            new Color(0.91f, 0.27f, 0.27f), // red
            new Color(0.25f, 0.52f, 0.95f), // blue
            new Color(0.30f, 0.80f, 0.35f), // green
            new Color(0.98f, 0.82f, 0.20f), // yellow
            new Color(0.62f, 0.36f, 0.88f), // purple
            new Color(0.98f, 0.55f, 0.18f), // orange
        };

        [Header("Walls")]
        [SerializeField] GameObject wallPiecePrefab;
        [SerializeField] Mesh wallMesh;
        [SerializeField] Mesh cornerMesh;
        [SerializeField] GameObject doorPiecePrefab;
        [SerializeField] bool doorArrowUsesDoorColor = false;
        [SerializeField] float doorHeightScale = 1f; // > 1 makes the door taller than the block top so it hides the exit cut
        const float DoorTop = 0.611f;

        [Header("Level")]
        [SerializeField] List<BlockData> blocks = new List<BlockData>
        {
            new BlockData { color = 0, cells = new List<Vector2Int> { new Vector2Int(2, 3) } },
        };
        [SerializeField] List<DoorData> doors = new List<DoorData>();

        [Header("Input")]
        [SerializeField] Drag drag = new Drag();

        [Header("Exit")]
        [SerializeField] BlockExit exit = new BlockExit();

        [Header("Recording")]
        [SerializeField] HandCursor handCursor = new HandCursor();

        const string BlockClipShader = "Prototype/BlockClip";
        Material arrowClipMaterial;

        int[,] cells;  // [W, H]: blockId, -1 = empty
        int[,] hEdges; // [W, H+1]: hEdges[x, y] = edge below cell (x, y)
        int[,] vEdges; // [W+1, H]: vEdges[x, y] = edge left of cell (x, y)
        readonly List<Transform> blockRoots = new List<Transform>();
        Vector2Int[] blockOffsets;
        bool[] exited;

        Transform tiles;
        Transform walls;
        readonly Dictionary<int, Material> colorMaterials = new Dictionary<int, Material>(); // doors: template shader
        readonly Dictionary<int, Material> blockMaterials = new Dictionary<int, Material>(); // blocks: BlockClip shader

        Material BlockTemplate => blockPiecePrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial;

        void Start()
        {
            BuildEdges();
            BuildTiles();
            BuildWalls();
            BuildBlocks();
            // 2D source for UI-like sounds; added at runtime so the scene needs no extra component.
            var audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            drag.Init(this, Camera.main, audioSource);
            exit.Init(audioSource);
            handCursor.Init();
        }

        void Update()
        {
            drag.Tick();
            handCursor.Tick();
        }

        // World point -> blockId on that cell, -1 if empty or off the board.
        public int BlockAt(Vector3 worldPos)
        {
            var local = transform.InverseTransformPoint(worldPos);
            int x = Mathf.FloorToInt(local.x / CellSize);
            int y = Mathf.FloorToInt(local.z / CellSize);

            if (x < 0 || y < 0 || x >= width || y >= height) return -1;
            return cells[x, y];
        }

        public Transform BlockRoot(int id) => blockRoots[id];

        public float CellWorldSize => CellSize;

        // How many cells the block has moved from where it was built.
        public Vector2Int BlockOffset(int id) => blockOffsets[id];

        // Offset in cells (fractions allowed) -> world position of the block root.
        public Vector3 OffsetToWorld(Vector2 offset) =>
            transform.TransformPoint(new Vector3(offset.x * CellSize, 0f, offset.y * CellSize));

        // Could the block sit at its current cells + delta? Every target cell inside the board and empty or its own.
        // For a one-cell orthogonal step, the crossed edge must also be Open (not Wall or Door).
        // An axis-locked block never moves along its locked axis.
        public bool CanPlace(int id, Vector2Int delta)
        {
            var axisLock = blocks[id].axisLock;
            if (axisLock == AxisLock.Horizontal && delta.y != 0) return false;
            if (axisLock == AxisLock.Vertical && delta.x != 0) return false;

            bool step = Mathf.Abs(delta.x) + Mathf.Abs(delta.y) == 1;

            foreach (var c in blocks[id].cells)
            {
                var n = c + delta;
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) return false;
                if (cells[n.x, n.y] != -1 && cells[n.x, n.y] != id) return false;
                if (step && EdgeBetween(c, delta) != EdgeOpen) return false;
            }

            return true;
        }

        public bool IsExited(int id) => exited[id];

        // One cell step: check, then move the block in the cell layer.
        // A blocked step toward a matching, wide-enough door exits the block instead (returns false; check IsExited).
        public bool TryStep(int id, Vector2Int dir)
        {
            if (exited[id]) return false;

            if (!CanPlace(id, dir))
            {
                if (CanExit(id, dir)) Exit(id, dir);
                return false;
            }

            var body = blocks[id].cells;
            foreach (var c in body) cells[c.x, c.y] = -1;
            for (int i = 0; i < body.Count; i++)
            {
                body[i] += dir;
                cells[body[i].x, body[i].y] = id;
            }

            blockOffsets[id] += dir;
            return true;
        }

        // Width rule: the block is pressed against the border in dir, and every column (or row) it covers
        // faces a Door of its own color on that border. One Wall or wrong-color edge blocks the exit.
        bool CanExit(int id, Vector2Int dir)
        {
            var data = blocks[id];
            if (data.axisLock == AxisLock.Horizontal && dir.y != 0) return false;
            if (data.axisLock == AxisLock.Vertical && dir.x != 0) return false;

            bool pressed = false;
            foreach (var c in data.cells)
            {
                var n = c + dir;
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) pressed = true;
                if (BorderEdge(c, dir) != data.color) return false;
            }

            return pressed;
        }

        // The border edge in dir on the cell's column (or row).
        int BorderEdge(Vector2Int c, Vector2Int dir)
        {
            if (dir.x > 0) return vEdges[width, c.y];
            if (dir.x < 0) return vEdges[0, c.y];
            if (dir.y > 0) return hEdges[c.x, height];
            return hEdges[c.x, 0];
        }

        // Free the block's cells at once (no longer clickable), then play the visual exit through the door.
        void Exit(int id, Vector2Int dir)
        {
            var data = blocks[id];
            foreach (var c in data.cells) cells[c.x, c.y] = -1;
            exited[id] = true;

            // A point on the border line the block is pushed through.
            var edgeLocal = new Vector3(dir.x > 0 ? width * CellSize : 0f, 0f, dir.y > 0 ? height * CellSize : 0f);
            var normal = transform.TransformDirection(new Vector3(dir.x, 0f, dir.y)).normalized;

            exit.Play(blockRoots[id], data.cells, blockOffsets[id], dir, palette[data.color], CellSize,
                OffsetToWorld(blockOffsets[id]), normal, transform.TransformPoint(edgeLocal));
        }

        int EdgeBetween(Vector2Int c, Vector2Int dir)
        {
            if (dir.x > 0) return vEdges[c.x + 1, c.y];
            if (dir.x < 0) return vEdges[c.x, c.y];
            if (dir.y > 0) return hEdges[c.x, c.y + 1];
            return hEdges[c.x, c.y];
        }

        // Border edges start as Wall, inner edges as Open; doors overwrite their edge with the door color.
        void BuildEdges()
        {
            hEdges = new int[width, height + 1];
            vEdges = new int[width + 1, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y <= height; y++)
                hEdges[x, y] = (y == 0 || y == height) ? EdgeWall : EdgeOpen;

            for (int x = 0; x <= width; x++)
            for (int y = 0; y < height; y++)
                vEdges[x, y] = (x == 0 || x == width) ? EdgeWall : EdgeOpen;

            foreach (var d in doors)
            {
                for (int i = 0; i < d.length; i++)
                {
                    int x = d.cell.x, y = d.cell.y;
                    switch (d.side)
                    {
                        case Side.Bottom: hEdges[x + i, y] = d.color; break;
                        case Side.Top: hEdges[x + i, y + 1] = d.color; break;
                        case Side.Left: vEdges[x, y + i] = d.color; break;
                        case Side.Right: vEdges[x + 1, y + i] = d.color; break;
                    }
                }
            }
        }

        void BuildTiles()
        {
            tiles = new GameObject("Tiles").transform;
            tiles.SetParent(transform, false);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var tile = Instantiate(groundTilePrefab, tiles);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.localPosition = CellCenter(x, y);
            }
        }

        // Border edges are drawn from the edge layer, in a 1-unit ring outside the board.
        // Consecutive edges with the same value form a run; each run is ONE piece stretched along its length.
        // Rotations: bottom 0, top 180, left 90, right 270. Corners: BL 0, BR 270, TR 180, TL 90.
        // Inner edges are not drawn yet (deferred).
        void BuildWalls()
        {
            walls = new GameObject("Walls").transform;
            walls.SetParent(transform, false);

            float w = width * CellSize;
            float h = height * CellSize;

            var bottom = new int[width];
            var top = new int[width];
            for (int x = 0; x < width; x++) { bottom[x] = hEdges[x, 0]; top[x] = hEdges[x, height]; }

            var left = new int[height];
            var right = new int[height];
            for (int y = 0; y < height; y++) { left[y] = vEdges[0, y]; right[y] = vEdges[width, y]; }

            SpawnSide(bottom, new Vector3(0f, 0f, -0.5f), Vector3.right, 0f, "Bottom");
            SpawnSide(top, new Vector3(0f, 0f, h + 0.5f), Vector3.right, 180f, "Top");
            SpawnSide(left, new Vector3(-0.5f, 0f, 0f), Vector3.forward, 90f, "Left");
            SpawnSide(right, new Vector3(w + 0.5f, 0f, 0f), Vector3.forward, 270f, "Right");

            SpawnCorner(new Vector3(-0.5f, 0f, -0.5f), 0f, "Corner_BL");
            SpawnCorner(new Vector3(w + 0.5f, 0f, -0.5f), 270f, "Corner_BR");
            SpawnCorner(new Vector3(w + 0.5f, 0f, h + 0.5f), 180f, "Corner_TR");
            SpawnCorner(new Vector3(-0.5f, 0f, h + 0.5f), 90f, "Corner_TL");
        }

        // edges = one value per cell along this side. origin = where the side starts, along = its direction.
        void SpawnSide(int[] edges, Vector3 origin, Vector3 along, float yRot, string sideName)
        {
            int start = 0;
            while (start < edges.Length)
            {
                int end = start;
                while (end < edges.Length && edges[end] == edges[start]) end++;

                int cells = end - start;
                var center = origin + along * ((start + cells * 0.5f) * CellSize);
                string runName = $"{sideName}_{start}_{cells}";

                if (edges[start] == EdgeWall) SpawnWallRun(center, yRot, cells, $"Wall_{runName}");
                else if (edges[start] >= 0) SpawnDoorRun(center, yRot, cells, edges[start], $"Door_{runName}");

                start = end;
            }
        }

        // Prototype shortcut: stretch the Mesh child along X. Wall mesh is 1 unit long, so N cells = 2N.
        void SpawnWallRun(Vector3 localPos, float yRot, int cells, string pieceName)
        {
            var piece = SpawnRoot(wallPiecePrefab, localPos, yRot, pieceName);
            var mesh = piece.transform.Find("Visuals/Mesh_Wall");
            mesh.GetComponent<MeshFilter>().sharedMesh = wallMesh;
            mesh.localScale = new Vector3(cells * CellSize, 1f, 1f);
        }

        // Door mesh is 2 units long, so N cells = N. The arrow is not stretched and stays at the run's center.
        void SpawnDoorRun(Vector3 localPos, float yRot, int cells, int color, string pieceName)
        {
            var piece = SpawnRoot(doorPiecePrefab, localPos, yRot, pieceName);
            var door = piece.transform.Find("Visuals/Mesh_Door");
            door.localScale = new Vector3(cells, doorHeightScale, 1f);

            // Keep the arrow on the (taller) door top: door top is +0.611 at scale 1.
            var arrow = piece.transform.Find("Visuals/Mesh_DoorArrow");
            arrow.localPosition += Vector3.up * (DoorTop * (doorHeightScale - 1f));
            door.GetComponent<MeshRenderer>().sharedMaterial = ColorMaterial(color, BlockTemplate);

            if (doorArrowUsesDoorColor)
                piece.transform.Find("Visuals/Mesh_DoorArrow").GetComponent<MeshRenderer>().sharedMaterial = ColorMaterial(color, BlockTemplate);
        }

        void SpawnCorner(Vector3 localPos, float yRot, string pieceName)
        {
            var piece = SpawnRoot(wallPiecePrefab, localPos, yRot, pieceName);
            piece.GetComponentInChildren<MeshFilter>().sharedMesh = cornerMesh;
        }

        GameObject SpawnRoot(GameObject prefab, Vector3 localPos, float yRot, string pieceName)
        {
            var piece = Instantiate(prefab, walls);
            piece.name = pieceName;
            piece.transform.localPosition = localPos;
            piece.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
            return piece;
        }

        void BuildBlocks()
        {
            cells = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y] = -1;

            // Note: TryStep moves blocks[id].cells in place; Play mode reverts it on exit.
            blockOffsets = new Vector2Int[blocks.Count];
            exited = new bool[blocks.Count];

            for (int id = 0; id < blocks.Count; id++)
            {
                var data = blocks[id];
                var root = new GameObject($"Block_{id}").transform;
                root.SetParent(transform, false);
                blockRoots.Add(root);

                foreach (var c in data.cells)
                    cells[c.x, c.y] = id;

                var pieces = BlockDrawRule.Build(data.cells, CellSize);
                for (int i = 0; i < pieces.Count; i++)
                {
                    var p = pieces[i];
                    SpawnPiece(root, MeshFor(p.kind), p.localPos, p.yRot, data.color, $"{p.kind}_{i}_r{p.yRot}");
                }

                if (data.axisLock != AxisLock.None) SpawnArrow(root, data);
            }
        }

        // 1. Pick the block cell nearest to the bounding-box center (so concave shapes do not put it on an empty cell).
        // 2. From that cell, count the unbroken run of block cells along the locked axis.
        // 3. Arrow_N with N = min(run, 3), placed at the run's center.
        void SpawnArrow(Transform root, BlockData data)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var c in data.cells)
            {
                minX = Mathf.Min(minX, c.x); maxX = Mathf.Max(maxX, c.x);
                minY = Mathf.Min(minY, c.y); maxY = Mathf.Max(maxY, c.y);
            }

            // Bounding-box center and cell centers, in cell units.
            var center = new Vector2((minX + maxX + 1) * 0.5f, (minY + maxY + 1) * 0.5f);
            var anchor = data.cells[0];
            float best = float.MaxValue;
            foreach (var c in data.cells)
            {
                float d = (new Vector2(c.x + 0.5f, c.y + 0.5f) - center).sqrMagnitude;
                if (d < best) { best = d; anchor = c; }
            }

            bool horizontal = data.axisLock == AxisLock.Horizontal;
            var dir = horizontal ? Vector2Int.right : Vector2Int.up;
            var body = new HashSet<Vector2Int>(data.cells);

            var runStart = anchor;
            while (body.Contains(runStart - dir)) runStart -= dir;
            var runEnd = anchor;
            while (body.Contains(runEnd + dir)) runEnd += dir;

            int run = horizontal ? runEnd.x - runStart.x + 1 : runEnd.y - runStart.y + 1;
            var runCenter = new Vector3((runStart.x + runEnd.x + 1) * 0.5f * CellSize, 0f, (runStart.y + runEnd.y + 1) * 0.5f * CellSize);

            var arrow = Instantiate(arrowPiecePrefab, root);
            arrow.name = $"Arrow_{data.axisLock}_{run}";
            arrow.transform.localPosition = runCenter;
            arrow.transform.localRotation = Quaternion.Euler(0f, (horizontal ? 0f : 90f) + arrowYawOffset, 0f);
            arrow.GetComponentInChildren<MeshFilter>().sharedMesh = arrowMeshes[Mathf.Min(run, arrowMeshes.Length) - 1];

            // The arrow is cut at the door together with the block, so it needs the clip shader too.
            var arrowRenderer = arrow.GetComponentInChildren<MeshRenderer>();
            if (arrowClipMaterial == null)
                arrowClipMaterial = new Material(arrowRenderer.sharedMaterial) { name = "Arrow_Clip", shader = Shader.Find(BlockClipShader) };
            arrowRenderer.sharedMaterial = arrowClipMaterial;
        }

        Mesh MeshFor(PieceKind kind)
        {
            switch (kind)
            {
                case PieceKind.Edge: return edgeMesh;
                case PieceKind.Center: return centerMesh;
                case PieceKind.InnerCorner: return innerCornerMesh;
                default: return outerCornerMesh;
            }
        }

        void SpawnPiece(Transform parent, Mesh mesh, Vector3 localPos, float yRot, int color, string pieceName)
        {
            var piece = Instantiate(blockPiecePrefab, parent);
            piece.name = pieceName;
            piece.transform.localPosition = localPos;
            piece.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);

            piece.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
            var renderer = piece.GetComponentInChildren<MeshRenderer>();
            renderer.sharedMaterial = BlockMaterial(color);
        }

        // Same as ColorMaterial, but on the BlockClip shader so the block can be cut at a door.
        // Swapping the shader keeps the template's matching values (color, texture, smoothness, metallic).
        Material BlockMaterial(int color)
        {
            if (!blockMaterials.TryGetValue(color, out var mat))
            {
                mat = new Material(BlockTemplate) { name = $"Block_{color}", shader = Shader.Find(BlockClipShader) };
                mat.color = palette[color];
                blockMaterials[color] = mat;
            }
            return mat;
        }

        Material ColorMaterial(int color, Material template)
        {
            if (!colorMaterials.TryGetValue(color, out var mat))
            {
                mat = new Material(template) { name = $"Color_{color}", color = palette[color] };
                colorMaterials[color] = mat;
            }
            return mat;
        }

        // Board origin = bottom-left corner of cell (0, 0).
        public Vector3 CellCenter(int x, int y)
        {
            return new Vector3(x * CellSize + CellSize * 0.5f, 0f, y * CellSize + CellSize * 0.5f);
        }
    }
}
