using System.Collections.Generic;
using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelModelQueriesTests
    {
        [Test]
        public void CollectEntities_ReturnsExistingEntitiesOfOneKind()
        {
            LevelModel model = AsciiModel.Parse(
                    "###",
                    "#A#",
                    "#1#")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();
            model.AddBlock(5);
            var blocks = new List<int>();

            model.CollectEntities(EntityKind.Block, blocks);

            Assert.That(blocks.Count, Is.EqualTo(1));
            Assert.That(model.CellOf(model.CellsOf(blocks[0])[0]), Is.EqualTo(new Cell(1, 1)));
        }

        [Test]
        public void OrdinalOf_CountsOnlyExistingEntitiesOfTheSameKind()
        {
            var model = new LevelModel(4, 1);
            int first = model.AddBlock(0);
            model.AddBlock(0);
            model.AddWall();
            int last = model.AddBlock(0);
            model.Paint(new Cell(0, 0), first);
            model.Paint(new Cell(1, 0), last);

            Assert.That(model.OrdinalOf(last), Is.EqualTo(2));
            Assert.That(model.CountOf(EntityKind.Block), Is.EqualTo(2));
        }
    }
}
