using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// One block on screen: its root (moved as one), its pooled parts and the views attached to it. The root sits at
    /// the board origin and parts at the block's cells, so a move is only a root offset from <c>home</c>, the board
    /// position the parts were built at (AlgorithmExplanation § Coordinates). Knows nothing of the logic (D104).
    /// </summary>
    public sealed class BlockView
    {
        private static readonly int ClipPlaneId = Shader.PropertyToID("_ClipPlane");

        private readonly List<PieceView> parts = new List<PieceView>();
        private readonly List<Renderer> renderers = new List<Renderer>();
        private readonly Material color;
        private readonly Vector3 home;

        public Transform Root { get; }

        public BlockView(Transform root, Material color, Vector3 home)
        {
            Root = root;
            this.color = color;
            this.home = home;
        }

        public void AddPart(PieceView part)
        {
            part.SetMaterial(color);
            parts.Add(part);
            renderers.Add(part.Renderer);
        }

        /// <summary>A modifier's view, placed under the root so it moves (and is cut) with the block.</summary>
        public T Attach<T>(T prefab) where T : Component
        {
            T attached = Object.Instantiate(prefab, Root);
            renderers.AddRange(attached.GetComponentsInChildren<Renderer>(true));

            return attached;
        }

        /// <summary>Draws every part with another material (Ice) instead of the block's palette color.</summary>
        public void SetSurface(Material material)
        {
            for (int i = 0; i < parts.Count; i++)
                parts[i].SetMaterial(material);
        }

        /// <summary>
        /// Cuts the block at a world plane (Game/Block shader). <paramref name="buffer"/> is a shared, reused block:
        /// each renderer copies its values, so materials stay shared.
        /// </summary>
        public void SetClipPlane(Vector4 plane, MaterialPropertyBlock buffer)
        {
            buffer.Clear();
            buffer.SetVector(ClipPlaneId, plane);

            for (int i = 0; i < renderers.Count; i++)
                renderers[i].SetPropertyBlock(buffer);
        }

        /// <summary>Puts the block at a board position at once, stopping a snap still running.</summary>
        public void SetPosition(Vector3 boardPosition)
        {
            Root.DOKill();
            Root.localPosition = boardPosition - home;
        }

        /// <summary>Animates the block onto a board position.</summary>
        public void SnapTo(Vector3 boardPosition, float duration)
        {
            Root.DOKill();
            Root.DOLocalMove(boardPosition - home, duration).SetEase(Ease.OutCubic);
        }

        /// <summary>Moves the block to a board position and finishes with the motion; a cancel kills it.</summary>
        public UniTask MoveTo(Vector3 boardPosition, float duration, Ease ease, CancellationToken cancellation)
        {
            Root.DOKill();

            return Root.DOLocalMove(boardPosition - home, duration)
                .SetEase(ease)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellation);
        }

        public void Hide() => Root.gameObject.SetActive(false);
    }
}
