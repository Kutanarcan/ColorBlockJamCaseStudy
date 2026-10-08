using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The one door through which presentation gets its assets (D62). Views read from here and never know how
    /// the assets were loaded; the asset is Addressable under <see cref="Key"/> in the Gameplay group (D117).
    /// Pieces are the kit's prefabs; their variants are meshes swapped in through the piece's view (D99).
    /// </summary>
    [CreateAssetMenu(menuName = "Color Block Jam/Presentation Assets", fileName = "PresentationAssets")]
    public sealed class PresentationAssets : ScriptableObject
    {
        public const string Key = "PresentationAssets";

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

        [Header("Blocks")]
        [SerializeField] private PieceView blockPiece;
        [SerializeField] private Mesh outerCornerMesh;
        [SerializeField] private Mesh edgeMesh;
        [SerializeField] private Mesh centerMesh;
        [SerializeField] private Mesh innerCornerMesh;
        [SerializeField] private ModifierViews modifierViews;

        [Header("Feedback")]
        [SerializeField] private ExitParticles exitParticles;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioClip crunchSound;
        [SerializeField] private Material selectionOutline;

        public Palette Palette => palette;
        public Material BlockTemplate => blockTemplate;
        public Material DoorTemplate => doorTemplate;

        public Transform GroundTile => groundTile;
        public PieceView WallPiece => wallPiece;
        public DoorPieceView DoorPiece => doorPiece;
        public Mesh WallMesh => wallMesh;
        public Mesh CornerMesh => cornerMesh;

        public PieceView BlockPiece => blockPiece;
        public ModifierViews ModifierViews => modifierViews;

        public ExitParticles ExitParticles => exitParticles;
        public Material SelectionOutline => selectionOutline;

        public AudioClip Sound(SoundEffect effect)
        {
            switch (effect)
            {
                case SoundEffect.Select: return selectSound;
                case SoundEffect.Drop: return dropSound;
                default: return crunchSound;
            }
        }

        public Mesh BlockMesh(BlockPartKind kind)
        {
            switch (kind)
            {
                case BlockPartKind.OuterCorner: return outerCornerMesh;
                case BlockPartKind.Edge: return edgeMesh;
                case BlockPartKind.Center: return centerMesh;
                default: return innerCornerMesh;
            }
        }
    }
}
