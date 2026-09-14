using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class GameServiceInitializationTests
    {
        [Test]
        public void StartGame_WithExplicitBoard_PreservesBoardLayout()
        {
            Board board = new(4, 1);
            board[0, 0] = CreateTile(4, 0, SpecialType.HorizontalStriped);
            board[1, 0] = CreateTile(8, 1, SpecialType.VerticalStriped);
            board[2, 0] = CreateTile(12, 2, SpecialType.Wrapped);
            board[3, 0] = CreateTile(3, -1, SpecialType.ColorBomb);
            var service = new GameService();

            Board initializedBoard = service.StartGame(board, new[] { 0, 1, 2, 3 });

            Assert.That(initializedBoard, Is.SameAs(board));
            AssertTile(board[0, 0], 4, 0, SpecialType.HorizontalStriped);
            AssertTile(board[1, 0], 8, 1, SpecialType.VerticalStriped);
            AssertTile(board[2, 0], 12, 2, SpecialType.Wrapped);
            AssertTile(board[3, 0], 3, -1, SpecialType.ColorBomb);
        }

        [Test]
        public void StartGame_WithExplicitBoard_DoesNotConsumeRandomState()
        {
            Random.State originalRandomState = Random.state;
            try
            {
                Board board = BoardFixture.Create(
                    "RGB",
                    "GBY",
                    "BYR");
                var service = new GameService();

                Random.InitState(7321);
                float expectedNextValue = Random.value;
                Random.InitState(7321);

                service.StartGame(board, new[] { 0, 1, 2, 3 });

                Assert.That(Random.value, Is.EqualTo(expectedNextValue));
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }

        [Test]
        public void StartGame_WithSparseTileIds_RefillUsesIdsAboveExistingMaximum()
        {
            Board board = BoardFixture.Create(
                "RGR",
                "BRY",
                "GRB");
            int tileId = 4;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board[x, y].Id = tileId++;
                }
            }

            var service = new GameService();
            service.StartGame(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 1, 0, 1, 1);

            Assert.That(result.BoardSequences[0].AddedTiles, Has.Count.EqualTo(3));
            Assert.That(
                result.BoardSequences[0].AddedTiles.Select(tile => tile.Id),
                Is.EquivalentTo(new[] { 13, 14, 15 }));
            Assert.That(
                result.BoardSequences[0].AddedTiles.All(tile =>
                    tile.Color >= 0 && tile.Color <= 3),
                Is.True);
        }

        private static Tile CreateTile(int id, int color, SpecialType special)
        {
            return new Tile
            {
                Id = id,
                Color = color,
                Special = special
            };
        }

        private static void AssertTile(Tile tile, int id, int color, SpecialType special)
        {
            Assert.That(tile.Id, Is.EqualTo(id));
            Assert.That(tile.Color, Is.EqualTo(color));
            Assert.That(tile.Special, Is.EqualTo(special));
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
