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
        public void FindMatches_OnWideBoard_ReturnsHorizontalMatchThree()
        {
            Board board = BoardFixture.Create(
                "RRRGB",
                "GBYRG",
                "BYGBY"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(1));
            AssertMatch(
                matches[0],
                0,
                MatchOrientation.Horizontal,
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0));
        }

        [Test]
        public void FindMatches_OnTallBoard_ReturnsVerticalMatchThree()
        {
            Board board = BoardFixture.Create(
                "RGB",
                "RBY",
                "RYG",
                "GBR"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(1));
            AssertMatch(
                matches[0],
                0,
                MatchOrientation.Vertical,
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2));
        }

        [Test]
        public void FindMatches_ReturnsHorizontalMatchFourAsSingleRun()
        {
            Board board = BoardFixture.Create(
                "RRRRG",
                "GBYGB",
                "BYGBY"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(1));
            Assert.That(matches[0].Orientation, Is.EqualTo(MatchOrientation.Horizontal));
            Assert.That(matches[0].Size, Is.EqualTo(4));
        }

        [Test]
        public void FindMatches_ReturnsHorizontalMatchFiveAsSingleRun()
        {
            Board board = BoardFixture.Create(
                "RRRRR",
                "GBYGB",
                "BYGBY"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(1));
            Assert.That(matches[0].Orientation, Is.EqualTo(MatchOrientation.Horizontal));
            Assert.That(matches[0].Size, Is.EqualTo(5));
        }

        [Test]
        public void FindMatches_ReturnsVerticalMatchFourAsSingleRun()
        {
            Board board = BoardFixture.Create(
                "RGB",
                "RYG",
                "RBR",
                "RGY",
                "YRB"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(1));
            Assert.That(matches[0].Orientation, Is.EqualTo(MatchOrientation.Vertical));
            Assert.That(matches[0].Size, Is.EqualTo(4));
        }

        [Test]
        public void FindMatches_ReturnsVerticalMatchFiveAsSingleRun()
        {
            Board board = BoardFixture.Create(
                "RGB",
                "RYG",
                "RBR",
                "RGY",
                "RYB"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(1));
            Assert.That(matches[0].Orientation, Is.EqualTo(MatchOrientation.Vertical));
            Assert.That(matches[0].Size, Is.EqualTo(5));
        }

        [Test]
        public void FindMatches_WithIndependentRuns_ReturnsSeparateMatches()
        {
            Board board = BoardFixture.Create(
                "RRRGB",
                "GBYRG",
                "BYYYR"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(2));
            Assert.That(matches.Select(match => match.TileType), Is.EquivalentTo(new[] { 0, 3 }));
            Assert.That(matches.All(match => match.Orientation == MatchOrientation.Horizontal), Is.True);
        }

        [Test]
        public void FindMatches_WithIntersection_ReturnsSeparateLinesSharingCenterCell()
        {
            Board board = BoardFixture.Create(
                "GBYGB",
                "YBRGY",
                "GRRRG",
                "BYRBY",
                "RGBYR"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Has.Count.EqualTo(2));
            Match horizontal = matches.Single(match => match.Orientation == MatchOrientation.Horizontal);
            Match vertical = matches.Single(match => match.Orientation == MatchOrientation.Vertical);
            Assert.That(horizontal.Cells, Does.Contain(new Vector2Int(2, 2)));
            Assert.That(vertical.Cells, Does.Contain(new Vector2Int(2, 2)));
        }

        [Test]
        public void FindMatches_WithNoRuns_ReturnsEmptyCollection()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG"
            );

            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            Assert.That(matches, Is.Empty);
        }

        private static void AssertMatch(
            Match match,
            int expectedTileType,
            MatchOrientation expectedOrientation,
            params Vector2Int[] expectedCells)
        {
            Assert.That(match.TileType, Is.EqualTo(expectedTileType));
            Assert.That(match.Orientation, Is.EqualTo(expectedOrientation));
            Assert.That(match.Size, Is.EqualTo(expectedCells.Length));
            Assert.That(match.Cells, Is.EqualTo(expectedCells));
        }
    }
}
