using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Game.Prototype
{
    // Visual exit only: slide the block root through the door and cut it at a plane inside the door.
    // Logic (cells freed, exited flag) is already done by Board before Play is called.
    [Serializable]
    public class BlockExit
    {
        [SerializeField] float snapDuration = 0.08f; // seconds to snap onto the cell before sliding in; 0 = instant
        [SerializeField] float speed = 10f;         // units per second along the exit direction
        [SerializeField] Ease ease = Ease.Linear;
        [SerializeField] float clipOffset = 0.5f;   // cut plane distance outside the board edge; 0.5 = door center
        [SerializeField] BlockExitParticles particles;
        [SerializeField] AudioClip enterSound;               // plays once when the block starts sliding into the door
        [SerializeField, Range(0f, 1f)] float enterVolume = 1f;

        static readonly int ClipPlaneId = Shader.PropertyToID("_ClipPlane");

        AudioSource audioSource;

        public void Init(AudioSource audioSource) => this.audioSource = audioSource;

        // root: block root. cells: the block's logical cells at exit. offset: its blockOffset. dir: exit direction in cells.
        // rest: root position on its current cell, on the ground. normal: dir in world space.
        // edge: any world point on the board border line.
        public void Play(Transform root, IReadOnlyList<Vector2Int> cells, Vector2Int offset, Vector2Int dir,
            Color color, float cellSize, Vector3 rest, Vector3 normal, Vector3 edge)
        {
            var rows = Rows(cells, dir);
            float length = rows.Count * cellSize;

            var cut = edge + normal * clipOffset;
            var plane = new Vector4(normal.x, normal.y, normal.z, Vector3.Dot(cut, normal));

            // Per block: color materials are shared by every block of that color.
            var props = new MaterialPropertyBlock();
            props.SetVector(ClipPlaneId, plane);
            foreach (var r in root.GetComponentsInChildren<Renderer>()) r.SetPropertyBlock(props);

            // The rear of the block starts length + clipOffset behind the cut; travel until it passes it.
            var target = rest + normal * (length + clipOffset);
            float duration = (length + clipOffset) / Mathf.Max(0.01f, speed);

            // Row k (0 = front) bursts when its center reaches the cut line.
            int nextRow = 0;
            void BurstPassedRows(float traveled)
            {
                while (nextRow < rows.Count && traveled >= nextRow * cellSize + cellSize * 0.5f + clipOffset)
                {
                    if (particles != null)
                        particles.Spawn(root, rows[nextRow], offset, dir, color, cellSize);
                    nextRow++;
                }
            }

            // First snap onto the cell in front of the door (drops the lift and lean), then slide in.
            root.DOKill();
            DOTween.Sequence()
                .Append(root.DOMove(rest, snapDuration).SetEase(Ease.OutQuad))
                .AppendCallback(PlayEnterSound)
                .Append(root.DOMove(target, duration).SetEase(ease)
                    .OnUpdate(() => BurstPassedRows(Vector3.Dot(root.position - rest, normal))))
                .OnComplete(() =>
                {
                    BurstPassedRows(float.MaxValue);
                    root.gameObject.SetActive(false);
                });
        }

        void PlayEnterSound()
        {
            if (audioSource != null && enterSound != null) audioSource.PlayOneShot(enterSound, enterVolume);
        }

        // Cells grouped by row along dir; index 0 = the front row (nearest the door).
        static List<List<Vector2Int>> Rows(IReadOnlyList<Vector2Int> cells, Vector2Int dir)
        {
            int max = int.MinValue, min = int.MaxValue;
            foreach (var c in cells)
            {
                int d = c.x * dir.x + c.y * dir.y;
                max = Mathf.Max(max, d);
                min = Mathf.Min(min, d);
            }

            var rows = new List<List<Vector2Int>>();
            for (int i = 0; i <= max - min; i++) rows.Add(new List<Vector2Int>());
            foreach (var c in cells) rows[max - (c.x * dir.x + c.y * dir.y)].Add(c);
            return rows;
        }
    }
}
