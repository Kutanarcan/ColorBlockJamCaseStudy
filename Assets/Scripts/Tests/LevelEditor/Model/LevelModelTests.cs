using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelModelTests
    {
        [Test]
        public void Paint_GivesTheCellToTheEntity()
        {
            var model = new LevelModel(3, 3);
            int block = model.AddBlock(1);

            model.Paint(new Cell(1, 1), block);

            Assert.That(model.OwnerOf(new Cell(1, 1)), Is.EqualTo(block));
            Assert.That(model.CellsOf(block), Is.EqualTo(new[] { model.IndexOf(new Cell(1, 1)) }));
        }

        [Test]
        public void Paint_TakesTheCellFromItsPreviousOwner()
        {
            var model = new LevelModel(3, 3);
            int first = model.AddBlock(1);
            int second = model.AddBlock(2);
            model.Paint(new Cell(1, 1), first);

            model.Paint(new Cell(1, 1), second);

            Assert.That(model.OwnerOf(new Cell(1, 1)), Is.EqualTo(second));
            Assert.That(model.CellsOf(first), Is.Empty);
        }

        [Test]
        public void Erase_LastCell_RemovesTheEntity()
        {
            var model = new LevelModel(3, 3);
            int block = model.AddBlock(1);
            model.Paint(new Cell(1, 1), block);

            model.Erase(new Cell(1, 1));

            Assert.That(model.Exists(block), Is.False);
            Assert.That(model.OwnerOf(new Cell(1, 1)), Is.EqualTo(LevelModel.None));
        }


        [Test]
        public void Clone_KeepsIds_AndIsIndependent()
        {
            var model = new LevelModel(3, 3) { TimeLimit = 12f };
            int block = model.AddBlock(2);
            model.Paint(new Cell(1, 1), block);
            model.AddModifier(block, new IceData { Count = 1 });

            LevelModel copy = model.Clone();
            model.Erase(new Cell(1, 1));
            model.RemoveModifierAt(block, 0);

            Assert.That(copy.OwnerOf(new Cell(1, 1)), Is.EqualTo(block));
            Assert.That(copy.ColorOf(block), Is.EqualTo(2));
            Assert.That(copy.ModifiersOf(block).Count, Is.EqualTo(1));
            Assert.That(copy.TimeLimit, Is.EqualTo(12f));
        }

        [Test]
        public void WithSize_KeepsTheEntities_WithoutCells()
        {
            var model = new LevelModel(3, 3);
            int door = model.AddDoor(4, Direction.Left);
            model.Paint(new Cell(0, 1), door);

            LevelModel resized = model.WithSize(5, 4);

            Assert.That(resized.Width, Is.EqualTo(5));
            Assert.That(resized.EntityCount, Is.EqualTo(model.EntityCount));
            Assert.That(resized.DirectionOf(door), Is.EqualTo(Direction.Left));
            Assert.That(resized.Exists(door), Is.False);
        }
    }
}
