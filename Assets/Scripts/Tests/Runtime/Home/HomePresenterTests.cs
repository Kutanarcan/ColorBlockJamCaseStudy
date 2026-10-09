using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Meta;
using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public sealed class HomePresenterTests
    {
        private TestMeta meta;
        private FakeHomeView view;
        private Scenes scenes;
        private SelectedLevel selected;
        private Navigation navigation;

        [SetUp]
        public void SetUp()
        {
            meta = new TestMeta(startCoins: 250);
            meta.Progression.CompleteLevel();
            meta.Progression.CompleteLevel();
            view = new FakeHomeView();
            scenes = new Scenes();
            selected = new SelectedLevel(new LevelByNumber(meta.Progression));
            navigation = new Navigation(selected, scenes);
        }

        [Test]
        public void LevelButton_ShowsAndStartsTheCurrentLevel()
        {
            // The level button opens the Play popup; its Play starts the level, as Home's actions do.
            using var presenter =
                new HomePresenter(view, meta.Wallet, meta.Progression, () => { }, navigation.PlayLevel);

            Assert.That(view.Level, Is.EqualTo(3), "two levels done: the third is current");

            view.PressLevel();

            Assert.That(selected.Key, Is.EqualTo("Level_3"));
            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }

        [Test]
        public void Home_ShowsTheCoins_AndTheComingLevels_AfterTheCurrentOne()
        {
            using var presenter = new HomePresenter(view, meta.Wallet, meta.Progression, () => { }, () => { });

            Assert.That(view.Coins, Is.EqualTo(250));
            Assert.That(view.FirstComing, Is.EqualTo(4));
        }

        /// <summary>The level progression points to, named by its number.</summary>
        private sealed class LevelByNumber : ILevelChoice
        {
            private readonly Progression progression;

            public LevelByNumber(Progression progression) => this.progression = progression;

            public string Choose() => "Level_" + progression.LevelNumber;
        }

        /// <summary>Loads nothing; logs each change.</summary>
        private sealed class Scenes : ISceneLoader
        {
            public List<string> Log { get; } = new List<string>();

            public UniTask ReplaceContentSceneAsync(string key, CancellationToken cancellation)
            {
                Log.Add("replace " + key);

                return UniTask.CompletedTask;
            }

            public UniTask UnloadContentSceneAsync(CancellationToken cancellation) => UniTask.CompletedTask;
        }
    }
}
