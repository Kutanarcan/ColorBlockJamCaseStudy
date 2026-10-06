using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>Starting points for a new level, and the sizes a level can have in the editor.</summary>
    public static class LevelTemplates
    {
        /// <summary>The smallest size with one inside line between two edges.</summary>
        public const int MinSize = 3;

        public const int MaxSize = 30;

        /// <summary>An empty level whose edge is one wall entity, so the edge rule already holds.</summary>
        public static LevelModel WalledLevel(int width, int height, float timeLimit)
        {
            var model = new LevelModel(width, height) { TimeLimit = timeLimit };
            int wall = model.AddWall();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                        model.Paint(new Cell(x, y), wall);
                }
            }

            return model;
        }
    }
}
