using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The one door through which presentation gets its assets (D62). Views read from here and never know how
    /// the assets were loaded; in I4 this content comes from Addressables instead of a scene reference.
    /// Pieces are the kit's prefabs; their variants are meshes swapped in through the piece's view (D99).
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Presentation Assets", fileName = "PresentationAssets")]
    public sealed class PresentationAssets : ScriptableObject
    {
        [Header("Colors")]
        [SerializeField] private Palette palette;
        [SerializeField] private Material blockTemplate;
        [SerializeField] private Material doorTemplate;

        [Header("Board")]
        [SerializeField] private Transform groundTile;
        [SerializeField] private PieceView wallPiece;
        [SerializeField] private DoorPieceView doorPiece;
        [SerializeField] private Mesh wallMesh;
        [SerializeField] private Mesh cornerMesh;

        public Palette Palette => palette;
        public Material BlockTemplate => blockTemplate;
        public Material DoorTemplate => doorTemplate;

        public Transform GroundTile => groundTile;
        public PieceView WallPiece => wallPiece;
        public DoorPieceView DoorPiece => doorPiece;
        public Mesh WallMesh => wallMesh;
        public Mesh CornerMesh => cornerMesh;
    }
}
