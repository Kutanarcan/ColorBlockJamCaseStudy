using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// What every root scope shares (D64, D131): the run's services over Addressables and the start-up flow. A root
    /// scope lives in its start scene and stays loaded; content scenes open next to it and their scopes become its
    /// children (D114). Each start scene adds its own services: the game its live ones, the Editor-only level test
    /// its stand-ins. This class knows neither.
    /// </summary>
    public abstract class RootScope : LifetimeScope
    {
        protected sealed override void Configure(IContainerBuilder builder)
        {
            new RootInstaller(
                new AddressablesContentInitializer(),
                new AddressablesContentDelivery(),
                new AddressablesAssetSource(),
                new AddressablesSceneLoader(this),
                transform)
                .Install(builder);

            InstallStartServices(builder);
        }

        /// <summary>The services this start scene runs on: the save store and the level choice.</summary>
        protected abstract void InstallStartServices(IContainerBuilder builder);

        // Development hooks for the I6 release check, until navigation (M3) does the round trip: right-click the
        // component in play mode.
        [ContextMenu("Unload Content Scene")]
        private void UnloadContentScene() =>
            Container?.Resolve<ISceneLoader>().UnloadContentSceneAsync(destroyCancellationToken).Forget();

        [ContextMenu("Load Gameplay")]
        private void LoadGameplay() =>
            Container?.Resolve<ISceneLoader>().ReplaceContentSceneAsync(SceneKeys.Gameplay, destroyCancellationToken)
                .Forget();
    }
}
