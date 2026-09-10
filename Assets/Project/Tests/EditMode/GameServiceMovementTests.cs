using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class GameServiceMovementTests
    {
        [Test]
        public void IsValidMovement_WhenAdjacentSwapCreatesNoMatch_ReturnsFalse()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);
            BoardFixture.Apply(
                board,
                "RGBY",
                "GBYR",
                "BYRG",
                "YRBG"
            );

            bool result = service.IsValidMovement(0, 0, 1, 0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidMovement_WhenSwapCreatesHorizontalMatch_ReturnsTrue()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);
            BoardFixture.Apply(
                board,
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );

            bool result = service.IsValidMovement(2, 3, 3, 3);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsValidMovement_WhenSwapCreatesVerticalMatch_ReturnsTrue()
        {
            var service = new GameService();
            var board = service.StartGame(4, 4);
            BoardFixture.Apply(
                board,
                "RGBY",
                "RBYG",
                "GBRY",
                "RYGB"
            );

            bool result = service.IsValidMovement(0, 2, 0, 3);

            Assert.That(result, Is.True);
        }
    }
}
