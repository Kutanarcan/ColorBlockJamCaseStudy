using Game.Infrastructure;
using NUnit.Framework;

namespace Game.Tests.Infrastructure
{
    public sealed class NavigationTests
    {
        private FakeSceneLoader scenes;
        private SelectedLevel selected;
        private Navigation navigation;

        [SetUp]
        public void SetUp()
        {
            scenes = new FakeSceneLoader();
            selected = new SelectedLevel(new Choice("Level_4"));
            navigation = new Navigation(selected, scenes);
        }

        [Test]
        public void Navigation_PlaysTheStartsChoice_InGameplay()
        {
            navigation.PlayLevel();

            Assert.That(selected.Key, Is.EqualTo("Level_4"));
            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }

        [Test]
        public void Navigation_GoesHome()
        {
            navigation.GoHome();

            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Main }));
        }

        [Test]
        public void Navigation_ChangesTheSceneOnce_WhileItIsChanging()
        {
            navigation.PlayLevel();
            navigation.GoHome();
            navigation.PlayLevel();

            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }

        /// <summary>Always the same level.</summary>
        private sealed class Choice : ILevelChoice
        {
            private readonly string key;

            public Choice(string key) => this.key = key;

            public string Choose() => key;
        }
    }
}
