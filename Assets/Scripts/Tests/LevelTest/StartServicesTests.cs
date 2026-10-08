using System;
using Game.Infrastructure;
using Game.LevelTest;
using Game.Meta;
using NUnit.Framework;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Game.Tests.LevelTest
{
    /// <summary>
    /// Both starts must offer the same start services (architecture.md § Start services): a contract added to one
    /// installer only would leave the level test without it, or running the game's real one. A new start service adds
    /// its contract here.
    /// </summary>
    public sealed class StartServicesTests
    {
        private static readonly Type[] Contracts =
        {
            typeof(ISaveStore),
            typeof(ILevelChoice),
            typeof(IStartingCoins)
        };

        private LevelTestConfig testConfig;

        [SetUp]
        public void SetUp() => testConfig = ScriptableObject.CreateInstance<LevelTestConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(testConfig);

        [Test]
        public void StartServices_RegisterTheSameContracts()
        {
            using IObjectResolver game = Build(new LiveServicesInstaller(new MemorySaveStore()));
            using IObjectResolver levelTest =
                Build(new LevelTestServicesInstaller("Level_1", new MemorySaveStore(), testConfig));

            foreach (Type contract in Contracts)
            {
                Assert.DoesNotThrow(() => game.Resolve(contract), $"The game does not offer {contract.Name}.");
                Assert.DoesNotThrow(() => levelTest.Resolve(contract), $"A level test does not offer {contract.Name}.");
            }
        }

        /// <summary>What the start services take from the shared root: the config (never read here) and progression.</summary>
        private static IObjectResolver Build(IInstaller startServices)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new LoadedConfig(null));
            builder.Register<Progression>(Lifetime.Singleton);
            startServices.Install(builder);

            return builder.Build();
        }
    }
}
