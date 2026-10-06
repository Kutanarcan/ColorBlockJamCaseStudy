using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class StrokeTests
    {
        private static readonly Brush RedBlock = new Brush(EntityKind.Block, 0, Direction.Down);
        private static readonly Brush BlueBlock = new Brush(EntityKind.Block, 1, Direction.Down);

        private Stroke stroke;

        [SetUp]
        public void CreateStroke() => stroke = new Stroke();

        // 'A' is a red block at (1, 3); '1' a red door facing down at (1, 0) and (2, 0).
        private static LevelModel Level() =>
            AsciiModel.Parse(
                    "######",
                    "#A...#",
                    "#....#",
                    "#....#",
                    "#11###")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

        [Test]
        public void Paint_OnAnEmptyCell_CreatesOneEntity()
        {
            LevelModel model = Level();
            int before = model.EntityCount;

            stroke.Begin(model, EditorTool.Paint, RedBlock, new Cell(3, 2), LevelModel.None);

            Assert.That(model.EntityCount, Is.EqualTo(before + 1));
            Assert.That(model.OwnerOf(new Cell(3, 2)), Is.EqualTo(stroke.Target));
            Assert.That(stroke.Changed, Is.True);
        }

        [Test]
        public void Paint_StartingOnAMatchingEntity_ExtendsIt()
        {
            LevelModel model = Level();
            int block = model.OwnerOf(new Cell(1, 3));
            int before = model.EntityCount;

            stroke.Begin(model, EditorTool.Paint, RedBlock, new Cell(1, 3), LevelModel.None);
            stroke.Continue(new Cell(2, 3));

            Assert.That(model.EntityCount, Is.EqualTo(before));
            Assert.That(model.OwnerOf(new Cell(2, 3)), Is.EqualTo(block));
        }

        [Test]
        public void Paint_ClickOnAMatchingEntity_ChangesNothing()
        {
            LevelModel model = Level();
            int before = model.EntityCount;

            stroke.Begin(model, EditorTool.Paint, RedBlock, new Cell(1, 3), LevelModel.None);

            Assert.That(model.EntityCount, Is.EqualTo(before));
            Assert.That(stroke.Changed, Is.False);
        }

        [Test]
        public void Paint_StartingOnAnotherEntity_PaintsANewEntityOverIt()
        {
            LevelModel model = Level();
            int red = model.OwnerOf(new Cell(1, 3));

            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(1, 3), LevelModel.None);

            Assert.That(stroke.Target, Is.Not.EqualTo(red));
            Assert.That(model.OwnerOf(new Cell(1, 3)), Is.EqualTo(stroke.Target));
            Assert.That(model.Exists(red), Is.False);
        }

        [Test]
        public void DoorStartedOnTheEdge_PointsOutward_WhateverTheBrushSays()
        {
            LevelModel model = Level();

            stroke.Begin(model, EditorTool.Paint, new Brush(EntityKind.Door, 2, Direction.Up), new Cell(3, 0), LevelModel.None);

            Assert.That(model.DirectionOf(stroke.Target), Is.EqualTo(Direction.Down));
        }

        [Test]
        public void DoorStartedInside_TakesTheBrushDirection()
        {
            LevelModel model = Level();

            stroke.Begin(model, EditorTool.Paint, new Brush(EntityKind.Door, 2, Direction.Left), new Cell(2, 2), LevelModel.None);

            Assert.That(model.DirectionOf(stroke.Target), Is.EqualTo(Direction.Left));
        }

        [Test]
        public void StrokeOnAnEdgeDoor_ExtendsIt_WhateverTheBrushSays()
        {
            LevelModel model = Level();
            int door = model.OwnerOf(new Cell(1, 0));

            stroke.Begin(model, EditorTool.Paint, new Brush(EntityKind.Door, 0, Direction.Up), new Cell(1, 0), LevelModel.None);

            Assert.That(stroke.Target, Is.EqualTo(door));
        }

        [Test]
        public void InnerDoorBrush_FacingAnotherWay_DoesNotExtendTheDoor()
        {
            LevelModel model = AsciiModel.Parse(
                    "######",
                    "#....#",
                    "#.22.#",
                    "#11###")
                .Door('1', 0, Direction.Down)
                .Door('2', 0, Direction.Down)
                .Build();
            int inner = model.OwnerOf(new Cell(2, 1));

            stroke.Begin(model, EditorTool.Paint, new Brush(EntityKind.Door, 0, Direction.Up), new Cell(2, 1), LevelModel.None);

            Assert.That(stroke.Target, Is.Not.EqualTo(inner));
        }

        [Test]
        public void FastDrag_PaintsOneConnectedShape()
        {
            LevelModel model = Level();

            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(1, 1), LevelModel.None);
            stroke.Continue(new Cell(4, 3));

            Assert.That(model.CellsOf(stroke.Target).Count, Is.EqualTo(6));
            Assert.That(RuleCheck.Run(new ConnectedBlockRule(), model), Is.Empty);
        }

        [Test]
        public void Drag_OutsideTheGrid_SkipsThoseCells()
        {
            LevelModel model = Level();

            stroke.Begin(model, EditorTool.Paint, new Brush(EntityKind.Wall, 0, Direction.Down), new Cell(2, 2), LevelModel.None);
            stroke.Continue(new Cell(-3, 2));

            Assert.That(model.OwnerOf(new Cell(1, 2)), Is.EqualTo(stroke.Target));
        }

        [Test]
        public void BlockBrush_SkipsTheFrame()
        {
            LevelModel model = Level();
            int before = model.EntityCount;

            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(0, 2), LevelModel.None);
            Assert.That(model.EntityCount, Is.EqualTo(before));

            stroke.Continue(new Cell(2, 2));
            Assert.That(model.CellsOf(stroke.Target).Count, Is.EqualTo(2));
            Assert.That(model.KindOf(model.OwnerOf(new Cell(0, 2))), Is.EqualTo(EntityKind.Wall));
        }

        [Test]
        public void ErasingAFrameDoor_TurnsItIntoAWall_AndAFrameWallStays()
        {
            LevelModel model = Level();
            int wall = model.OwnerOf(new Cell(0, 0));

            stroke.Begin(model, EditorTool.Erase, RedBlock, new Cell(1, 0), LevelModel.None);
            Assert.That(model.OwnerOf(new Cell(1, 0)), Is.EqualTo(wall));

            stroke.Begin(model, EditorTool.Erase, RedBlock, new Cell(0, 1), LevelModel.None);
            Assert.That(model.OwnerOf(new Cell(0, 1)), Is.EqualTo(wall));
            Assert.That(stroke.Changed, Is.False);
        }

        [Test]
        public void Erase_ClearsEveryCellOnThePath_AndCreatesNothing()
        {
            LevelModel model = Level();
            int before = model.EntityCount;

            stroke.Begin(model, EditorTool.Erase, RedBlock, new Cell(1, 1), LevelModel.None);
            stroke.Continue(new Cell(1, 3));

            for (int y = 1; y <= 3; y++)
            {
                Assert.That(model.OwnerOf(new Cell(1, y)), Is.EqualTo(LevelModel.None));
            }

            Assert.That(model.EntityCount, Is.EqualTo(before));
        }

        [Test]
        public void StrokeStartedNextToTheSelectedEntity_ExtendsIt_SoATIsOneBlock()
        {
            LevelModel model = Level();
            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(1, 2), LevelModel.None);
            stroke.Continue(new Cell(3, 2));
            int bar = stroke.Target;

            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(2, 1), bar);

            Assert.That(stroke.Target, Is.EqualTo(bar));
            Assert.That(model.CellsOf(bar).Count, Is.EqualTo(4));
        }

        [Test]
        public void StrokeStartedAwayFromTheSelectedEntity_StartsANewOne()
        {
            LevelModel model = Level();
            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(1, 2), LevelModel.None);
            int first = stroke.Target;

            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(4, 1), first);

            Assert.That(stroke.Target, Is.Not.EqualTo(first));
        }

        [Test]
        public void SelectedEntityOfAnotherColor_IsNotExtended()
        {
            LevelModel model = Level();
            int red = model.OwnerOf(new Cell(1, 3));

            stroke.Begin(model, EditorTool.Paint, BlueBlock, new Cell(2, 3), red);

            Assert.That(stroke.Target, Is.Not.EqualTo(red));
        }
    }
}
