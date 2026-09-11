using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class MatchFinderTests
    {
        [Test]
        public void FindMatches_OnWideBoard_ReturnsHorizontalStraightMatchThree()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RRRGB",
                "GBYRG",
                "BYGBY"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            AssertPattern(
                patterns[0],
                0,
                MatchShape.Straight,
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0));
        }

        [Test]
        public void FindMatches_OnTallBoard_ReturnsVerticalStraightMatchThree()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RGB",
                "RBY",
                "RYG",
                "GBR"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            AssertPattern(
                patterns[0],
                0,
                MatchShape.Straight,
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2));
        }

        [Test]
        public void FindMatches_ReturnsHorizontalMatchFourAsSingleStraightPattern()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RRRRG",
                "GBYGB",
                "BYGBY"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.Straight));
            Assert.That(patterns[0].Size, Is.EqualTo(4));
        }

        [Test]
        public void FindMatches_ReturnsHorizontalMatchFiveAsSingleStraightPattern()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RRRRR",
                "GBYGB",
                "BYGBY"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.Straight));
            Assert.That(patterns[0].Size, Is.EqualTo(5));
        }

        [Test]
        public void FindMatches_ReturnsVerticalMatchFourAsSingleStraightPattern()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RGB",
                "RYG",
                "RBR",
                "RGY",
                "YRB"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.Straight));
            Assert.That(patterns[0].Size, Is.EqualTo(4));
        }

        [Test]
        public void FindMatches_ReturnsVerticalMatchFiveAsSingleStraightPattern()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RGB",
                "RYG",
                "RBR",
                "RGY",
                "RYB"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.Straight));
            Assert.That(patterns[0].Size, Is.EqualTo(5));
        }

        [Test]
        public void FindMatches_WithIndependentRunsOfSameType_ReturnsSeparatePatterns()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RRRGB",
                "GBYRG",
                "BGRRR"
            );

            Assert.That(patterns, Has.Count.EqualTo(2));
            Assert.That(patterns.All(pattern => pattern.TileType == 0), Is.True);
            Assert.That(patterns.All(pattern => pattern.Shape == MatchShape.Straight), Is.True);
        }

        [Test]
        public void FindMatches_WithEndpointIntersectionOnBothRuns_ReturnsLWithUniqueCells()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "GBRGB",
                "BYRYG",
                "YGRRR",
                "RGBYG"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            AssertPattern(
                patterns[0],
                0,
                MatchShape.L,
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2));
        }

        [Test]
        public void FindMatches_WithVerticalEndpointAndHorizontalInteriorIntersection_ReturnsT()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "GBYGB",
                "GRRRG",
                "BYRBY",
                "YGRYG",
                "RGBYR"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.T));
        }

        [Test]
        public void FindMatches_WithHorizontalEndpointAndVerticalInteriorIntersection_ReturnsT()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "GBYGB",
                "YBRGY",
                "RRRYG",
                "BYRBY",
                "RGBYR"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.T));
        }

        [Test]
        public void FindMatches_WithInteriorIntersectionOnBothRuns_ReturnsCrossWithUniqueCells()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "GBYGB",
                "YBRGY",
                "GRRRG",
                "BYRBY",
                "RGBYR"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.Cross));
            Assert.That(patterns[0].Size, Is.EqualTo(5));
            Assert.That(patterns[0].Cells, Is.EquivalentTo(new[]
            {
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(2, 1),
                new Vector2Int(2, 3)
            }));
        }

        [Test]
        public void FindMatches_WithTransitiveIntersections_ReturnsOneComplexPattern()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "GBYGB",
                "RRRGB",
                "BYRYG",
                "GBRRR",
                "YGBYG"
            );

            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Shape, Is.EqualTo(MatchShape.Complex));
            Assert.That(patterns[0].Size, Is.EqualTo(7));
        }

        [Test]
        public void FindMatches_WithNoRuns_ReturnsEmptyCollection()
        {
            IReadOnlyList<MatchPattern> patterns = FindPatterns(
                "RGBY",
                "GBYR",
                "BYRG"
            );

            Assert.That(patterns, Is.Empty);
        }

        private static IReadOnlyList<MatchPattern> FindPatterns(params string[] rows)
        {
            Board board = BoardFixture.Create(rows);
            return MatchFinder.FindMatches(board);
        }

        private static void AssertPattern(
            MatchPattern pattern,
            int expectedTileType,
            MatchShape expectedShape,
            params Vector2Int[] expectedCells)
        {
            Assert.That(pattern.TileType, Is.EqualTo(expectedTileType));
            Assert.That(pattern.Shape, Is.EqualTo(expectedShape));
            Assert.That(pattern.Size, Is.EqualTo(expectedCells.Length));
            Assert.That(pattern.Cells, Is.EquivalentTo(expectedCells));
        }
    }
}
