using Game.Core;
using Game.LevelEditor;

namespace Game.Tests.LevelEditor
{
    /// <summary>Test-only: sessions on small levels, edited the way the window edits them.</summary>
    internal static class TestSessions
    {
        /// <summary>A new 5 x 4 level with a wall ring.</summary>
        public static LevelEditorSession New(params ILevelRule[] rules)
        {
            var session = new LevelEditorSession(ModifierCatalog.Default(), new LevelRules(rules));
            session.New(5, 4, 30f);

            return session;
        }

        /// <summary>The new 5 x 4 level with a blue door at (2, 0) and a selected blue block above it, keyed "level-01".</summary>
        public static LevelEditorSession Playable(params ILevelRule[] rules)
        {
            LevelEditorSession session = New(rules);
            session.ChooseBrush(new Brush(EntityKind.Door, 1, Direction.Down));
            Click(session, new Cell(2, 0));
            session.ChooseBrush(new Brush(EntityKind.Block, 1, Direction.Down));
            Click(session, new Cell(2, 1));
            session.Document.Key = "level-01";

            return session;
        }

        public static void Click(LevelEditorSession session, Cell cell)
        {
            session.BeginStroke(cell);
            session.EndStroke();
        }
    }
}
