using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// One door piece per run (D98): a line of door cells with the same color and direction, across door
    /// entities (V1 D7). The piece sits on the run's approach side (where blocks come from), centered on it.
    /// </summary>
    public static class DoorDrawRule
    {
        private const float ApproachOffset = BoardLayout.CellSize * 0.25f;

        public static void Build(BoardLayout layout, List<DoorRun> runs)
        {
            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    var cell = new Cell(x, y);
                    DoorData door = layout.DoorAt(cell);

                    if (door != null && !IsSameRun(door, layout.DoorAt(cell - RunStep(door))))
                        runs.Add(RunFrom(layout, cell, door));
                }
            }
        }

        /// <summary>Does this door cell's quadrant (signX, signZ) lie on the side the door piece covers?</summary>
        public static bool Covers(DoorData door, int signX, int signZ)
        {
            Cell approach = Approach(door);

            return approach.X != 0 ? signX == approach.X : signZ == approach.Y;
        }

        private static DoorRun RunFrom(BoardLayout layout, Cell first, DoorData door)
        {
            Cell step = RunStep(door);
            Cell last = first;
            int length = 1;

            while (IsSameRun(door, layout.DoorAt(last + step)))
            {
                last += step;
                length++;
            }

            Vector3 center = (OnApproachSide(first, door) + OnApproachSide(last, door)) * 0.5f;
            float yaw = PieceYaw.Facing(Opposite(door.Direction));

            return new DoorRun(new Placement(center, yaw), length, door.ColorId);
        }

        private static Vector3 OnApproachSide(Cell cell, DoorData door)
        {
            Cell approach = Approach(door);

            return BoardLayout.CellCenter(cell) + new Vector3(approach.X * ApproachOffset, 0f, approach.Y * ApproachOffset);
        }

        private static bool IsSameRun(DoorData door, DoorData other) =>
            other != null && other.ColorId == door.ColorId && other.Direction == door.Direction;

        /// <summary>Runs go along the door line: X for doors facing up or down, Y for left or right.</summary>
        private static Cell RunStep(DoorData door) =>
            door.Direction == Direction.Up || door.Direction == Direction.Down ? new Cell(1, 0) : new Cell(0, 1);

        private static Cell Approach(DoorData door) => Opposite(door.Direction).ToOffset();

        private static Direction Opposite(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                case Direction.Left: return Direction.Right;
                default: return Direction.Left;
            }
        }
    }
}
