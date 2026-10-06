using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.LevelIO;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Composition root of the Gameplay scene until Infrastructure (D62, D64): wires the level source,
    /// starts the session and builds the board. I4 replaces it with <c>GameplayLifetimeScope</c>; nothing it
    /// creates knows that.
    /// </summary>
    public sealed class GameplayInstaller : MonoBehaviour
    {
        [SerializeField] private PresentationAssets assets;
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private DragSettings dragSettings = new DragSettings(0.3f, 0.1f);
        [Tooltip("Seconds between the last exit finishing and the win popup (D89). Moves to GameConfig in I4.")]
        [SerializeField, Min(0f)] private float winPopupDelay = 0.5f;
        [SerializeField] private TextAsset[] levels;
        [SerializeField] private string levelKey;

        private readonly Sequencer exits = new Sequencer();
        private readonly Sequencer flow = new Sequencer();
        private PaletteMaterials materials;

        private void Start() => InstallAsync(destroyCancellationToken).Forget();

        private void OnDestroy()
        {
            flow.Dispose();
            exits.Dispose();
            materials?.Dispose();
        }

        private async UniTaskVoid InstallAsync(CancellationToken cancellation)
        {
            var source = new TextAssetLevelSource(levels, new LevelJson(ModifierCatalog.Default()));
            Result<LevelData> level = await source.LoadAsync(levelKey)
                .AsUniTask()
                .AttachExternalCancellation(cancellation);

            if (level.IsFailure)
            {
                Debug.LogError($"Level '{levelKey}' could not be loaded: {level.Error}", this);
                return;
            }

            Result<LevelSession> session = LevelSession.TryCreate(level.Value);

            if (session.IsFailure)
            {
                Debug.LogError($"Level '{levelKey}' is invalid: {session.Error}", this);
                return;
            }

            materials = new PaletteMaterials(assets.Palette, assets.BlockTemplate, assets.DoorTemplate);
            var layout = new BoardLayout(level.Value);
            var board = new BoardView(new GameObject("Board").transform, assets, materials);
            board.Build(new BoardDressing(layout));
            cameraRig.Fit(layout.Extent(assets.WallMesh.bounds.max.y));

            var blockParts = new PiecePool(assets.BlockPiece, NewInactiveRoot("Pool_BlockParts"));
            var blocks = new BlocksView(new GameObject("Blocks").transform, assets, materials, blockParts,
                ModifierPresenters.Default(assets.ModifierViews));
            blocks.Build(session.Value.Board);

            var inputLock = new InputLock();
            session.Value.Observer = new GameplayDirector(blocks, inputLock, exits, flow, winPopupDelay);

            var drag = new DragController(session.Value, blocks, new MousePointerInput(cameraRig.SceneCamera),
                inputLock, new DragResolver(session.Value), dragSettings);
            gameObject.AddComponent<FrameTicker>().Initialize(drag);
        }

        private static Transform NewInactiveRoot(string name)
        {
            var root = new GameObject(name);
            root.SetActive(false);

            return root.transform;
        }
    }
}
