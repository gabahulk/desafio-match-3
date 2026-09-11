using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class GameServiceGenerationTests
    {
        [Test]
        public void StartGame_ReturnsRequestedDimensions()
        {
            var service = new GameService();

            Board board = service.StartGame(6, 6);

            Assert.That(board.Width, Is.EqualTo(6));
            Assert.That(board.Height, Is.EqualTo(6));
        }

        [Test]
        public void StartGame_AssignsSequentialTileIdsStartingAtZero()
        {
            var service = new GameService();

            Board board = service.StartGame(4, 3);

            int expectedId = 0;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Assert.That(board[x, y].Id, Is.EqualTo(expectedId));
                    expectedId++;
                }
            }
        }

        [Test]
        public void StartGame_CreatesOnlyNormalTiles()
        {
            var service = new GameService();

            Board board = service.StartGame(4, 3);

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Assert.That(board[x, y].Special, Is.EqualTo(SpecialType.None));
                    Assert.That(board[x, y].IsEmpty, Is.False);
                }
            }
        }

        [Test]
        public void StartGame_Repeatedly_DoesNotCreateImmediateMatches()
        {
            Random.State originalRandomState = Random.state;
            try
            {
                Random.InitState(17321);

                for (int iteration = 0; iteration < 32; iteration++)
                {
                    var service = new GameService();
                    Board board = service.StartGame(8, 8);

                    AssertBoardHasNoImmediateMatches(board, iteration);
                }
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }

        private static void AssertBoardHasNoImmediateMatches(Board board, int iteration)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (x >= 2)
                    {
                        bool hasHorizontalMatch =
                            board[x, y].Color == board[x - 1, y].Color &&
                            board[x - 1, y].Color == board[x - 2, y].Color;

                        Assert.That(
                            hasHorizontalMatch,
                            Is.False,
                            $"Generated board {iteration} has a horizontal match ending at ({x}, {y}).");
                    }

                    if (y >= 2)
                    {
                        bool hasVerticalMatch =
                            board[x, y].Color == board[x, y - 1].Color &&
                            board[x, y - 1].Color == board[x, y - 2].Color;

                        Assert.That(
                            hasVerticalMatch,
                            Is.False,
                            $"Generated board {iteration} has a vertical match ending at ({x}, {y}).");
                    }
                }
            }
        }
    }
}
