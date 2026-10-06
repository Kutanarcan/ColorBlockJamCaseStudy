using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Dumb view of Arrow (D103, D104), on the <c>ArrowPiece</c> prefab (D99): shows the arrow mesh of the given
    /// length where it is told. <see cref="yawOffset"/> fixes the mesh's own orientation.
    /// </summary>
    public sealed class ArrowView : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [Tooltip("Arrow_1, Arrow_2, Arrow_3: index = length − 1")]
        [SerializeField] private Mesh[] meshesByLength = new Mesh[0];
        [Tooltip("Yaw that makes the mesh point +Z (Up). 0 if Arrow_N already points +Z at rotation 0.")]
        [SerializeField] private float yawOffset;

        public int MaxLength => meshesByLength.Length;

        public void Show(int length, Vector3 localPosition, float yaw)
        {
            meshFilter.sharedMesh = meshesByLength[length - 1];
            transform.SetLocalPositionAndRotation(localPosition, Quaternion.Euler(0f, yaw + yawOffset, 0f));
        }
    }
}
