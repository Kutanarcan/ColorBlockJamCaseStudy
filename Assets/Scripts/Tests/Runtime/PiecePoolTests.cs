using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class PiecePoolTests
    {
        private GameObject prefab;
        private Transform storage;
        private Transform parent;

        [SetUp]
        public void SetUp()
        {
            prefab = new GameObject("Piece", typeof(PieceView));
            storage = new GameObject("Storage").transform;
            parent = new GameObject("Parent").transform;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(storage.gameObject);
            Object.DestroyImmediate(parent.gameObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void ReleasedPiece_IsReused_UnderTheNewParent()
        {
            var pool = new PiecePool(prefab.GetComponent<PieceView>(), storage);
            PieceView first = pool.Get(parent);

            pool.Release(first);
            Assert.That(first.gameObject.activeSelf, Is.False);
            Assert.That(first.transform.parent, Is.SameAs(storage));

            PieceView second = pool.Get(parent);
            Assert.That(second, Is.SameAs(first));
            Assert.That(second.gameObject.activeSelf, Is.True);
            Assert.That(second.transform.parent, Is.SameAs(parent));
        }
    }
}
