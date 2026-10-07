using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private ExitSettings exitSettings = new ExitSettings(0.08f, 10f, 0.5f);
        [Tooltip("Seconds between the last exit finishing and the win popup (D89). Moves to GameConfig in I4.")]
        [SerializeField, Min(0f)] private float winPopupDelay = 0.5f;
        [SerializeField] private TextAsset[] levels;
        [SerializeField] private string levelKey;

        private readonly Sequencer exits = new Sequencer();
        private readonly Sequencer flow = new Sequencer();
        private PaletteMaterials materials;
        private BoardView board;
        private GameplayLoop loop;

        private void Start() => InstallAsync(destroyCancellationToken).Forget();

        private void OnDestroy()
        {
            flow.Dispose();
            exits.Dispose();
            board?.Dispose();
            materials?.Dispose();
        }

        private async UniTaskVoid InstallAsync(CancellationToken cancellation)
        {
            ILevelSource source = ChooseLevel(new LevelJson(ModifierCatalog.Default()), out string key);
            Result<LevelData> level = await source.LoadAsync(key)
                .AsUniTask()
                .AttachExternalCancellation(cancellation);

            if (level.IsFailure)
            {
                Debug.LogError($"Level '{key}' could not be loaded: {level.Error}", this);
                return;
            }

            Result<LevelSession> session = LevelSession.TryCreate(level.Value);

            if (session.IsFailure)
            {
                Debug.LogError($"Level '{key}' is invalid: {session.Error}", this);
                return;
            }

            // DOTween sets itself up on its first tween (a component and its settings asset); do it while loading,
            // not on the first exit.
            DOTween.Init();
            materials = new PaletteMaterials(assets.Palette, assets.BlockTemplate, assets.DoorTemplate);
            BuildBoard(level.Value);
            BlocksView blocks = BuildBlocks(session.Value);
            WirePlay(session.Value, blocks);
        }

        /// <summary>The level the Level Editor asked to play (D75), read from its file; otherwise the scene's own.</summary>
        private ILevelSource ChooseLevel(LevelJson json, out string key)
        {
#if UNITY_EDITOR
            if (new PlayRequest(new SessionStatePlayRequestStore()).TryTake(out key))
                return new EditorFileLevelSource(json);
#endif
            key = levelKey;

            return new TextAssetLevelSource(levels, json);
        }

        private void BuildBoard(LevelData level)
        {
            var layout = new BoardLayout(level);
            board = new BoardView(new GameObject("Board").transform, assets, materials);
            board.Build(new BoardDressing(layout));
            cameraRig.Fit(layout.Extent(assets.WallMesh.bounds.max.y));
        }

        private BlocksView BuildBlocks(LevelSession session)
        {
            var blockParts = new PiecePool(assets.BlockPiece, NewInactiveRoot("Pool_BlockParts"));
            var blocks = new BlocksView(new GameObject("Blocks").transform, assets, materials, blockParts,
                ModifierPresenters.Default(assets.ModifierViews));
            blocks.Build(session.Board);

            return blocks;
        }

        private void WirePlay(LevelSession session, BlocksView blocks)
        {
            var sfx = new AudioSfxPlayer(gameObject.AddComponent<AudioSource>(), assets);
            var burst = new ExitBurst(assets.ExitParticles, materials, assets.Palette.Count,
                new GameObject("ExitParticles").transform, Environment.TickCount);
            var exitSteps = new ExitSteps(blocks, burst, sfx, exitSettings);

            var inputLock = new InputLock();
            var director = new GameplayDirector(exitSteps, inputLock, exits, flow, winPopupDelay);
            session.Observer = director;

            var drag = new DragController(session, blocks, new MousePointerInput(cameraRig.SceneCamera), inputLock,
                new DragResolver(session), dragSettings, sfx);
            loop = new GameplayLoop(session, director, drag, blocks, inputLock);
            gameObject.AddComponent<FrameTicker>().Initialize(loop);
        }

        // Development hooks until the HUD's restart and pause buttons exist (U1, U2): right-click the component.
        [ContextMenu("Restart")]
        private void Restart() => loop?.Restart();

        [ContextMenu("Pause")]
        private void Pause() => loop?.Pause();

        [ContextMenu("Resume")]
        private void Resume() => loop?.Resume();

        private static Transform NewInactiveRoot(string name)
        {
            var root = new GameObject(name);
            root.SetActive(false);

            return root.transform;
        }
    }
}
