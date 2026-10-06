using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class DirectionExtensionsTests
    {
        [TestCase(Direction.Up, 0, 1)]
        [TestCase(Direction.Down, 0, -1)]
        [TestCase(Direction.Left, -1, 0)]
        [TestCase(Direction.Right, 1, 0)]
        public void ToOffset_IsOneCell_AlongTheDirection(Direction direction, int x, int y)
        {
            Assert.That(direction.ToOffset(), Is.EqualTo(new Cell(x, y)));
        }

        [TestCase(Direction.Up, Axis.Vertical)]
        [TestCase(Direction.Down, Axis.Vertical)]
        [TestCase(Direction.Left, Axis.Horizontal)]
        [TestCase(Direction.Right, Axis.Horizontal)]
        public void ToAxis_IsTheLineTheDirectionMovesOn(Direction direction, Axis axis)
        {
            Assert.That(direction.ToAxis(), Is.EqualTo(axis));
        }
    }
}
