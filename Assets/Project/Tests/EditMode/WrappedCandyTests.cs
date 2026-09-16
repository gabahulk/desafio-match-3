using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class WrappedCandyTests
    {
        [Test]
        public void TrySwap_PlayerCreatedL_CreatesProtectedWrappedAtTo()
        {
            Board board = BoardFixture.Create(
                "GBRGB",
                "BYRYG",
                "YGRRR",
                "RGBYG");
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 1, 2, 2);

            AssertWrappedCreation(result.BoardSequences[0], new Vector2Int(2, 2), 0, 4);
        }

        [Test]
        public void TrySwap_PlayerCreatedT_CreatesProtectedWrappedAtTo()
        {
            Board board = BoardFixture.Create(
                "GBYGB",
                "GRRRG",
                "BYRBY",
                "YGRYG",
                "RGBYR");
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 2, 2, 1);

            AssertWrappedCreation(result.BoardSequences[0], new Vector2Int(2, 1), 0, 4);
        }

        [Test]
        public void TrySwap_PlayerCreatedCross_CreatesOneProtectedWrapped()
        {
            Board board = BoardFixture.Create(
                "GBYGB",
                "YBRGY",
                "GRRRG",
                "BYRBY",
                "RGBYR");
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 1, 2, 2);

            AssertWrappedCreation(result.BoardSequences[0], new Vector2Int(2, 2), 0, 4);
        }

        [Test]
        public void TrySwap_CascadeCreatedL_UsesMovedDestinationAndCreatesWrapped()
        {
            Board board = BoardFixture.Create(
                "RGBYG",
                "GBYRB",
                "BYRBG",
                "BBBRR",
                "GYRBY",
                "YGRGB");
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 3, 1, 3);

            Assert.That(result.BoardSequences, Has.Count.GreaterThanOrEqualTo(2));
            BoardSequence cascade = result.BoardSequences[1];
            Assert.That(cascade.CreatedSpecialTiles, Has.Count.EqualTo(1));
            Assert.That(cascade.CreatedSpecialTiles[0].Position,
                Is.EqualTo(new Vector2Int(2, 3)));
            Assert.That(cascade.CreatedSpecialTiles[0].Color, Is.EqualTo(0));
            Assert.That(cascade.CreatedSpecialTiles[0].Special,
                Is.EqualTo(SpecialType.Wrapped));
            Assert.That(cascade.MatchedPositions, Has.None.EqualTo(new Vector2Int(2, 3)));
            Assert.That(result.BoardSequences[0].MovedTiles.Any(move =>
                move.To == new Vector2Int(2, 3)), Is.True);
        }

        [Test]
        public void TrySwap_WhenPreferredWrappedSpawnIsSpecial_FallsBackToNormalFromCell()
        {
            Board board = BoardFixture.Create(
                "GBRGB",
                "BYRYG",
                "YGRRR",
                "RGBYG");
            board[2, 1].Special = SpecialType.HorizontalStriped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 1, 2, 2);

            Assert.That(result.BoardSequences[0].CreatedSpecialTiles, Has.Count.EqualTo(1));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles[0].Position,
                Is.EqualTo(new Vector2Int(2, 1)));
            Assert.That(result.BoardSequences[0].CreatedSpecialTiles[0].Special,
                Is.EqualTo(SpecialType.Wrapped));
        }

        [Test]
        public void TrySwap_MatchedWrapped_ExplodesTwiceWithGravityBetweenBlasts()
        {
            Board board = CreateCenterWrappedMatchBoard();
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 1, 2, 2, 2);

            Assert.That(result.BoardSequences, Has.Count.GreaterThanOrEqualTo(2));
            BoardSequence first = result.BoardSequences[0];
            Assert.That(first.MatchedPositions, Is.EqualTo(Area(2, 2, 5, 5, new Vector2Int(2, 2))));
            Assert.That(first.MovedTiles.Any(move =>
                move.From == new Vector2Int(2, 2) && move.To == new Vector2Int(2, 3)), Is.True);

            BoardSequence second = result.BoardSequences[1];
            Assert.That(second.MatchedPositions, Is.EqualTo(Area(2, 3, 5, 5)));
            Assert.That(second.MatchedPositions, Does.Contain(new Vector2Int(2, 3)));
            Assert.That(FindWrappedMove(first), Is.EqualTo(new Vector2Int(2, 3)));
        }

        [Test]
        public void TrySwap_WrappedAtCorner_FirstBlastIsBoundedAndPreservesSource()
        {
            Board board = BoardFixture.Create(
                "RRRGB",
                "GBYRG",
                "BYGBY");
            board[1, 0].Special = SpecialType.Wrapped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 1, 0, 0, 0);

            Assert.That(result.BoardSequences[0].MatchedPositions, Is.EqualTo(new[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            }));
            Assert.That(result.BoardSequences[1].MatchedPositions,
                Has.All.Matches<Vector2Int>(position => position.x >= 0 && position.y >= 0));
        }

        [Test]
        public void TrySwap_WrappedAtEdge_FirstBlastIsBoundedAndPreservesSource()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "RBYG",
                "RYGB",
                "GBYR");
            board[0, 0].Special = SpecialType.Wrapped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 0, 1);

            Assert.That(result.BoardSequences[0].MatchedPositions, Is.EqualTo(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2)
            }));
            Assert.That(result.BoardSequences[0].MatchedPositions,
                Has.None.EqualTo(new Vector2Int(0, 1)));
        }

        [Test]
        public void TrySwap_WrappedBlastHitsStriped_ExpandsStripedInSameStep()
        {
            Board board = CreateCenterWrappedMatchBoard();
            board[1, 1].Special = SpecialType.HorizontalStriped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 1, 2, 2, 2);

            Assert.That(result.BoardSequences[0].MatchedPositions,
                Does.Contain(new Vector2Int(0, 1)));
            Assert.That(result.BoardSequences[0].MatchedPositions,
                Does.Contain(new Vector2Int(4, 1)));
            Assert.That(result.BoardSequences[0].MatchedPositions,
                Has.None.EqualTo(new Vector2Int(2, 2)));
        }

        [Test]
        public void TrySwap_StripedBlastHitsWrapped_SchedulesSecondBlast()
        {
            Board board = BoardFixture.Create(
                "GBYRR",
                "BYRBB",
                "RRRGB",
                "YGBYR",
                "GBYRG");
            board[0, 2].Special = SpecialType.HorizontalStriped;
            board[4, 2].Special = SpecialType.Wrapped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 2, 1, 2);

            Assert.That(result.BoardSequences, Has.Count.GreaterThanOrEqualTo(2));
            Assert.That(result.BoardSequences[0].MatchedPositions,
                Has.None.EqualTo(new Vector2Int(4, 2)));
            Assert.That(result.BoardSequences[1].MatchedPositions, Is.Not.Empty);
        }

        [Test]
        public void TrySwap_WrappedBlastHitsWrapped_BothScheduleExactlyOneSecondBlast()
        {
            Board board = CreateCenterWrappedMatchBoard();
            board[1, 1].Special = SpecialType.Wrapped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 1, 2, 2, 2);

            Assert.That(result.BoardSequences, Has.Count.GreaterThanOrEqualTo(2));
            Assert.That(result.BoardSequences[0].MatchedPositions,
                Has.None.EqualTo(new Vector2Int(1, 1)));
            Assert.That(result.BoardSequences[0].MatchedPositions,
                Has.None.EqualTo(new Vector2Int(2, 2)));
            Assert.That(result.BoardSequences[1].MatchedPositions, Is.Not.Empty);
        }

        private static Board CreateCenterWrappedMatchBoard()
        {
            Board board = BoardFixture.Create(
                "GBYGB",
                "YBRGY",
                "GRRRG",
                "BYGBY",
                "RGBYR");
            board[1, 2].Special = SpecialType.Wrapped;
            return board;
        }

        private static List<Vector2Int> Area(
            int centerX,
            int centerY,
            int width,
            int height,
            Vector2Int? excluded = null)
        {
            List<Vector2Int> cells = new();
            for (int y = Mathf.Max(0, centerY - 1); y <= Mathf.Min(height - 1, centerY + 1); y++)
            {
                for (int x = Mathf.Max(0, centerX - 1); x <= Mathf.Min(width - 1, centerX + 1); x++)
                {
                    Vector2Int position = new(x, y);
                    if (!excluded.HasValue || position != excluded.Value)
                    {
                        cells.Add(position);
                    }
                }
            }

            return cells;
        }

        private static Vector2Int FindWrappedMove(BoardSequence sequence)
        {
            // Movement entries intentionally carry positions rather than tile IDs. The
            // known source position identifies this fixture's Wrapped tile unambiguously.
            MovedTileInfo move = sequence.MovedTiles.Single(candidate =>
                candidate.From == new Vector2Int(2, 2));
            return move.To;
        }

        private static void AssertWrappedCreation(
            BoardSequence sequence,
            Vector2Int expectedPosition,
            int expectedColor,
            int expectedDestroyedCount)
        {
            Assert.That(sequence.CreatedSpecialTiles, Has.Count.EqualTo(1));
            SpecialTileInfo created = sequence.CreatedSpecialTiles[0];
            Assert.That(created.Position, Is.EqualTo(expectedPosition));
            Assert.That(created.Color, Is.EqualTo(expectedColor));
            Assert.That(created.Special, Is.EqualTo(SpecialType.Wrapped));
            Assert.That(sequence.MatchedPositions, Has.Count.EqualTo(expectedDestroyedCount));
            Assert.That(sequence.MatchedPositions, Has.None.EqualTo(expectedPosition));
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
    }
}
