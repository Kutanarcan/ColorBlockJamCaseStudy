using System.Collections.Generic;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class DoorDrawRuleTests
    {
        private static List<DoorRun> Dress(Direction direction, params string[] rows)
        {
            var runs = new List<DoorRun>();
            DoorDrawRule.Build(new BoardLayout(LevelMap.Parse(direction, rows)), runs);

            return runs;
        }

        private static void AssertRun(DoorRun run, Vector3 center, float yaw, int length, int colorId)
        {
            Assert.That(PlacementAssert.Is(run.Placement, center, yaw), Is.True, run.ToString());
            Assert.That(run.Length, Is.EqualTo(length));
            Assert.That(run.ColorId, Is.EqualTo(colorId));
        }

        [Test]
        public void DoorDrawRule_OnePiecePerSameColorRun_CenteredAndAsLongAsTheRun()
        {
            // Two color-0 door entities side by side form one run; colors 1 and 2 are runs of their own.
            List<DoorRun> runs = Dress(Direction.Down,
                "######",
                "#....#",
                "#0012#");

            Assert.That(runs.Count, Is.EqualTo(3));
            AssertRun(runs[0], new Vector3(4f, 0f, 1.5f), 0f, 2, 0);
            AssertRun(runs[1], new Vector3(7f, 0f, 1.5f), 0f, 1, 1);
            AssertRun(runs[2], new Vector3(9f, 0f, 1.5f), 0f, 1, 2);
        }

        [Test]
        public void LeftDoors_RunAlongTheColumn_AndFaceRight()
        {
            List<DoorRun> runs = Dress(Direction.Left,
                "###",
                "0..",
                "0..",
                "###");

            Assert.That(runs.Count, Is.EqualTo(1));
            AssertRun(runs[0], new Vector3(1.5f, 0f, 4f), 90f, 2, 0);
        }
    }
}
