using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class MatchGrouperTests
    {
        [Test]
        public void Group_WithSingleMatch_ReturnsStraightGroup()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "RRRGB",
                "GBYRG",
                "BYGBY"
            );

            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0].TileType, Is.EqualTo(0));
            Assert.That(groups[0].Matches, Has.Count.EqualTo(1));
            Assert.That(groups[0].Shape, Is.EqualTo(MatchShape.Straight));
            Assert.That(groups[0].Size, Is.EqualTo(3));
        }

        [Test]
        public void Group_WithIndependentMatchesOfSameType_ReturnsSeparateGroups()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "RRRGB",
                "GBYRG",
                "BGRRR"
            );

            Assert.That(groups, Has.Count.EqualTo(2));
            Assert.That(groups.All(group => group.TileType == 0), Is.True);
            Assert.That(groups.All(group => group.Shape == MatchShape.Straight), Is.True);
        }

        [Test]
        public void Group_WithEndpointIntersectionOnBothLines_ReturnsLWithUniqueCells()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "GBRGB",
                "BYRYG",
                "YGRRR",
                "RGBYG"
            );

            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0].Shape, Is.EqualTo(MatchShape.L));
            Assert.That(groups[0].Matches, Has.Count.EqualTo(2));
            Assert.That(groups[0].Size, Is.EqualTo(5));
            Assert.That(groups[0].Cells, Is.EquivalentTo(new[]
            {
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2)
            }));
        }

        [Test]
        public void Group_WithVerticalEndpointAndHorizontalInteriorIntersection_ReturnsT()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "GBYGB",
                "GRRRG",
                "BYRBY",
                "YGRYG",
                "RGBYR"
            );

            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0].Shape, Is.EqualTo(MatchShape.T));
        }

        [Test]
        public void Group_WithHorizontalEndpointAndVerticalInteriorIntersection_ReturnsT()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "GBYGB",
                "YBRGY",
                "RRRYG",
                "BYRBY",
                "RGBYR"
            );

            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0].Shape, Is.EqualTo(MatchShape.T));
        }

        [Test]
        public void Group_WithInteriorIntersectionOnBothLines_ReturnsCrossWithUniqueCells()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "GBYGB",
                "YBRGY",
                "GRRRG",
                "BYRBY",
                "RGBYR"
            );

            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0].Shape, Is.EqualTo(MatchShape.Cross));
            Assert.That(groups[0].Matches, Has.Count.EqualTo(2));
            Assert.That(groups[0].Size, Is.EqualTo(5));
        }

        [Test]
        public void Group_WithTransitiveIntersections_ReturnsOneGroup()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "GBYGB",
                "RRRGB",
                "BYRYG",
                "GBRRR",
                "YGBYG"
            );

            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0].Matches, Has.Count.EqualTo(3));
            Assert.That(groups[0].Size, Is.EqualTo(7));
        }

        [Test]
        public void Group_WithMoreThanTwoConnectedLines_ReturnsComplex()
        {
            IReadOnlyList<MatchGroup> groups = FindGroups(
                "GBYGB",
                "RRRGB",
                "BYRYG",
                "GBRRR",
                "YGBYG"
            );

            Assert.That(groups, Has.Count.EqualTo(1));
            Assert.That(groups[0].Shape, Is.EqualTo(MatchShape.Complex));
        }

        [Test]
        public void Group_WhenMatchDiscoveryOrderIsReversed_PreservesMembershipAndShape()
        {
            Board board = BoardFixture.Create(
                "GBYGB",
                "YBRGY",
                "GRRRG",
                "BYRBY",
                "RGBYR"
            );
            IReadOnlyList<Match> matches = MatchFinder.FindMatches(board);

            IReadOnlyList<MatchGroup> originalGroups = MatchGrouper.Group(matches);
            IReadOnlyList<MatchGroup> reversedGroups = MatchGrouper.Group(matches.Reverse().ToArray());

            Assert.That(reversedGroups, Has.Count.EqualTo(1));
            Assert.That(reversedGroups[0].Shape, Is.EqualTo(originalGroups[0].Shape));
            Assert.That(reversedGroups[0].Cells, Is.EquivalentTo(originalGroups[0].Cells));
        }

        private static IReadOnlyList<MatchGroup> FindGroups(params string[] rows)
        {
            Board board = BoardFixture.Create(rows);
            return MatchGrouper.Group(MatchFinder.FindMatches(board));
        }
    }
}
