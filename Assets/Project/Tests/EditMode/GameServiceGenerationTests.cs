using System.Collections.Generic;
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

            List<List<Tile>> board = service.StartGame(6, 6);

            Assert.That(board, Has.Count.EqualTo(6));
            foreach (List<Tile> row in board)
            {
                Assert.That(row, Has.Count.EqualTo(6));
            }
        }

        [Test]
        public void StartGame_AssignsSequentialTileIdsStartingAtZero()
        {
            var service = new GameService();

            List<List<Tile>> board = service.StartGame(4, 3);

            int expectedId = 0;
            foreach (List<Tile> row in board)
            {
                foreach (Tile tile in row)
                {
                    Assert.That(tile.Id, Is.EqualTo(expectedId));
                    expectedId++;
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
                    List<List<Tile>> board = service.StartGame(8, 8);

                    AssertBoardHasNoImmediateMatches(board, iteration);
                }
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }

        private static void AssertBoardHasNoImmediateMatches(List<List<Tile>> board, int iteration)
        {
            for (int y = 0; y < board.Count; y++)
            {
                for (int x = 0; x < board[y].Count; x++)
                {
                    if (x >= 2)
                    {
                        bool hasHorizontalMatch =
                            board[y][x].Type == board[y][x - 1].Type &&
                            board[y][x - 1].Type == board[y][x - 2].Type;

                        Assert.That(
                            hasHorizontalMatch,
                            Is.False,
                            $"Generated board {iteration} has a horizontal match ending at ({x}, {y}).");
                    }

                    if (y >= 2)
                    {
                        bool hasVerticalMatch =
                            board[y][x].Type == board[y - 1][x].Type &&
                            board[y - 1][x].Type == board[y - 2][x].Type;

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
