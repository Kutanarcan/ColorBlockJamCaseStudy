using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Prototype
{
    [Serializable]
    public class BlockData
    {
        public int color;
        public List<Vector2Int> cells = new List<Vector2Int>();
    }

    public class Board : MonoBehaviour
    {
        const float CellSize = 2f;

        [SerializeField] int width = 6;
        [SerializeField] int height = 8;
        [SerializeField] GameObject groundTilePrefab;

        [Header("Blocks")]
        [SerializeField] GameObject blockPiecePrefab;
        [SerializeField] Mesh outerCornerMesh;
        [SerializeField] Mesh edgeMesh;
        [SerializeField] Mesh centerMesh;
        [SerializeField] Mesh innerCornerMesh;
        [SerializeField] Color[] palette =
        {
            new Color(0.91f, 0.27f, 0.27f), // red
            new Color(0.25f, 0.52f, 0.95f), // blue
            new Color(0.30f, 0.80f, 0.35f), // green
            new Color(0.98f, 0.82f, 0.20f), // yellow
            new Color(0.62f, 0.36f, 0.88f), // purple
            new Color(0.98f, 0.55f, 0.18f), // orange
        };

        [Header("Level")]
        [SerializeField] List<BlockData> blocks = new List<BlockData>
        {
            new BlockData { color = 0, cells = new List<Vector2Int> { new Vector2Int(2, 3) } },
        };

        Transform tiles;
        readonly Dictionary<int, Material> colorMaterials = new Dictionary<int, Material>();

        void Start()
        {
            BuildTiles();
            BuildBlocks();
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

        void BuildBlocks()
        {
            for (int id = 0; id < blocks.Count; id++)
            {
                var data = blocks[id];
                var root = new GameObject($"Block_{id}").transform;
                root.SetParent(transform, false);

                var pieces = BlockDrawRule.Build(data.cells, CellSize);
                for (int i = 0; i < pieces.Count; i++)
                {
                    var p = pieces[i];
                    SpawnPiece(root, MeshFor(p.kind), p.localPos, p.yRot, data.color, $"{p.kind}_{i}_r{p.yRot}");
                }
            }
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
            renderer.sharedMaterial = ColorMaterial(color, renderer.sharedMaterial);
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
