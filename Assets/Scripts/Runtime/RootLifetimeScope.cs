using System.IO;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Runtime
{
    /// <summary>
    /// The root scope in the Bootstrap scene (D64): holds the services that live for the whole run. Bootstrap stays
    /// loaded, so the scope does too; content scenes open next to it and their scopes become its children (D114).
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        private const string SaveFolder = "Save";

        protected override void Configure(IContainerBuilder builder)
        {
            new RootInstaller(
                new AddressablesContentInitializer(),
                new AddressablesContentDelivery(),
                new AddressablesAssetSource(),
                new AddressablesSceneLoader(this),
                PlayRequests(),
                new JsonSaveStore(Path.Combine(Application.persistentDataPath, SaveFolder)))
                .Install(builder);
        }

        /// <summary>Only the Level Editor asks for a level (D118); a player build has no request to read.</summary>
        private static IPlayRequestStore PlayRequests()
        {
#if UNITY_EDITOR
            return new EditorPlayRequestStore();
#else
            return new NoPlayRequests();
#endif
        }

        // Development hooks for the I6 release check, until navigation (M4) does the round trip: right-click the
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
