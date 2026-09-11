using System;
using Gazeus.DesafioMatch3.Models;
using NUnit.Framework;

namespace Gazeus.DesafioMatch3.Tests.EditMode.Fixtures
{
    public sealed class BoardFixtureTests
    {
        [Test]
        public void Create_MapsSymbolsAndAssignsSequentialTileIds()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "YRBG"
            );

            int[] expectedColors =
            {
                0, 1, 2, 3,
                1, 2, 3, 0,
                2, 3, 0, 1,
                3, 0, 2, 1
            };

            Assert.That(board.Width, Is.EqualTo(4));
            Assert.That(board.Height, Is.EqualTo(4));
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int expectedId = y * board.Width + x;
                    Assert.That(board[x, y].Color, Is.EqualTo(expectedColors[expectedId]));
                    Assert.That(board[x, y].Special, Is.EqualTo(SpecialType.None));
                    Assert.That(board[x, y].Id, Is.EqualTo(expectedId));
                }
            }
        }

        [Test]
        public void Create_WhenNoRowsAreProvided_Throws()
        {
            Assert.Throws<ArgumentException>(() => BoardFixture.Create());
        }

        [Test]
        public void Create_WhenRowsHaveDifferentWidths_Throws()
        {
            Assert.Throws<ArgumentException>(() => BoardFixture.Create(
                "RGBY",
                "GBY",
                "BYRG",
                "YRBG"
            ));
        }

        [Test]
        public void Create_WhenSymbolIsUnsupported_Throws()
        {
            Assert.Throws<ArgumentException>(() => BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYXG",
                "YRBG"
            ));
        }
    }
}
