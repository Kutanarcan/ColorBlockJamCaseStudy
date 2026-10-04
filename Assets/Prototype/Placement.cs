using System;
using UnityEngine;

namespace Game.Prototype
{
    // Where is the block on screen? Visual only; knows no rules, only writes the block root's position.
    [Serializable]
    public class Placement
    {
        [SerializeField] float lift = 0.3f;      // how high the block rises while held
        [SerializeField] float followSpeed = 0f; // ease toward the pointer while held; 0 = instant
        [SerializeField] float snapSpeed = 20f;  // ease onto the cell after release; 0 = instant

        Board board;

        Transform settling; // released block still easing onto its cell
        Vector3 settleGoal;

        public void Init(Board board) => this.board = board;

        // While held: move the block toward offset (in cells, fractions allowed), lifted.
        public void Hold(Transform block, Vector2 offset)
        {
            var goal = board.OffsetToWorld(offset) + Vector3.up * lift;
            block.position = Ease(block.position, goal, followSpeed);
        }

        // On release: start easing the block onto its cell.
        public void StartSettle(Transform block, Vector2Int cell)
        {
            FinishSettle();
            settling = block;
            settleGoal = board.OffsetToWorld(cell);
        }

        // Every frame: continue the release ease.
        public void Tick()
        {
            if (settling == null) return;

            settling.position = Ease(settling.position, settleGoal, snapSpeed);
            if ((settling.position - settleGoal).sqrMagnitude < 0.0001f) FinishSettle();
        }

        // Jump a settling block onto its cell at once (e.g. another block is grabbed).
        public void FinishSettle()
        {
            if (settling == null) return;

            settling.position = settleGoal;
            settling = null;
        }

        static Vector3 Ease(Vector3 from, Vector3 to, float speed)
        {
            if (speed <= 0f) return to;
            return Vector3.Lerp(from, to, 1f - Mathf.Exp(-speed * Time.deltaTime));
        }
    }
}
