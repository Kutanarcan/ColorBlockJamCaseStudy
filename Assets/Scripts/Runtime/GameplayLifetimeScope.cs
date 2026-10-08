using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The Gameplay scene's scope (D64, D117): a child of the root scope, opened by the scene loader. It carries the
    /// scene's own references and settings and runs <see cref="GameplayEntry"/>; closing the scene disposes it, its
    /// asset scope and everything the entry built.
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private DragSettings dragSettings = new DragSettings(0.3f, 0.1f, 0.3f);
        [SerializeField] private ExitSettings exitSettings = new ExitSettings(0.08f, 10f, 0.5f);

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(cameraRig);
            builder.RegisterInstance(dragSettings);
            builder.RegisterInstance(exitSettings);
            builder.RegisterEntryPoint<GameplayEntry>().WithParameter(transform).AsSelf();
        }

        // Development hooks until the HUD's restart and pause buttons exist (U1, U2): right-click the component.
        [ContextMenu("Restart")]
        private void Restart() => Container?.Resolve<GameplayEntry>().Restart();

        [ContextMenu("Pause")]
        private void Pause() => Container?.Resolve<GameplayEntry>().Pause();

        [ContextMenu("Resume")]
        private void Resume() => Container?.Resolve<GameplayEntry>().Resume();
    }
}
