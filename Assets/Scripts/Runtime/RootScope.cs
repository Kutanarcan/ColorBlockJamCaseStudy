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
    /// its stand-ins. This class knows neither. It also sets the run's frame rate, first thing, so both starts get it.
    /// </summary>
    public abstract class RootScope : LifetimeScope
    {
        [Tooltip("Frames per second the run asks for. A screen slower than this still shows its own rate; on mobile " +
                 "this replaces V Sync.")]
        [SerializeField, Min(30)] private int targetFrameRate = 60;

        protected override void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            base.Awake();
        }

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
    }
}
