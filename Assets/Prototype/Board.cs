using UnityEngine;

namespace Game.Prototype
{
    public class Board : MonoBehaviour
    {
        const float CellSize = 2f;

        [SerializeField] int width = 6;
        [SerializeField] int height = 8;
        [SerializeField] GameObject groundTilePrefab;

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
        public static Vector3 CellCenter(int x, int y)
        {
            return new Vector3(x * CellSize + CellSize * 0.5f, 0f, y * CellSize + CellSize * 0.5f);
        }
    }
}
