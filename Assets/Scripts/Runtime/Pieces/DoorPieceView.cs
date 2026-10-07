using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The only way into the door piece (D99). The door mesh is one cell long and is stretched along X to the
    /// run's length; the arrow is a sibling, so it keeps its size at the run's center (D98).
    /// </summary>
    public sealed class DoorPieceView : MonoBehaviour
    {
        [SerializeField] private Transform door;
        [SerializeField] private MeshRenderer doorRenderer;
        [SerializeField] private MeshRenderer arrowRenderer;

        public Renderer ArrowRenderer => arrowRenderer;

        public void SetLength(int cells)
        {
            Vector3 scale = door.localScale;
            door.localScale = new Vector3(cells, scale.y, scale.z);
        }

        public void SetMaterial(Material material) => doorRenderer.sharedMaterial = material;

        public void SetArrowMaterial(Material material) => arrowRenderer.sharedMaterial = material;
    }
}
