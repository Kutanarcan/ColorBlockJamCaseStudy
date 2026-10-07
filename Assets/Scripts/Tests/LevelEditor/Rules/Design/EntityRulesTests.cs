using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class EntityRulesTests
    {
        [Test]
        public void DoorWithAGap_IsNotStraight()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "#1#1#")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new StraightDoorRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void DoorAlongItsDirection_IsNotStraight()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "1A..#",
                    "1####")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new StraightDoorRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void StraightDoor_Passes()
        {
            LevelModel model = AsciiModel.Parse(
                    "######",
                    "#AA..#",
                    "#11###")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new StraightDoorRule(), model), Is.Empty);
        }

        [Test]
        public void DiagonalOnlyBlock_IsNotConnected()
        {
            LevelModel model = AsciiModel.Parse(
                    "####",
                    "#A.#",
                    "#.A#",
                    "####")
                .Block('A', 0)
                .Build();

            Assert.That(RuleCheck.Run(new ConnectedBlockRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void LShapedBlock_IsConnected()
        {
            LevelModel model = AsciiModel.Parse(
                    "####",
                    "#A.#",
                    "#AA#",
                    "####")
                .Block('A', 0)
                .Build();

            Assert.That(RuleCheck.Run(new ConnectedBlockRule(), model), Is.Empty);
        }

        [Test]
        public void BlockWiderThanEveryDoor_Violates()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#AA.#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new DoorWidthRule(), model).Count, Is.EqualTo(1));
        }

        [Test]
        public void DoorRun_MaySpanSeveralDoorEntities()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#AA.#",
                    "#12##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Door('2', 0, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new DoorWidthRule(), model), Is.Empty);
        }

        [Test]
        public void DoorOfAnotherColor_DoesNotCount()
        {
            LevelModel model = AsciiModel.Parse(
                    "#####",
                    "#A..#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 3, Direction.Down)
                .Build();

            Assert.That(RuleCheck.Run(new DoorWidthRule(), model).Count, Is.EqualTo(1));
        }
    }
}
