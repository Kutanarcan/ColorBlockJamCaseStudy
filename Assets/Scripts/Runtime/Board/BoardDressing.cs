using System.Collections.Generic;
using Game.Core;

namespace Game.Runtime
{
    /// <summary>Every static board piece of a level, decided once without Unity: what the board view instantiates.</summary>
    public sealed class BoardDressing
    {
        private readonly List<Placement> ground = new List<Placement>();
        private readonly List<Placement> walls = new List<Placement>();
        private readonly List<Placement> corners = new List<Placement>();
        private readonly List<DoorRun> doors = new List<DoorRun>();

        public IReadOnlyList<Placement> Ground => ground;
        public IReadOnlyList<Placement> Walls => walls;
        public IReadOnlyList<Placement> Corners => corners;
        public IReadOnlyList<DoorRun> Doors => doors;

        public BoardDressing(BoardLayout layout)
        {
            AddGround(layout);
            WallDrawRule.Build(layout, walls, corners);
            DoorDrawRule.Build(layout, doors);
        }

        /// <summary>One tile under every open cell, unreachable holes included (V1 D4).</summary>
        private void AddGround(BoardLayout layout)
        {
            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    var cell = new Cell(x, y);

                    if (!layout.IsSolid(cell))
                        ground.Add(new Placement(BoardLayout.CellCenter(cell), 0f));
                }
            }
        }
    }
}
