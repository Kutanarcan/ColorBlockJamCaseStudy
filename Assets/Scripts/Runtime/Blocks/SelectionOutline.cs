using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Runtime
{
    /// <summary>
    /// A screen-space outline around the held block (FINDINGS, prototype method): every piece as one silhouette, no
    /// seams between pieces. A camera command buffer draws the block's renderers into a mask, then blits the outline
    /// over the frame. Color and width live on the material (<c>Game/SelectionOutline</c>).
    /// </summary>
    public sealed class SelectionOutline : IBlockSelection, IDisposable
    {
        private const CameraEvent DrawAfter = CameraEvent.AfterForwardAlpha;
        private const int MaskPass = 0;
        private const int CompositePass = 1;

        private static readonly int MaskId = Shader.PropertyToID("_SelectionOutlineMask");

        private readonly Camera camera;
        private readonly Material material;
        private readonly IBlocksView blocks;
        private readonly CommandBuffer buffer = new CommandBuffer { name = "SelectionOutline" };
        private readonly List<Renderer> renderers = new List<Renderer>();

        public SelectionOutline(Camera camera, Material material, IBlocksView blocks)
        {
            this.camera = camera;
            this.material = material;
            this.blocks = blocks;
            camera.AddCommandBuffer(DrawAfter, buffer);
        }

        /// <summary>
        /// Records the commands once per selection: <c>DrawRenderer</c> reads each renderer's transform when the frame
        /// renders, so the outline follows the block while it is dragged.
        /// </summary>
        public void Show(Block block)
        {
            renderers.Clear();
            blocks.ViewOf(block).AddRenderersTo(renderers);

            buffer.Clear();
            buffer.GetTemporaryRT(MaskId, -1, -1, 0, FilterMode.Point, RenderTextureFormat.R8);
            buffer.SetRenderTarget(MaskId);
            buffer.ClearRenderTarget(false, true, Color.clear);

            for (int i = 0; i < renderers.Count; i++)
                buffer.DrawRenderer(renderers[i], material, 0, MaskPass);

            buffer.Blit(MaskId, BuiltinRenderTextureType.CameraTarget, material, CompositePass);
            buffer.ReleaseTemporaryRT(MaskId);
        }

        public void Hide() => buffer.Clear();

        public void Dispose()
        {
            if (camera != null)
                camera.RemoveCommandBuffer(DrawAfter, buffer);

            buffer.Release();
        }
    }
}
