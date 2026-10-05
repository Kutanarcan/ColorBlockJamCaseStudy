using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class GridTests
    {
        [Test]
        public void NewGrid_IsEmpty()
        {
            var grid = new Grid(3, 2);

            for (int y = 0; y < 2; y++)
            for (int x = 0; x < 3; x++)
                Assert.That(grid.Get(new Cell(x, y)), Is.EqualTo(Grid.Empty));
        }

        [Test]
        public void IndexOf_IsRowMajor_AndRoundTripsWithCellOf()
        {
            var grid = new Grid(3, 2);

            Assert.That(grid.IndexOf(new Cell(2, 1)), Is.EqualTo(5));
            Assert.That(grid.CellOf(5), Is.EqualTo(new Cell(2, 1)));
        }
    }
}
