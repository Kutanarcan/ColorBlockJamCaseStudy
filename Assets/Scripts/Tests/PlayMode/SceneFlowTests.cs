#if UNITY_EDITOR
using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Runtime;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;
using Object = UnityEngine.Object;

namespace Game.Tests.PlayMode
{
    /// <summary>
    /// Smoke test of the real scene flow through Addressables (Editor play mode script). Needs the Gameplay scene
    /// marked addressable under its key.
    /// </summary>
    public sealed class SceneFlowTests
    {
        private const string BootstrapPath = "Assets/Scenes/Bootstrap.unity";
        private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(10);

        [UnityTest]
        public IEnumerator SceneFlow_BootstrapToGameplay_AndBack() => UniTask.ToCoroutine(async () =>
        {
            var single = new LoadSceneParameters(LoadSceneMode.Single);
            await EditorSceneManager.LoadSceneAsyncInPlayMode(BootstrapPath, single);
            var root = Object.FindObjectOfType<RootLifetimeScope>();

            await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == SceneKeys.Gameplay)
                .Timeout(LoadTimeout);
            await UniTask.DelayFrame(2); // let the scene's Start build the board
            var gameplay = Object.FindObjectOfType<GameplayLifetimeScope>();

            Assert.That(gameplay.Parent, Is.SameAs(root));

            await root.Container.Resolve<ISceneLoader>().UnloadContentSceneAsync(CancellationToken.None);

            Assert.That(SceneManager.GetSceneByName(SceneKeys.Gameplay).isLoaded, Is.False);
            Assert.That(gameplay == null, "The Gameplay scope outlived its scene.");
            Assert.That(root.gameObject.scene.rootCount, Is.EqualTo(1), "Gameplay left objects in Bootstrap.");
        });
    }
}
#endif
