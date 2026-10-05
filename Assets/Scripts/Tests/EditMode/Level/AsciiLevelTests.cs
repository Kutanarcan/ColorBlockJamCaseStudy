using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class AsciiLevelTests
    {
        [Test]
        public void Parse_TopRow_IsHighestY()
        {
            LevelData level = AsciiLevel.Parse(
                    "A.",
                    "..")
                .Block('A', 0)
                .Build();

            Assert.That(level.Blocks[0].Cells, Is.EqualTo(new[] { new Cell(0, 1) }));
        }

        [Test]
        public void Parse_UndeclaredSymbol_Throws()
        {
            AsciiLevel ascii = AsciiLevel.Parse("A.");

            Assert.Throws<ArgumentException>(() => ascii.Build());
        }
    }
}
