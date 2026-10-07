using System.Collections.Generic;

namespace Game.LevelEditor
{
    /// <summary>
    /// Every block and door color is in the game's palette (V1 D52): the game has no color for an id past its end.
    /// Discovered with the palette asset's size; tests give the size.
    /// </summary>
    public sealed class PaletteColorRule : ILevelRule
    {
        private readonly int? colorCount;

        public PaletteColorRule() { }

        public PaletteColorRule(int colorCount) => this.colorCount = colorCount;

        public void Check(LevelModel level, List<RuleViolation> violations)
        {
            int count = colorCount ?? PreviewColors.PaletteCount;

            for (int entity = 0; entity < level.EntityCount; entity++)
            {
                if (level.KindOf(entity) == EntityKind.Wall || level.CellsOf(entity).Count == 0)
                    continue;

                int colorId = level.ColorOf(entity);

                if (colorId < 0 || colorId >= count)
                    violations.Add(new RuleViolation(
                        $"Color {colorId} is not in the palette ({count} colors).", level.CellsOf(entity)[0]));
            }
        }
    }
}
