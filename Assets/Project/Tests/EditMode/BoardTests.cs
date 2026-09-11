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
            Tile expectedTile = new() { Id = 17, Color = 2 };

            board[3, 2] = expectedTile;

            Assert.That(board.Width, Is.EqualTo(4));
            Assert.That(board.Height, Is.EqualTo(3));
            Assert.That(board[0, 0].Id, Is.EqualTo(-1));
            Assert.That(board[0, 0].Color, Is.EqualTo(-1));
            Assert.That(board[0, 0].Special, Is.EqualTo(SpecialType.None));
            Assert.That(board[0, 0].IsEmpty, Is.True);
            Assert.That(board[3, 2], Is.SameAs(expectedTile));
        }

        [Test]
        public void Clone_CreatesIndependentTiles()
        {
            Board original = new(2, 1);
            original[1, 0].Id = 9;
            original[1, 0].Color = 3;
            original[1, 0].Special = SpecialType.Wrapped;

            Board clone = original.Clone();

            Assert.That(clone[1, 0].Id, Is.EqualTo(9));
            Assert.That(clone[1, 0].Color, Is.EqualTo(3));
            Assert.That(clone[1, 0].Special, Is.EqualTo(SpecialType.Wrapped));

            clone[1, 0].Id = 10;
            clone[1, 0].Color = 1;
            clone[1, 0].Special = SpecialType.HorizontalStriped;

            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(clone[1, 0], Is.Not.SameAs(original[1, 0]));
            Assert.That(original[1, 0].Id, Is.EqualTo(9));
            Assert.That(original[1, 0].Color, Is.EqualTo(3));
            Assert.That(original[1, 0].Special, Is.EqualTo(SpecialType.Wrapped));
        }
    }
}
