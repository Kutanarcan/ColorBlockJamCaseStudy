using System.Collections.Generic;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class WallDrawRuleTests
    {
        private static (List<Placement> walls, List<Placement> corners) Dress(params string[] rows)
        {
            var walls = new List<Placement>();
            var corners = new List<Placement>();
            WallDrawRule.Build(new BoardLayout(LevelMap.Parse(Direction.Down, rows)), walls, corners);

            return (walls, corners);
        }

        [Test]
        public void WallDrawRule_DressesFrameInnerWallsAndDoors()
        {
            (List<Placement> walls, List<Placement> corners) = Dress(
                "#####",
                "#...#",
                "#.#.#",
                "#...#",
                "##0##");

            // Frame: 2 segments per cell edge facing an open cell (4 sides × 3 cells × 2), minus the 2 the door covers.
            Assert.That(walls.Count, Is.EqualTo(24 - 2));
            // Frame: 4 inner corners. Lone inner wall: 4 outer corners, a round pillar.
            Assert.That(corners.Count, Is.EqualTo(8));

            Assert.That(PlacementAssert.Has(corners, new Vector3(1.5f, 0f, 1.5f), 0f), "bottom-left frame corner");
            Assert.That(PlacementAssert.Has(corners, new Vector3(5.5f, 0f, 5.5f), 180f), "pillar, top-right");
            Assert.That(PlacementAssert.Has(walls, new Vector3(2.5f, 0f, 1.5f), 0f), "bottom frame, facing up");
            Assert.That(PlacementAssert.Has(walls, new Vector3(1.5f, 0f, 4.5f), 90f), "left frame, facing right");
            Assert.That(PlacementAssert.Has(walls, new Vector3(4.5f, 0f, 1.5f), 0f), Is.False, "covered by the door");
        }

        [Test]
        public void Notch_GetsOuterCornersOnTop_AndInnerCornersWhereItMeetsTheFrame()
        {
            (List<Placement> walls, List<Placement> corners) = Dress(
                "#####",
                "#...#",
                "#.#.#",
                "#####");

            Assert.That(PlacementAssert.Has(corners, new Vector3(4.5f, 0f, 3.5f), 90f), "tooth, top-left");
            Assert.That(PlacementAssert.Has(corners, new Vector3(5.5f, 0f, 3.5f), 180f), "tooth, top-right");
            Assert.That(PlacementAssert.Has(walls, new Vector3(4.5f, 0f, 2.5f), 270f), "tooth side, facing left");
            Assert.That(PlacementAssert.Has(corners, new Vector3(4.5f, 0f, 1.5f), 270f), "base, left of the tooth");
            Assert.That(PlacementAssert.Has(corners, new Vector3(5.5f, 0f, 1.5f), 0f), "base, right of the tooth");
        }

        [Test]
        public void SolidCellsAwayFromOpenCells_GetNothing()
        {
            (List<Placement> walls, List<Placement> corners) = Dress(
                "###",
                "###",
                "###");

            Assert.That(walls, Is.Empty);
            Assert.That(corners, Is.Empty);
        }
    }
}
