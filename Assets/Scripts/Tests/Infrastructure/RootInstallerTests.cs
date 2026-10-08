using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Infrastructure;
using NUnit.Framework;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Tests.Infrastructure
{
    public sealed class RootInstallerTests
    {
        [Test]
        public void RootScope_ResolvesItsServices()
        {
            var content = new FakeContentInitializer();
            var scenes = new FakeSceneLoader();
            var builder = new ContainerBuilder();
            new RootInstaller(content, new FakeContentDelivery(), new FakeAssetSource(), scenes).Install(builder);

            using IObjectResolver container = builder.Build();

            Assert.That(container.Resolve<IContentInitializer>(), Is.SameAs(content));
            Assert.That(container.Resolve<IAssetLoader>(), Is.InstanceOf<AssetScope>());
            Assert.That(container.Resolve<ISceneLoader>(), Is.SameAs(scenes));
            Assert.That(container.Resolve<ILevelSource>(), Is.InstanceOf<AssetLevelSource>());
            Assert.That(container.Resolve<SelectedLevel>(), Is.Not.Null);
            Assert.That(container.Resolve<IReadOnlyList<IAsyncStartable>>(), Has.Some.InstanceOf<Bootstrapper>());
        }

        [Test]
        public void ChildScope_GetsItsOwnAssetScope_ReleasedWithIt()
        {
            var source = new FakeAssetSource();
            var builder = new ContainerBuilder();
            new RootInstaller(new FakeContentInitializer(), new FakeContentDelivery(), source, new FakeSceneLoader())
                .Install(builder);
            using IObjectResolver root = builder.Build();
            IScopedObjectResolver child = root.CreateScope();

            var childAssets = (AssetScope)child.Resolve<IAssetLoader>();
            childAssets.LoadAsync<TextAsset>("a", CancellationToken.None).Forget();
            FakeAssetHandle handle = source.Finish("a");
            child.Dispose();

            Assert.That(root.Resolve<IAssetLoader>(), Is.Not.SameAs(childAssets));
            Assert.That(handle.Releases, Is.EqualTo(1));
            Object.DestroyImmediate(handle.Asset);
        }
    }
}
