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
        [SerializeField] private TextAsset[] levels;
        [SerializeField] private string levelKey;

        private PaletteMaterials materials;

        private void Start() => InstallAsync(destroyCancellationToken).Forget();

        private void OnDestroy() => materials?.Dispose();

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
            var board = new BoardView(new GameObject("Board").transform, assets, materials);
            board.Build(new BoardDressing(new BoardLayout(level.Value)));

            var blockParts = new PiecePool(assets.BlockPiece, NewInactiveRoot("Pool_BlockParts"));
            var blocks = new BlocksView(new GameObject("Blocks").transform, assets, materials, blockParts,
                ModifierPresenters.Default(assets.ModifierViews));
            blocks.Build(session.Value.Board);
        }

        private static Transform NewInactiveRoot(string name)
        {
            var root = new GameObject(name);
            root.SetActive(false);

            return root.transform;
        }
    }
}
