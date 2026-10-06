using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Select, drag, end. The logic moves first (<see cref="DragResolver"/>), the dragged block's view follows:
    /// lifted and clamped to reachable space while dragged, snapped onto its cell when the drag ends, which commits
    /// the move. A block that exits mid-drag ends the drag; the director plays its exit. A locked input ends the
    /// drag and refuses a new one.
    /// </summary>
    public sealed class DragController : ITickable
    {
        private readonly LevelSession session;
        private readonly BlocksView blocks;
        private readonly IPointerInput pointer;
        private readonly InputLock inputLock;
        private readonly DragResolver resolver;
        private readonly DragSettings settings;
        private readonly ISfxPlayer sfx;

        private Block draggedBlock;
        private Vector2 dragStartPointer;
        private Vector2 dragStartPosition;

        public DragController(LevelSession session, BlocksView blocks, IPointerInput pointer, InputLock inputLock,
            DragResolver resolver, DragSettings settings, ISfxPlayer sfx)
        {
            this.session = session;
            this.blocks = blocks;
            this.pointer = pointer;
            this.inputLock = inputLock;
            this.resolver = resolver;
            this.settings = settings;
            this.sfx = sfx;
        }

        public void Tick(float deltaTime)
        {
            if (draggedBlock == null)
            {
                if (pointer.WasPressedThisFrame && !inputLock.IsLocked)
                    TrySelectBlock();

                return;
            }

            if (pointer.IsPressed && !inputLock.IsLocked)
                UpdateDrag();
            else
                EndDrag();
        }

        private void TrySelectBlock()
        {
            if (!BoardRaycast.TryGetCellPoint(pointer.PointerRay, out Vector2 cellPoint))
                return;

            if (!(session.Board.EntityAt(BoardRaycast.ToCell(cellPoint)) is Block block))
                return;

            draggedBlock = block;
            resolver.BeginDrag();
            dragStartPointer = cellPoint;
            dragStartPosition = PositionOf(block);
            blocks.ViewOf(block).SetPosition(BoardLayout.ToBoard(dragStartPosition, settings.LiftHeight));
            sfx.Play(SoundEffect.Select);
        }

        private void UpdateDrag()
        {
            if (!BoardRaycast.TryGetCellPoint(pointer.PointerRay, out Vector2 cellPoint))
                return;

            Vector2 wanted = dragStartPosition + cellPoint - dragStartPointer;
            var target = new Cell(Mathf.RoundToInt(wanted.x), Mathf.RoundToInt(wanted.y));

            if (resolver.MoveToward(draggedBlock, target) == MoveResult.Exited)
            {
                draggedBlock = null;
                return;
            }

            Vector2 shown = resolver.ClampToReachable(draggedBlock, wanted);
            blocks.ViewOf(draggedBlock).SetPosition(BoardLayout.ToBoard(shown, settings.LiftHeight));
        }

        private void EndDrag()
        {
            session.CommitMove();
            sfx.Play(SoundEffect.Drop);
            blocks.ViewOf(draggedBlock).SnapTo(BoardLayout.ToBoard(PositionOf(draggedBlock), 0f), settings.SnapDuration);
            draggedBlock = null;
        }

        private static Vector2 PositionOf(Block block) => new Vector2(block.Position.X, block.Position.Y);
    }
}
