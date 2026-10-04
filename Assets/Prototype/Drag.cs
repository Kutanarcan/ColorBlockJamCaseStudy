using System;
using UnityEngine;

namespace Game.Prototype
{
    // Input and held state. Board owns it: Init in Start, Tick every frame.
    // Each frame: pointer -> target (in cells) -> DragCollision decides where the block can go -> Placement moves it.
    [Serializable]
    public class Drag
    {
        [SerializeField] DragCollision collision = new DragCollision();
        [SerializeField] Placement placement = new Placement();
        [SerializeField] AudioClip selectSound; // on grab
        [SerializeField] AudioClip dropSound;   // on release (not when the block exits mid-drag)
        [SerializeField, Range(0f, 1f)] float soundVolume = 1f;

        Board board;
        Camera cam;
        AudioSource audioSource;

        int heldId = -1;
        Transform held;
        Vector3 grabHit;       // pointer hit when grabbed
        Vector2Int grabOffset; // block offset (in cells) when grabbed

        public void Init(Board board, Camera cam, AudioSource audioSource)
        {
            this.board = board;
            this.cam = cam;
            this.audioSource = audioSource;
            collision.Init(board);
            placement.Init(board);
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(0)) TryGrab();
            else if (heldId >= 0 && Input.GetMouseButton(0)) Follow();
            else if (heldId >= 0 && Input.GetMouseButtonUp(0)) Release();

            placement.Tick();
        }

        void TryGrab()
        {
            if (!PointerOnGround(out var hit)) return;

            int id = board.BlockAt(hit);
            if (id < 0) return;

            placement.FinishSettle();
            heldId = id;
            held = board.BlockRoot(id);
            grabHit = hit;
            grabOffset = board.BlockOffset(id);
            Play(selectSound);
        }

        void Follow()
        {
            if (!PointerOnGround(out var hit)) return;

            // Where the pointer wants the block, in cells.
            var d = board.transform.InverseTransformVector(hit - grabHit) / board.CellWorldSize;
            var pointer = new Vector2(grabOffset.x + d.x, grabOffset.y + d.z);

            collision.Walk(heldId, new Vector2Int(Mathf.RoundToInt(pointer.x), Mathf.RoundToInt(pointer.y)));

            // Pushed through a matching door: the block is gone, drop it.
            if (board.IsExited(heldId))
            {
                held = null;
                heldId = -1;
                return;
            }

            placement.Hold(held, collision.Lean(heldId, pointer));
        }

        void Release()
        {
            placement.StartSettle(held, board.BlockOffset(heldId));
            held = null;
            heldId = -1;
            Play(dropSound);
        }

        void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip, soundVolume);
        }

        // Ray onto the board's ground plane; no colliders.
        bool PointerOnGround(out Vector3 hit)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var ground = new Plane(Vector3.up, board.transform.position);

            if (ground.Raycast(ray, out float dist))
            {
                hit = ray.GetPoint(dist);
                return true;
            }

            hit = default;
            return false;
        }
    }
}
