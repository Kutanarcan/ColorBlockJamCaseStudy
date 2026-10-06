using System;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class ArrowPlacementRuleTests
    {
        private const int MaxSize = 3;

        private static Block BlockOf(params Cell[] cells) =>
            new Block(0, new Cell(0, 0), cells, 0, Array.Empty<IModifier>());

        [Test]
        public void VerticalL_ArrowRunsThroughTheCentralCell_AlongItsColumn()
        {
            // AlgorithmExplanation § Arrow placement: anchor (0,1), run (0,0)–(0,2) → Arrow_3 at (1, 0, 3).
            Block block = BlockOf(new Cell(0, 0), new Cell(0, 1), new Cell(0, 2), new Cell(1, 0));

            ArrowPlacement arrow = ArrowPlacementRule.Of(block, Direction.Up, MaxSize);

            Assert.That(arrow.Size, Is.EqualTo(3));
            Assert.That(PlacementAssert.Is(arrow.Placement, new Vector3(1f, 0f, 3f), 0f), arrow.ToString());
        }

        [Test]
        public void Bar_ArrowFacesItsDirection()
        {
            Block block = BlockOf(new Cell(0, 0), new Cell(1, 0));

            ArrowPlacement arrow = ArrowPlacementRule.Of(block, Direction.Left, MaxSize);

            Assert.That(arrow.Size, Is.EqualTo(2));
            Assert.That(PlacementAssert.Is(arrow.Placement, new Vector3(2f, 0f, 1f), 270f), arrow.ToString());
        }

        [Test]
        public void LongRun_IsCappedAtTheLongestMesh()
        {
            Block block = BlockOf(new Cell(0, 0), new Cell(1, 0), new Cell(2, 0), new Cell(3, 0), new Cell(4, 0));

            Assert.That(ArrowPlacementRule.Of(block, Direction.Right, MaxSize).Size, Is.EqualTo(MaxSize));
        }
    }
}
