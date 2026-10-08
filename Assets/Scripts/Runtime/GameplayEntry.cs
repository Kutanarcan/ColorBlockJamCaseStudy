using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core;
using Game.Infrastructure;
using UnityEngine;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The Gameplay scope's entry point (D117): loads the presentation assets and the level by key, then builds the
    /// session, views and play objects for that level. They depend on loaded data, so they are made here, not
    /// registered in the container. Everything it creates sits under the scope's object and goes with the scene;
    /// the frame reaches the loop through VContainer's tick.
    /// </summary>
    public sealed class GameplayEntry : IAsyncStartable, VContainer.Unity.ITickable, IDisposable
    {
        private readonly IAssetLoader assets;
        private readonly ILevelSource levels;
        private readonly LoadedConfig config;
        private readonly CameraRig cameraRig;
        private readonly DragSettings dragSettings;
        private readonly ExitSettings exitSettings;
        private readonly Transform root;
        private readonly Sequencer exits = new Sequencer();
        private readonly Sequencer flow = new Sequencer();
        private PresentationAssets presentation;
        private PaletteMaterials materials;
        private BoardView board;
        private SelectionOutline outline;
        private GameplayLoop loop;

        public GameplayEntry(IAssetLoader assets, ILevelSource levels, LoadedConfig config, CameraRig cameraRig,
            DragSettings dragSettings, ExitSettings exitSettings, Transform root)
        {
            this.assets = assets;
            this.levels = levels;
            this.config = config;
            this.cameraRig = cameraRig;
            this.dragSettings = dragSettings;
            this.exitSettings = exitSettings;
            this.root = root;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            try
            {
                await BuildAsync(cancellation);
            }
            catch (OperationCanceledException)
            {
                // The scene closed while it was loading: nothing is left to build, and that is not an error.
            }
        }

        private async UniTask BuildAsync(CancellationToken cancellation)
        {
            // Until progression (M0) the game plays the first level in the config's order.
            string key = config.Value.LevelKeys[0];
            presentation = await assets.LoadAsync<PresentationAssets>(PresentationAssets.Key, cancellation);
            Result<LevelData> level = await levels.LoadAsync(key).AsUniTask().AttachExternalCancellation(cancellation);

            if (level.IsFailure)
            {
                Debug.LogError($"Level '{key}' could not be loaded: {level.Error}");
                return;
            }

            Result<LevelSession> session = LevelSession.TryCreate(level.Value);

            if (session.IsFailure)
            {
                Debug.LogError($"Level '{key}' is invalid: {session.Error}");
                return;
            }

            // DOTween sets itself up on its first tween (a component and its settings asset); do it while loading,
            // not on the first exit.
            DOTween.Init();
            materials = new PaletteMaterials(presentation.Palette, presentation.BlockTemplate,
                presentation.DoorTemplate);
            BuildBoard(level.Value);
            BlocksView blocks = BuildBlocks(session.Value);
            WirePlay(session.Value, blocks);
        }

        void VContainer.Unity.ITickable.Tick() => loop?.Tick(Time.deltaTime);

        public void Restart() => loop?.Restart();

        public void Pause() => loop?.Pause();

        public void Resume() => loop?.Resume();

        public void Dispose()
        {
            flow.Dispose();
            exits.Dispose();
            outline?.Dispose();
            board?.Dispose();
            materials?.Dispose();
        }

        private void BuildBoard(LevelData level)
        {
            var layout = new BoardLayout(level);
            board = new BoardView(NewRoot("Board", true), presentation, materials);
            board.Build(new BoardDressing(layout));
            cameraRig.Fit(layout.Extent(presentation.WallMesh.bounds.max.y));
        }

        private BlocksView BuildBlocks(LevelSession session)
        {
            var blockParts = new PiecePool(presentation.BlockPiece, NewRoot("Pool_BlockParts", false));
            var blocks = new BlocksView(NewRoot("Blocks", true), presentation, materials, blockParts,
                ModifierPresenters.Default(presentation.ModifierViews));
            blocks.Build(session.Board);

            return blocks;
        }

        private void WirePlay(LevelSession session, BlocksView blocks)
        {
            var sfx = new AudioSfxPlayer(root.gameObject.AddComponent<AudioSource>(), presentation);
            var burst = new ExitBurst(presentation.ExitParticles, materials, presentation.Palette.Count,
                NewRoot("ExitParticles", true), Environment.TickCount);
            var exitSteps = new ExitSteps(blocks, burst, sfx, exitSettings);

            outline = new SelectionOutline(cameraRig.SceneCamera, presentation.SelectionOutline, blocks);
            var inputLock = new InputLock();
            var director = new GameplayDirector(exitSteps, inputLock, exits, flow, config.Value.WinPopupDelay);
            session.Observer = director;

            var drag = new DragController(session, blocks, new MousePointerInput(cameraRig.SceneCamera), inputLock,
                new DragResolver(session), dragSettings, sfx, outline);
            loop = new GameplayLoop(session, director, drag, blocks, inputLock);
        }

        private Transform NewRoot(string name, bool active)
        {
            var child = new GameObject(name);
            child.SetActive(active);
            child.transform.SetParent(root, false);

            return child.transform;
        }
    }
}
