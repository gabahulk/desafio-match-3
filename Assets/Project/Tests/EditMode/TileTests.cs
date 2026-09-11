using Gazeus.DesafioMatch3.Models;
using NUnit.Framework;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class TileTests
    {
        [Test]
        public void NormalColoredTile_IsNotEmpty()
        {
            Tile tile = new()
            {
                Id = 1,
                Color = 0
            };

            Assert.That(tile.Special, Is.EqualTo(SpecialType.None));
            Assert.That(tile.IsEmpty, Is.False);
        }

        [Test]
        public void ColoredStripedTile_IsNotEmpty()
        {
            Tile tile = new()
            {
                Id = 2,
                Color = 0,
                Special = SpecialType.HorizontalStriped
            };

            Assert.That(tile.IsEmpty, Is.False);
        }

        [Test]
        public void ColorlessColorBomb_IsNotEmpty()
        {
            Tile tile = new()
            {
                Id = 3,
                Color = -1,
                Special = SpecialType.ColorBomb
            };

            Assert.That(tile.IsEmpty, Is.False);
        }

        [Test]
        public void EmptyTile_IsEmpty()
        {
            Tile tile = new()
            {
                Id = -1,
                Color = -1,
                Special = SpecialType.None
            };

            Assert.That(tile.IsEmpty, Is.True);
        }
    }
}
