using System;
using System.Collections.Generic;
using System.Threading;
using Game.Infrastructure;
using NUnit.Framework;

namespace Game.Tests.Infrastructure
{
    public sealed class CoveredSceneLoaderTests
    {
        private List<string> log;
        private FakeSceneLoader scenes;
        private CoveredSceneLoader loader;

        [SetUp]
        public void SetUp()
        {
            log = new List<string>();
            scenes = new FakeSceneLoader(log);
            loader = new CoveredSceneLoader(scenes, new FakeLoadingCover(log));
        }

        [Test]
        public void CoveredSceneLoader_ShowsTheCoverBeforeTheChange_AndLeavesItForTheNewScene()
        {
            loader.ReplaceContentSceneAsync(SceneKeys.Gameplay, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(log, Is.EqualTo(new[] { "show", "replace " + SceneKeys.Gameplay }));
        }

        [Test]
        public void CoveredSceneLoader_LiftsTheCover_WhenTheLoadFails()
        {
            scenes.Fails = true;

            Assert.Throws<InvalidOperationException>(() =>
                loader.ReplaceContentSceneAsync(SceneKeys.Gameplay, CancellationToken.None).GetAwaiter().GetResult());
            Assert.That(log, Is.EqualTo(new[] { "show", "replace " + SceneKeys.Gameplay, "hide" }));
        }

        [Test]
        public void CoveredSceneLoader_LiftsTheCover_AfterAnUnloadWithNoNextScene()
        {
            loader.UnloadContentSceneAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(log, Is.EqualTo(new[] { "unload", "hide" }));
        }
    }
}
