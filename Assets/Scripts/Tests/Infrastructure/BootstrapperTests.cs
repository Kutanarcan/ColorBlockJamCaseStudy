using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Infrastructure;
using Game.Meta;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests.Infrastructure
{
    public sealed class BootstrapperTests
    {
        private GameConfig config;
        private LoadedConfig loaded;
        private FakeSaveStore saves;
        private SelectedLevel level;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            SetLevelKeys(config, "Level_1", "Level_2", "Level_3");
            loaded = new LoadedConfig(new FakeAssetLoader().Add(GameConfig.Key, config));
            saves = new FakeSaveStore();
            level = new SelectedLevel(new ProgressionLevelChoice(loaded, new Progression(saves)));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        [Test]
        public void Bootstrapper_InitializesContent_OnceOnStart()
        {
            var content = new FakeContentInitializer();

            Start(content, new FakeContentDelivery(), new FakeSceneLoader());

            Assert.That(content.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Bootstrapper_UpdatesContent_LoadsConfig_ThenGameplay_OnlyAfterContentIsReady()
        {
            var content = new FakeContentInitializer(held: true);
            var delivery = new FakeContentDelivery();
            var scenes = new FakeSceneLoader();

            Start(content, delivery, scenes);
            Assert.That(delivery.Log, Is.Empty);
            Assert.That(scenes.Log, Is.Empty);

            content.Finish();
            Assert.That(delivery.Log, Is.Not.Empty);
            Assert.That(loaded.Value, Is.SameAs(config));
            Assert.That(scenes.Log, Is.EqualTo(new[] { "replace " + SceneKeys.Gameplay }));
        }

        [Test]
        public void Bootstrapper_OpensTheFirstConfiguredLevel_InAFreshSave()
        {
            Start(new FakeContentInitializer(), new FakeContentDelivery(), new FakeSceneLoader());

            Assert.That(level.Key, Is.EqualTo("Level_1"));
        }

        [Test]
        public void Bootstrapper_OpensTheLevelProgressionStandsOn_WrappingTheConfiguredList()
        {
            saves.Save(Progression.SaveKey, new ProgressionData { levelsCompleted = 4 });

            Start(new FakeContentInitializer(), new FakeContentDelivery(), new FakeSceneLoader());

            Assert.That(level.Key, Is.EqualTo("Level_2"));
        }

        private void Start(FakeContentInitializer content, FakeContentDelivery delivery, FakeSceneLoader scenes) =>
            new Bootstrapper(content, new ContentUpdate(delivery), loaded, level, scenes)
                .StartAsync(CancellationToken.None)
                .Forget();

        private static void SetLevelKeys(GameConfig target, params string[] keys)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty list = serialized.FindProperty("levelKeys");
            list.arraySize = keys.Length;

            for (int i = 0; i < keys.Length; i++)
                list.GetArrayElementAtIndex(i).stringValue = keys[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
