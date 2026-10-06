using System;
using System.Collections.Generic;
using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class CellLineTests
    {
        private readonly List<Cell> line = new List<Cell>();

        [Test]
        public void Fill_StraightLine_ListsEveryCellAfterTheStart()
        {
            CellLine.Fill(new Cell(1, 2), new Cell(4, 2), line);

            Assert.That(line, Is.EqualTo(new[] { new Cell(2, 2), new Cell(3, 2), new Cell(4, 2) }));
        }

        [Test]
        public void Fill_SameCell_IsEmpty()
        {
            CellLine.Fill(new Cell(1, 1), new Cell(1, 1), line);

            Assert.That(line, Is.Empty);
        }

        [TestCase(0, 0, 3, 2)]
        [TestCase(5, 5, 1, 2)]
        [TestCase(2, 0, 0, 6)]
        public void Fill_Jump_StepsThroughSideNeighbours_ToTheEnd(int fromX, int fromY, int toX, int toY)
        {
            var from = new Cell(fromX, fromY);
            var to = new Cell(toX, toY);

            CellLine.Fill(from, to, line);

            Assert.That(line.Count, Is.EqualTo(Math.Abs(toX - fromX) + Math.Abs(toY - fromY)));
            Assert.That(line[line.Count - 1], Is.EqualTo(to));

            Cell previous = from;

            foreach (Cell cell in line)
            {
                Assert.That(Math.Abs(cell.X - previous.X) + Math.Abs(cell.Y - previous.Y), Is.EqualTo(1));
                previous = cell;
            }
        }
    }
}
