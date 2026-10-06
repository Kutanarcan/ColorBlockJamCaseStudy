using System.Collections.Generic;
using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class BlockDrawRuleTests
    {
        /// <summary>'#' = block cell, first row on top.</summary>
        private static List<BlockPart> Dress(params string[] rows)
        {
            var cells = new HashSet<Cell>();

            for (int row = 0; row < rows.Length; row++)
            {
                for (int x = 0; x < rows[row].Length; x++)
                {
                    if (rows[row][x] == '#')
                        cells.Add(new Cell(x, rows.Length - 1 - row));
                }
            }

            var parts = new List<BlockPart>();
            BlockDrawRule.Build(cells, parts);

            return parts;
        }

        private static int Count(List<BlockPart> parts, BlockPartKind kind)
        {
            int count = 0;

            for (int i = 0; i < parts.Count; i++)
            {
                if (parts[i].Kind == kind)
                    count++;
            }

            return count;
        }

        // Counts follow AlgorithmExplanation § BlockDrawRule: an InnerCorner covers three quadrants.
        [TestCase(new[] { "#.", "##" }, 5, 4, 0, 1, TestName = "L")]
        [TestCase(new[] { "###", ".#." }, 6, 4, 0, 2, TestName = "T")]
        [TestCase(new[] { "###", "#.#", "###" }, 4, 16, 0, 4, TestName = "Ring")]
        [TestCase(new[] { ".#.", "###", ".#." }, 8, 0, 0, 4, TestName = "Plus")]
        [TestCase(new[] { "##", "##" }, 4, 8, 4, 0, TestName = "Square")]
        public void BlockDrawRule_DressesLTURingAndPlus(string[] shape, int outer, int edge, int center, int inner)
        {
            List<BlockPart> parts = Dress(shape);

            Assert.That(Count(parts, BlockPartKind.OuterCorner), Is.EqualTo(outer), "OuterCorner");
            Assert.That(Count(parts, BlockPartKind.Edge), Is.EqualTo(edge), "Edge");
            Assert.That(Count(parts, BlockPartKind.Center), Is.EqualTo(center), "Center");
            Assert.That(Count(parts, BlockPartKind.InnerCorner), Is.EqualTo(inner), "InnerCorner");
        }

        [Test]
        public void SingleCell_GetsFourOuterCorners_PivotedOnTheCellCenter_FacingEachQuadrant()
        {
            List<BlockPart> parts = Dress("#");

            Assert.That(parts.Count, Is.EqualTo(4));
            var center = new Vector3(1f, 0f, 1f);

            foreach (float yaw in new[] { 0f, 90f, 180f, 270f })
            {
                Assert.That(parts.Exists(p => p.Kind == BlockPartKind.OuterCorner
                                              && PlacementAssert.Is(p.Placement, center, yaw)), yaw.ToString());
            }
        }

        [Test]
        public void InnerCorner_SitsOnTheVertex_FacingTheEmptyCell()
        {
            // L: cells (0,0), (0,1), (1,0); (1,1) is empty, the shared vertex is at (2, 0, 2).
            List<BlockPart> parts = Dress(
                "#.",
                "##");

            BlockPart inner = parts.Find(p => p.Kind == BlockPartKind.InnerCorner);
            Assert.That(PlacementAssert.Is(inner.Placement, new Vector3(2f, 0f, 2f), 0f), inner.ToString());
        }
    }
}
