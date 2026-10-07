using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.Infrastructure
{
    /// <summary>
    /// Loads content scenes additively next to Bootstrap (D114). The loaded scene becomes the active one, so objects
    /// created from code land in it and go away with it; its <c>LifetimeScope</c> becomes a child of the root scope.
    /// A scene opened on its own in the Editor has no parent and still runs.
    /// </summary>
    public sealed class AddressablesSceneLoader : ISceneLoader
    {
        private readonly LifetimeScope parent;
        private AsyncOperationHandle<SceneInstance> current;

        public AddressablesSceneLoader(LifetimeScope parent) => this.parent = parent;

        public async UniTask ReplaceContentSceneAsync(string key, CancellationToken cancellation)
        {
            await UnloadContentSceneAsync(cancellation);

            SceneInstance scene;

            using (LifetimeScope.EnqueueParent(parent))
            {
                current = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive);
                scene = await current.ToUniTask(cancellationToken: cancellation);
            }

            SceneManager.SetActiveScene(scene.Scene);
        }

        public async UniTask UnloadContentSceneAsync(CancellationToken cancellation)
        {
            if (!current.IsValid())
                return;

            AsyncOperationHandle<SceneInstance> unloading = current;
            current = default;

            await Addressables.UnloadSceneAsync(unloading).ToUniTask(cancellationToken: cancellation);
        }
    }
}
