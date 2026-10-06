using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime
{
    /// <summary>
    /// Reuses kit pieces of one prefab. A released piece goes inactive under the storage root; Get hands it
    /// back under a new parent. Restart and exit return pieces here instead of destroying them (P6, P7).
    /// </summary>
    public sealed class PiecePool
    {
        private readonly ObjectPool<PieceView> pool;
        private readonly Transform storage;

        public PiecePool(PieceView prefab, Transform storage)
        {
            this.storage = storage;
            pool = new ObjectPool<PieceView>(
                () => Object.Instantiate(prefab, storage),
                actionOnRelease: piece => piece.gameObject.SetActive(false),
                actionOnDestroy: piece => Object.Destroy(piece.gameObject));
        }

        public PieceView Get(Transform parent)
        {
            PieceView piece = pool.Get();
            piece.transform.SetParent(parent, false);
            piece.gameObject.SetActive(true);

            return piece;
        }

        public void Release(PieceView piece)
        {
            piece.transform.SetParent(storage, false);
            pool.Release(piece);
        }
    }
}
