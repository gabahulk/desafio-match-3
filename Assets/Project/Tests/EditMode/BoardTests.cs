using Gazeus.DesafioMatch3.Models;
using NUnit.Framework;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class BoardTests
    {
        [Test]
        public void Constructor_CreatesRectangularEmptyGridWithCoordinateAccess()
        {
            Board board = new(4, 3);
            Tile expectedTile = new() { Id = 17, Type = 2 };

            board[3, 2] = expectedTile;

            Assert.That(board.Width, Is.EqualTo(4));
            Assert.That(board.Height, Is.EqualTo(3));
            Assert.That(board[0, 0].Id, Is.EqualTo(-1));
            Assert.That(board[0, 0].Type, Is.EqualTo(-1));
            Assert.That(board[3, 2], Is.SameAs(expectedTile));
        }

        [Test]
        public void Clone_CreatesIndependentTiles()
        {
            Board original = new(2, 1);
            original[1, 0].Id = 9;
            original[1, 0].Type = 3;

            Board clone = original.Clone();
            clone[1, 0].Id = 10;
            clone[1, 0].Type = 1;

            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(clone[1, 0], Is.Not.SameAs(original[1, 0]));
            Assert.That(original[1, 0].Id, Is.EqualTo(9));
            Assert.That(original[1, 0].Type, Is.EqualTo(3));
        }
    }
}
