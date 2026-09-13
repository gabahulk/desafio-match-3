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
        public void TrySwap_WhenSwapCreatesHorizontalMatch_FirstSequenceContainsMatchedCells()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 3, 3, 3);

            AssertFirstMatchedPositions(
                result.BoardSequences,
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3));
        }

        [Test]
        public void TrySwap_WhenRefillCreatesMatch_ReturnsAdditionalBoardSequence()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 3, 3, 3);

            Assert.That(result.BoardSequences, Has.Count.EqualTo(2));
        }

        [Test]
        public void TrySwap_WhenSwapCreatesVerticalMatch_FirstSequenceContainsMatchedCells()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "RBYG",
                "GBRY",
                "RYGB"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 2, 0, 3);

            AssertFirstMatchedPositions(
                result.BoardSequences,
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2));
        }

        [Test]
        public void TrySwap_WhenSwapCreatesMatchFour_FirstSequenceContainsAllFourCells()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 3, 2, 2);

            AssertFirstMatchedPositions(
                result.BoardSequences,
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(3, 3));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles, Has.Count.EqualTo(1));
        }

        [Test]
        public void TrySwap_WhenSwapCreatesMatchFive_FirstSequenceContainsAllFiveCells()
        {
            Board board = BoardFixture.Create(
                "RGBYR",
                "GBYRG",
                "BYRGB",
                "YBRGY",
                "RRGRR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 4, 2, 3);

            AssertFirstMatchedPositions(
                result.BoardSequences,
                new Vector2Int(0, 4),
                new Vector2Int(1, 4),
                new Vector2Int(3, 4),
                new Vector2Int(4, 4));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles, Has.Count.EqualTo(1));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles[0].Position,
                Is.EqualTo(new Vector2Int(2, 4)));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles[0].Special,
                Is.EqualTo(SpecialType.ColorBomb));
        }

        [Test]
        public void TrySwap_WhenSwapCreatesIntersectingMatches_CreatesWrappedAndDestroysRemainingUnion()
        {
            Board board = BoardFixture.Create(
                "RGBYR",
                "GYRBG",
                "RRGRB",
                "BYRGR",
                "YGBYR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 2, 3, 2);

            AssertFirstMatchedPositions(
                result.BoardSequences,
                new Vector2Int(2, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 3));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles, Has.Count.EqualTo(1));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles[0].Position,
                Is.EqualTo(new Vector2Int(2, 2)));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles[0].Special,
                Is.EqualTo(SpecialType.Wrapped));
        }

        [Test]
        public void TrySwap_OnWideRectangularBoard_ResolvesHorizontalMatch()
        {
            Board board = BoardFixture.Create(
                "RGBYG",
                "GBYGR",
                "BYRRG"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 4, 1, 4, 2);

            AssertFirstMatchedPositions(
                result.BoardSequences,
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2));
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
                MoveResult result = service.TrySwap(fromX, fromY, toX, toY);
                Assert.That(result.IsValid, Is.True);
                return result;
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }

        private static void AssertFirstMatchedPositions(
            IReadOnlyList<BoardSequence> sequences,
            params Vector2Int[] expectedPositions)
        {
            Assert.That(sequences, Is.Not.Empty);
            Assert.That(sequences[0].MatchedPosition, Has.Count.EqualTo(expectedPositions.Length));
            Assert.That(sequences[0].MatchedPosition, Is.EquivalentTo(expectedPositions));
        }
    }
}
