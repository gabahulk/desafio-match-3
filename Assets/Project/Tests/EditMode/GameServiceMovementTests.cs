using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class GameServiceMovementTests
    {
        [Test]
        public void TrySwap_WhenAdjacentSwapCreatesNoMatch_ReturnsInvalidResultWithoutChangingBoard()
        {
            Board board = BoardFixture.Create(
                "RGRY",
                "GRBY",
                "BYGR",
                "YBGR"
            );
            Board originalBoard = board.Clone();
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = service.TrySwap(0, 0, 1, 0);
            MoveResult subsequentResult = TrySwapDeterministically(service, 1, 0, 1, 1);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.BoardSequences, Is.Empty);
            Assert.That(subsequentResult.IsValid, Is.True);
            AssertBoardsHaveEqualTiles(board, originalBoard);
        }

        [Test]
        public void TrySwap_WhenSwapCreatesHorizontalMatch_ReturnsValidResult()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 3, 3, 3);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.BoardSequences, Is.Not.Empty);
        }

        [Test]
        public void TrySwap_WhenSwapCreatesVerticalMatch_ReturnsValidResult()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "RBYG",
                "GBRY",
                "RYGB"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 2, 0, 3);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.BoardSequences, Is.Not.Empty);
        }

        [TestCase(0, 0, 0, 0)]
        [TestCase(0, 0, 2, 0)]
        [TestCase(0, 0, 1, 1)]
        public void TrySwap_WhenPositionsAreNotAdjacent_ReturnsInvalidWithoutChangingState(
            int fromX,
            int fromY,
            int toX,
            int toY)
        {
            var service = new GameService(BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"), new[] { 0, 1, 2, 3 });
            TrySwapDeterministically(service, 2, 3, 3, 3);
            Board boardBeforeInvalidSwap = service.Board.Clone();
            int scoreBeforeInvalidSwap = service.Score;

            MoveResult result = service.TrySwap(fromX, fromY, toX, toY);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.BoardSequences, Is.Empty);
            AssertBoardsHaveEqualTiles(service.Board, boardBeforeInvalidSwap);
            Assert.That(service.Score, Is.EqualTo(scoreBeforeInvalidSwap));
        }

        [TestCase(-1, 0, 0, 0)]
        [TestCase(0, -1, 0, 0)]
        [TestCase(3, 0, 4, 0)]
        [TestCase(0, 3, 0, 4)]
        public void TrySwap_WhenPositionIsOutsideBoard_ReturnsInvalidWithoutChangingState(
            int fromX,
            int fromY,
            int toX,
            int toY)
        {
            var service = new GameService(BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"), new[] { 0, 1, 2, 3 });
            Board boardBeforeSwap = service.Board.Clone();

            MoveResult result = service.TrySwap(fromX, fromY, toX, toY);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.BoardSequences, Is.Empty);
            AssertBoardsHaveEqualTiles(service.Board, boardBeforeSwap);
            Assert.That(service.Score, Is.Zero);
        }

        private static MoveResult TrySwapDeterministically(
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
                return service.TrySwap(fromX, fromY, toX, toY);
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }

        private static void AssertBoardsHaveEqualTiles(Board actual, Board expected)
        {
            Assert.That(actual.Width, Is.EqualTo(expected.Width));
            Assert.That(actual.Height, Is.EqualTo(expected.Height));

            for (int y = 0; y < actual.Height; y++)
            {
                for (int x = 0; x < actual.Width; x++)
                {
                    Assert.That(actual[x, y].Id, Is.EqualTo(expected[x, y].Id));
                    Assert.That(actual[x, y].Color, Is.EqualTo(expected[x, y].Color));
                    Assert.That(actual[x, y].Special, Is.EqualTo(expected[x, y].Special));
                }
            }
        }
    }
}
