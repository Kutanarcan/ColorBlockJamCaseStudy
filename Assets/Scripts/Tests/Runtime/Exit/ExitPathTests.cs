using System;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class ExitPathTests
    {
        private const float ClipOffset = 0.5f;

        private static Block BlockAt(Cell position, params Cell[] shape) =>
            new Block(0, position, shape, 0, Array.Empty<IModifier>());

        [Test]
        public void ExitPath_CutsInsideTheDoor_AndCountsRowsFromTheFront()
        {
            // A vertical bar on (1, 1) and (1, 2), leaving up: its front edge is z = 6, the door piece's middle 6.5.
            var path = new ExitPath(BlockAt(new Cell(1, 1), new Cell(0, 0), new Cell(0, 1)), Direction.Up, ClipOffset);

            Assert.That(path.ClipPlane, Is.EqualTo(new Vector4(0f, 0f, 1f, 6.5f)));
            Assert.That(path.RowCount, Is.EqualTo(2));
            Assert.That(path.RowOf(new Cell(1, 2)), Is.EqualTo(0), "the front row");
            Assert.That(path.RowOf(new Cell(1, 1)), Is.EqualTo(1));
            Assert.That(path.TotalDistance, Is.EqualTo(2 * BoardLayout.CellSize + ClipOffset));
            Assert.That(path.CenterAtCut(new Cell(1, 1)).z, Is.EqualTo(6.5f), "each row bursts on the cut");
        }

        [Test]
        public void ExitPath_TowardLowerCells_CutsBeforeTheFrontEdge()
        {
            // A horizontal bar on (2, 1) and (3, 1), leaving left: its front edge is x = 4, so the cut is x = 3.5.
            var path = new ExitPath(BlockAt(new Cell(2, 1), new Cell(0, 0), new Cell(1, 0)), Direction.Left, ClipOffset);

            Assert.That(path.ClipPlane, Is.EqualTo(new Vector4(-1f, 0f, 0f, -3.5f)));
            Assert.That(path.RowOf(new Cell(2, 1)), Is.EqualTo(0));
            Assert.That(path.Rest, Is.EqualTo(new Vector3(4f, 0f, 2f)));
        }
    }
}
