using System.Collections.Generic;
using System.Reflection;
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
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

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
            Board board = BoardFixture.Create(
                "RGBY",
                "RBYG",
                "GBRY",
                "RYGB"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

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
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

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
            Board board = BoardFixture.Create(
                "RGBYR",
                "GBYRG",
                "BYRGB",
                "YBRGY",
                "RRGRR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

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
            Board board = BoardFixture.Create(
                "RGBYR",
                "GYRBG",
                "RRGRB",
                "BYRGR",
                "YGBYR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

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
            Board board = BoardFixture.Create(
                "RGBYG",
                "GBYGR",
                "BYRRG"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            List<BoardSequence> sequences = SwapDeterministically(service, 4, 1, 4, 2);

            AssertFirstMatchedPositions(
                sequences,
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2));
        }

        [Test]
        public void SwapTile_WithInjectedNonContiguousIds_RefillIdsStartAfterHighestExistingId()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            board[3, 3].Id = 99;
            var service = new GameService(board, new[] { 4, 5, 6, 7, 8, 9, 10, 11 });

            List<BoardSequence> sequences = SwapDeterministically(service, 2, 3, 3, 3);

            Board resolvedBoard = GetServiceBoard(service);
            Assert.That(sequences, Has.Count.EqualTo(1));
            Assert.That(resolvedBoard[0, 0].Id, Is.EqualTo(102));
            Assert.That(resolvedBoard[1, 0].Id, Is.EqualTo(101));
            Assert.That(resolvedBoard[2, 0].Id, Is.EqualTo(100));
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

        private static Board GetServiceBoard(GameService service)
        {
            FieldInfo boardField = typeof(GameService).GetField(
                "_board",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(boardField, Is.Not.Null);
            return (Board)boardField.GetValue(service);
        }
    }
}
