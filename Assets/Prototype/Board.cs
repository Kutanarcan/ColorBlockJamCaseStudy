using UnityEngine;

namespace Game.Prototype
{
    public class Board : MonoBehaviour
    {
        const float CellSize = 2f;

        [SerializeField] int width = 6;
        [SerializeField] int height = 8;
        [SerializeField] GameObject groundTilePrefab;
        [SerializeField, Range(0f, 1f)] float cellGap = 0.1f; // added to the 2-unit pitch; the mesh already leaves 0.047

        float Pitch => CellSize + cellGap;

        Transform tiles;

        void Start()
        {
            BuildTiles();
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

        // Board origin = bottom-left corner of cell (0, 0).
        public Vector3 CellCenter(int x, int y)
        {
            return new Vector3(x * Pitch + Pitch * 0.5f, 0f, y * Pitch + Pitch * 0.5f);
        }
    }
}
