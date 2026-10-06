using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class BoardRaycastTests
    {
        [Test]
        public void Raycast_HitsTheGround_InCellUnits()
        {
            Assert.That(BoardRaycast.TryGetCellPoint(new Ray(new Vector3(3f, 10f, 5f), Vector3.down), out Vector2 cellPoint));
            Assert.That(cellPoint, Is.EqualTo(new Vector2(1.5f, 2.5f)));
            Assert.That(BoardRaycast.ToCell(cellPoint), Is.EqualTo(new Cell(1, 2)));
        }

        [Test]
        public void Raycast_LeftOfTheBoard_FloorsToANegativeCell()
        {
            BoardRaycast.TryGetCellPoint(new Ray(new Vector3(-1f, 10f, 1f), Vector3.down), out Vector2 cellPoint);

            Assert.That(BoardRaycast.ToCell(cellPoint), Is.EqualTo(new Cell(-1, 0)));
        }

        [Test]
        public void Raycast_AwayFromTheGround_HitsNothing()
        {
            Assert.That(BoardRaycast.TryGetCellPoint(new Ray(new Vector3(0f, 10f, 0f), Vector3.up), out _), Is.False);
        }
    }
}
