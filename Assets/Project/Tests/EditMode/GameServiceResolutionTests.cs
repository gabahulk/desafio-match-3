using System.Collections.Generic;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class GameServiceResolutionTests
    {
        [Test]
        public void SwapTile_WhenSwapCreatesHorizontalMatch_FirstSequenceContainsMatchedCells()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);
            BoardFixture.Apply(
                board,
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );

            List<BoardSequence> sequences = SwapDeterministically(service, 2, 3, 3, 3);

            AssertFirstMatchedPositions(
                sequences,
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3));
        }

        [Test]
        public void SwapTile_WhenSwapCreatesVerticalMatch_FirstSequenceContainsMatchedCells()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);
            BoardFixture.Apply(
                board,
                "RGBY",
                "RBYG",
                "GBRY",
                "RYGB"
            );

            List<BoardSequence> sequences = SwapDeterministically(service, 0, 2, 0, 3);

            AssertFirstMatchedPositions(
                sequences,
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2));
        }

        [Test]
        public void SwapTile_WhenSwapCreatesMatchFour_FirstSequenceContainsAllFourCells()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);
            BoardFixture.Apply(
                board,
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );

            List<BoardSequence> sequences = SwapDeterministically(service, 2, 3, 2, 2);

            AssertFirstMatchedPositions(
                sequences,
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3),
                new Vector2Int(3, 3));
        }

        [Test]
        public void SwapTile_WhenSwapCreatesMatchFive_FirstSequenceContainsAllFiveCells()
        {
            var service = new GameService();
            var board = service.StartGame(5, 5);
            BoardFixture.Apply(
                board,
                "RGBYR",
                "GBYRG",
                "BYRGB",
                "YBRGY",
                "RRGRR"
            );

            List<BoardSequence> sequences = SwapDeterministically(service, 2, 4, 2, 3);

            AssertFirstMatchedPositions(
                sequences,
                new Vector2Int(0, 4),
                new Vector2Int(1, 4),
                new Vector2Int(2, 4),
                new Vector2Int(3, 4),
                new Vector2Int(4, 4));
        }

        [Test]
        public void SwapTile_WhenSwapCreatesIntersectingMatches_FirstSequenceContainsUnionWithoutDuplicates()
        {
            var service = new GameService();
            var board = service.StartGame(5, 5);
            BoardFixture.Apply(
                board,
                "RGBYR",
                "GYRBG",
                "RRGRB",
                "BYRGR",
                "YGBYR"
            );

            List<BoardSequence> sequences = SwapDeterministically(service, 2, 2, 3, 2);

            AssertFirstMatchedPositions(
                sequences,
                new Vector2Int(2, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(2, 3));
        }

        [Test]
        public void SwapTile_OnWideRectangularBoard_ResolvesHorizontalMatch()
        {
            var service = new GameService();
            var board = service.StartGame(5, 3);
            BoardFixture.Apply(
                board,
                "RGBYG",
                "GBYGR",
                "BYRRG"
            );

            List<BoardSequence> sequences = SwapDeterministically(service, 4, 1, 4, 2);

            AssertFirstMatchedPositions(
                sequences,
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2));
        }

        private static List<BoardSequence> SwapDeterministically(
            GameService service,
            int fromX,
            int fromY,
            int toX,
            int toY)
        {
            Random.State originalRandomState = Random.state;
            try
            {
                Random.InitState(48271);
                return service.SwapTile(fromX, fromY, toX, toY);
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }

        private static void AssertFirstMatchedPositions(
            List<BoardSequence> sequences,
            params Vector2Int[] expectedPositions)
        {
            Assert.That(sequences, Is.Not.Empty);
            Assert.That(sequences[0].MatchedPosition, Has.Count.EqualTo(expectedPositions.Length));
            Assert.That(sequences[0].MatchedPosition, Is.EquivalentTo(expectedPositions));
        }
    }
}
