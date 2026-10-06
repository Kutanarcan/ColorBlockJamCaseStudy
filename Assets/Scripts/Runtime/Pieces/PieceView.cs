using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The only way into a kit piece's mesh (D99). Sits on the prefab root; the Mesh child is a serialized
    /// reference, so moving it in the hierarchy never breaks the code.
    /// </summary>
    public sealed class PieceView : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        public void SetMesh(Mesh mesh) => meshFilter.sharedMesh = mesh;

        public void SetMaterial(Material material) => meshRenderer.sharedMaterial = material;
    }
}
