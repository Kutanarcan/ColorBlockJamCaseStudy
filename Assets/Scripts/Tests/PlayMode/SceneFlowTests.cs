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
    /// Smoke test of the real scene flow through Addressables (Editor play mode script). Needs the Main and Gameplay
    /// scenes marked addressable under their keys.
    /// </summary>
    [PrebuildSetup(typeof(TestRunnerStartScene))]
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

            // The game opens Home first (M2); Gameplay is opened from there, as the level button does.
            await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == SceneKeys.Main).Timeout(LoadTimeout);
            await root.Container.Resolve<ISceneLoader>().ReplaceContentSceneAsync(SceneKeys.Gameplay,
                CancellationToken.None);
            Assert.That(SceneManager.GetSceneByName(SceneKeys.Main).isLoaded, Is.False, "Home closed for Gameplay");

            await UniTask.DelayFrame(2); // let the scene's Start build the board
            var gameplay = Object.FindObjectOfType<GameplayLifetimeScope>();

            Assert.That(gameplay.Parent, Is.SameAs(root));

            await root.Container.Resolve<ISceneLoader>().UnloadContentSceneAsync(CancellationToken.None);

            Assert.That(SceneManager.GetSceneByName(SceneKeys.Gameplay).isLoaded, Is.False);
            Assert.That(gameplay == null, "The Gameplay scope outlived its scene.");
            Assert.That(root.gameObject.scene.rootCount, Is.EqualTo(1), "Gameplay left objects in Bootstrap.");
        });

        /// <summary>
        /// M3: Home, a level, Home again, through <see cref="Navigation"/>, as Home's Play and LoseLife's Leave do.
        /// Win, next, fail and the popups between are a manual check in the README (cut list 4).
        /// </summary>
        [UnityTest]
        public IEnumerator SceneFlow_HomeToGameplayAndBack() => UniTask.ToCoroutine(async () =>
        {
            var single = new LoadSceneParameters(LoadSceneMode.Single);
            await EditorSceneManager.LoadSceneAsyncInPlayMode(BootstrapPath, single);
            var root = Object.FindObjectOfType<RootLifetimeScope>();

            await WaitForActive(SceneKeys.Main);
            var home = Object.FindObjectOfType<MainLifetimeScope>();
            Assert.That(home.Parent, Is.SameAs(root));

            home.Container.Resolve<Navigation>().PlayLevel();
            await WaitForActive(SceneKeys.Gameplay);
            await UniTask.DelayFrame(2); // let the scene's Start build the board
            var gameplay = Object.FindObjectOfType<GameplayLifetimeScope>();

            Assert.That(home == null, "Home's scope outlived its scene.");
            Assert.That(gameplay.Parent, Is.SameAs(root));

            gameplay.Container.Resolve<Navigation>().GoHome();
            await WaitForActive(SceneKeys.Main);

            Assert.That(gameplay == null, "The Gameplay scope outlived its scene.");
            Assert.That(SceneManager.GetSceneByName(SceneKeys.Gameplay).isLoaded, Is.False);
            Assert.That(root.gameObject.scene.rootCount, Is.EqualTo(1), "A content scene left objects in Bootstrap.");
        });

        private static UniTask WaitForActive(string scene) =>
            UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == scene).Timeout(LoadTimeout);
    }
}
#endif
