using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelTemplatesTests
    {
        [Test]
        public void WalledLevel_IsOneWallOnEveryEdgeCell_AndEmptyInside()
        {
            LevelModel level = LevelTemplates.WalledLevel(5, 4, 30f);

            Assert.That(level.CountOf(EntityKind.Wall), Is.EqualTo(1));
            Assert.That(level.CellsOf(level.OwnerOf(new Cell(0, 0))).Count, Is.EqualTo(2 * 5 + 2 * 4 - 4));
            Assert.That(level.OwnerOf(new Cell(2, 2)), Is.EqualTo(LevelModel.None));
            Assert.That(RuleCheck.Run(new EdgeCellsRule(), level), Is.Empty);
        }
    }
}
