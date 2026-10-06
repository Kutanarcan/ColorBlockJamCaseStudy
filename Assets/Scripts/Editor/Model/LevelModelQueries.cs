using System.Collections.Generic;

namespace Game.LevelEditor
{
    /// <summary>Lookups over the existing entities of a level by kind. Entities without cells are skipped.</summary>
    public static class LevelModelQueries
    {
        /// <summary>Fills <paramref name="result"/> with the existing entities of one kind, in id order.</summary>
        public static void CollectEntities(this LevelModel level, EntityKind kind, List<int> result)
        {
            result.Clear();

            for (int id = 0; id < level.EntityCount; id++)
            {
                if (level.KindOf(id) == kind && level.Exists(id))
                    result.Add(id);
            }
        }

        /// <summary>The entity's 1-based place among the existing entities of its kind. For display: ids are internal.</summary>
        public static int OrdinalOf(this LevelModel level, int entity)
        {
            int ordinal = 0;

            for (int id = 0; id <= entity; id++)
            {
                if (level.KindOf(id) == level.KindOf(entity) && level.Exists(id))
                    ordinal++;
            }

            return ordinal;
        }

        public static int CountOf(this LevelModel level, EntityKind kind)
        {
            int count = 0;

            for (int id = 0; id < level.EntityCount; id++)
            {
                if (level.KindOf(id) == kind && level.Exists(id))
                    count++;
            }

            return count;
        }
    }
}
