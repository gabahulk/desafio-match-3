using System;
using System.Linq;
using Gazeus.DesafioMatch3.Core;
using NUnit.Framework;

namespace Gazeus.DesafioMatch3.Tests.EditMode.Fixtures
{
    public sealed class BoardFixtureTests
    {
        [Test]
        public void Apply_MapsSymbolsAndPreservesTileIds()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);
            int[] originalIds = board.SelectMany(row => row).Select(tile => tile.Id).ToArray();

            BoardFixture.Apply(
                board,
                "RGBY",
                "GBYR",
                "BYRG",
                "YRBG"
            );

            int[] expectedTypes =
            {
                0, 1, 2, 3,
                1, 2, 3, 0,
                2, 3, 0, 1,
                3, 0, 2, 1
            };

            Assert.That(
                board.SelectMany(row => row).Select(tile => tile.Type),
                Is.EqualTo(expectedTypes));
            Assert.That(
                board.SelectMany(row => row).Select(tile => tile.Id),
                Is.EqualTo(originalIds));
        }

        [Test]
        public void Apply_WhenRowCountDoesNotMatchBoardHeight_Throws()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);

            Assert.Throws<ArgumentException>(() => BoardFixture.Apply(
                board,
                "RGBY",
                "GBYR",
                "BYRG"
            ));
        }

        [Test]
        public void Apply_WhenRowWidthDoesNotMatchBoardWidth_Throws()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);

            Assert.Throws<ArgumentException>(() => BoardFixture.Apply(
                board,
                "RGBY",
                "GBY",
                "BYRG",
                "YRBG"
            ));
        }

        [Test]
        public void Apply_WhenSymbolIsUnsupported_Throws()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);

            Assert.Throws<ArgumentException>(() => BoardFixture.Apply(
                board,
                "RGBY",
                "GBYR",
                "BYXG",
                "YRBG"
            ));
        }
    }
}
