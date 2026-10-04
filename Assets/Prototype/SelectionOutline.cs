using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Prototype
{
    // Screen-space outline around one block (all its pieces as one silhouette, no seams between pieces).
    // A camera CommandBuffer draws the block into a mask, then blits the outline over the frame.
    [Serializable]
    public class SelectionOutline
    {
        [SerializeField] Color color = new Color(1f, 0.78f, 0.2f); // between yellow and gold
        [SerializeField, Range(1f, 6f)] float width = 4f;          // pixels

        static readonly int MaskId = Shader.PropertyToID("_SelectionOutlineMask");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int WidthId = Shader.PropertyToID("_Width");

        CommandBuffer buffer;
        Material material;

        public void Init(Camera cam)
        {
            material = new Material(Shader.Find("Prototype/SelectionOutline"));
            buffer = new CommandBuffer { name = "SelectionOutline" };
            cam.AddCommandBuffer(CameraEvent.AfterForwardAlpha, buffer);
        }

        // Rebuilt once per selection: DrawRenderer reads each renderer's transform when the frame renders,
        // so the outline follows the block while it is dragged.
        public void Show(Transform block)
        {
            material.SetColor(ColorId, color);
            material.SetFloat(WidthId, width);

            buffer.Clear();
            buffer.GetTemporaryRT(MaskId, -1, -1, 0, FilterMode.Point, RenderTextureFormat.R8);
            buffer.SetRenderTarget(MaskId);
            buffer.ClearRenderTarget(false, true, Color.clear);
            foreach (var r in block.GetComponentsInChildren<Renderer>())
                buffer.DrawRenderer(r, material, 0, 0);
            buffer.Blit(MaskId, BuiltinRenderTextureType.CameraTarget, material, 1);
            buffer.ReleaseTemporaryRT(MaskId);
        }

        public void Hide() => buffer.Clear();
    }
}
