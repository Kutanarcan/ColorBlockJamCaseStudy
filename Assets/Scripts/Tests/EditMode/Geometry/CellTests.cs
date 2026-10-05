using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class CellTests
    {
        [Test]
        public void Addition_And_Subtraction_AreComponentWise()
        {
            var a = new Cell(2, 3);
            var b = new Cell(1, -1);

            Assert.That(a + b, Is.EqualTo(new Cell(3, 2)));
            Assert.That(a - b, Is.EqualTo(new Cell(1, 4)));
        }

        [Test]
        public void Equality_ComparesCoordinates()
        {
            Assert.That(new Cell(1, 2) == new Cell(1, 2), Is.True);
            Assert.That(new Cell(1, 2) != new Cell(2, 1), Is.True);
        }
    }
}
