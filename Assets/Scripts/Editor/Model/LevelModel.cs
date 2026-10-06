using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// The editor's working model, data-oriented: one owner array (cell index → entity id) and parallel
    /// per-entity lists. Entity ids stay stable while editing; an entity with no cells is gone and is
    /// skipped on save. Converted to and from LevelData only on load and save (LevelModelConverter).
    /// A modifier is never changed in place, only replaced (<see cref="ReplaceModifierAt"/>), so copies of the
    /// model can share modifier instances. Lookups by kind live in <see cref="LevelModelQueries"/>.
    /// </summary>
    public sealed class LevelModel
    {
        public const int None = -1;

        private readonly int[] owners;
        private readonly List<EntityKind> kinds = new List<EntityKind>();
        private readonly List<int> colorIds = new List<int>();
        private readonly List<Direction> directions = new List<Direction>();
        private readonly List<List<int>> cells = new List<List<int>>();
        private readonly List<List<ModifierData>> modifiers = new List<List<ModifierData>>();

        public int Width { get; }
        public int Height { get; }
        public float TimeLimit { get; set; }

        /// <summary>Every id ever created, including entities that lost all their cells.</summary>
        public int EntityCount => kinds.Count;

        public LevelModel(int width, int height)
        {
            Width = width;
            Height = height;
            owners = new int[width * height];

            for (int i = 0; i < owners.Length; i++)
            {
                owners[i] = None;
            }
        }

        /// <summary>The same entities (ids, colors, directions, modifiers) on an empty grid of the given size.</summary>
        private LevelModel(LevelModel source, int width, int height) : this(width, height)
        {
            TimeLimit = source.TimeLimit;
            kinds.AddRange(source.kinds);
            colorIds.AddRange(source.colorIds);
            directions.AddRange(source.directions);

            for (int id = 0; id < source.kinds.Count; id++)
            {
                cells.Add(new List<int>());
                modifiers.Add(new List<ModifierData>(source.modifiers[id]));
            }
        }

        public bool Contains(Cell cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;
        public int IndexOf(Cell cell) => cell.Y * Width + cell.X;
        public Cell CellOf(int index) => new Cell(index % Width, index / Width);
        public int OwnerOf(Cell cell) => owners[IndexOf(cell)];

        /// <summary>An independent copy with the same entity ids, for the undo history.</summary>
        public LevelModel Clone()
        {
            var copy = new LevelModel(this, Width, Height);
            Array.Copy(owners, copy.owners, owners.Length);

            for (int id = 0; id < cells.Count; id++)
            {
                copy.cells[id].AddRange(cells[id]);
            }

            return copy;
        }

        /// <summary>The same entities, without cells, on a grid of another size (the resizer paints them back).</summary>
        public LevelModel WithSize(int width, int height) => new LevelModel(this, width, height);

        public int AddWall() => AddEntity(EntityKind.Wall, 0, default);
        public int AddDoor(int colorId, Direction direction) => AddEntity(EntityKind.Door, colorId, direction);
        public int AddBlock(int colorId) => AddEntity(EntityKind.Block, colorId, default);

        /// <summary>Gives the cell to the entity, taking it from its previous owner.</summary>
        public void Paint(Cell cell, int entity)
        {
            int index = IndexOf(cell);

            if (owners[index] == entity)
                return;

            Release(index);
            owners[index] = entity;
            cells[entity].Add(index);
        }

        public void Erase(Cell cell) => Release(IndexOf(cell));

        public void Delete(int entity)
        {
            List<int> owned = cells[entity];

            for (int i = 0; i < owned.Count; i++)
            {
                owners[owned[i]] = None;
            }

            owned.Clear();
        }

        public bool Exists(int entity) => cells[entity].Count > 0;
        public EntityKind KindOf(int entity) => kinds[entity];
        public int ColorOf(int entity) => colorIds[entity];
        public Direction DirectionOf(int entity) => directions[entity];
        public IReadOnlyList<int> CellsOf(int entity) => cells[entity];
        public IReadOnlyList<ModifierData> ModifiersOf(int entity) => modifiers[entity];

        public void SetColor(int entity, int colorId) => colorIds[entity] = colorId;
        public void SetDirection(int entity, Direction direction) => directions[entity] = direction;
        public void AddModifier(int entity, ModifierData modifier) => modifiers[entity].Add(modifier);
        public void RemoveModifierAt(int entity, int index) => modifiers[entity].RemoveAt(index);
        public void ReplaceModifierAt(int entity, int index, ModifierData modifier) => modifiers[entity][index] = modifier;

        private int AddEntity(EntityKind kind, int colorId, Direction direction)
        {
            kinds.Add(kind);
            colorIds.Add(colorId);
            directions.Add(direction);
            cells.Add(new List<int>());
            modifiers.Add(new List<ModifierData>());

            return kinds.Count - 1;
        }

        private void Release(int index)
        {
            int previous = owners[index];

            if (previous == None)
                return;

            cells[previous].Remove(index);
            owners[index] = None;
        }
    }
}
