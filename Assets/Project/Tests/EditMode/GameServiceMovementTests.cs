using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class GameServiceMovementTests
    {
        [Test]
        public void IsValidMovement_WhenAdjacentSwapCreatesNoMatch_ReturnsFalse()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "YRBG"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            bool result = service.IsValidMovement(0, 0, 1, 0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidMovement_WhenSwapCreatesHorizontalMatch_ReturnsTrue()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            bool result = service.IsValidMovement(2, 3, 3, 3);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValidMovement_WhenSwapCreatesVerticalMatch_ReturnsTrue()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "RBYG",
                "GBRY",
                "RYGB"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            bool result = service.IsValidMovement(0, 2, 0, 3);

            Assert.That(result, Is.True);
        }
    }
}
