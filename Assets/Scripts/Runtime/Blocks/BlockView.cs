using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// One block on screen: its root (moved as one), its pooled parts and the views attached to it. The root sits at
    /// the board origin and parts at the block's cells, so a move is only a root offset from <c>home</c>, the board
    /// position the parts were built at (AlgorithmExplanation § Coordinates). Knows nothing of the logic (D104).
    /// Parts share one block material; this block's color reaches them per instance through a property block (D108).
    /// The property block is one shared, reused buffer: each renderer copies its values when it is set.
    /// </summary>
    public sealed class BlockView
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int ClipPlaneId = Shader.PropertyToID("_ClipPlane");

        private readonly List<PieceView> parts = new List<PieceView>();
        private readonly List<Renderer> attachedRenderers = new List<Renderer>();
        private readonly List<GameObject> attached = new List<GameObject>();
        private readonly Material material;
        private readonly Color color;
        private readonly MaterialPropertyBlock buffer;
        private readonly Vector3 home;
        private readonly DOGetter<Vector3> getLocalPosition;
        private readonly DOSetter<Vector3> setLocalPosition;

        public Transform Root { get; }

        public BlockView(Transform root, Material material, Color color, MaterialPropertyBlock buffer, Vector3 home)
        {
            Root = root;
            this.material = material;
            this.color = color;
            this.buffer = buffer;
            this.home = home;

            // Made once: DOLocalMove would build both as new closures on every move.
            getLocalPosition = GetLocalPosition;
            setLocalPosition = SetLocalPosition;
        }

        public void AddPart(PieceView part)
        {
            part.SetMaterial(material);
            buffer.Clear();
            buffer.SetColor(ColorId, color);
            part.Renderer.SetPropertyBlock(buffer);
            parts.Add(part);
        }

        /// <summary>A modifier's view, placed under the root so it moves (and is cut) with the block.</summary>
        public T Attach<T>(T prefab) where T : Component
        {
            T view = Object.Instantiate(prefab, Root);
            attached.Add(view.gameObject);
            attachedRenderers.AddRange(view.GetComponentsInChildren<Renderer>(true));

            return view;
        }

        /// <summary>
        /// Draws every part with another material (Ice) instead of the block's color; the color block goes, so the
        /// parts batch with every other part of that material.
        /// </summary>
        public void SetSurface(Material surface)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].SetMaterial(surface);
                parts[i].Renderer.SetPropertyBlock(null);
            }
        }

        /// <summary>
        /// Cuts the block and what is attached to it at a world plane (Game/Block shader). The parts keep their color;
        /// attached views (the arrow) keep their own material's.
        /// </summary>
        public void SetClipPlane(Vector4 plane)
        {
            buffer.Clear();
            buffer.SetColor(ColorId, color);
            buffer.SetVector(ClipPlaneId, plane);

            for (int i = 0; i < parts.Count; i++)
                parts[i].Renderer.SetPropertyBlock(buffer);

            buffer.Clear();
            buffer.SetVector(ClipPlaneId, plane);

            for (int i = 0; i < attachedRenderers.Count; i++)
                attachedRenderers[i].SetPropertyBlock(buffer);
        }

        /// <summary>Puts the block at a board position at once, stopping a snap still running.</summary>
        public void SetPosition(Vector3 boardPosition)
        {
            Root.DOKill();
            Root.localPosition = boardPosition - home;
        }

        /// <summary>Animates the block onto a board position.</summary>
        public void SnapTo(Vector3 boardPosition, float duration) => Move(boardPosition, duration).SetEase(Ease.OutCubic);

        /// <summary>Moves the block to a board position and finishes with the motion; a cancel kills it.</summary>
        public UniTask MoveTo(Vector3 boardPosition, float duration, Ease ease, CancellationToken cancellation) =>
            Move(boardPosition, duration)
                .SetEase(ease)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellation);

        public void Hide() => Root.gameObject.SetActive(false);

        /// <summary>
        /// Ends this view (restart, D71): stops its motion, returns its parts uncut to the pool and destroys the
        /// rest. The view is not used afterwards.
        /// </summary>
        public void Release(PiecePool pool)
        {
            Root.DOKill();

            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].Renderer.SetPropertyBlock(null);
                pool.Release(parts[i]);
            }

            for (int i = 0; i < attached.Count; i++)
                Object.Destroy(attached[i]);

            Object.Destroy(Root.gameObject);
        }

        /// <summary>
        /// One root move, without per-call garbage: the getter and setter are made once, and the tween goes back to
        /// DOTween's pool when it ends (recyclable). Safe because no tween reference is kept past its end; the
        /// target lets <c>DOKill</c> find it.
        /// </summary>
        private Tweener Move(Vector3 boardPosition, float duration)
        {
            Root.DOKill();

            return DOTween.To(getLocalPosition, setLocalPosition, boardPosition - home, duration)
                .SetTarget(Root)
                .SetRecyclable(true);
        }

        private Vector3 GetLocalPosition() => Root.localPosition;

        private void SetLocalPosition(Vector3 position) => Root.localPosition = position;
    }
}
