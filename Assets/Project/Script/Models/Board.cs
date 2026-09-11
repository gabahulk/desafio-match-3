using System;

namespace Gazeus.DesafioMatch3.Models
{
    public sealed class Board
    {
        private readonly Tile[,] _tiles;

        public int Width { get; }
        public int Height { get; }

        public Tile this[int x, int y]
        {
            get => _tiles[x, y];
            set => _tiles[x, y] = value;
        }

        public Board(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Width = width;
            Height = height;
            _tiles = new Tile[width, height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _tiles[x, y] = new Tile
                    {
                        Id = -1,
                        Color = -1,
                        Special = SpecialType.None
                    };
                }
            }
        }

        public Board Clone()
        {
            Board clone = new(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile tile = _tiles[x, y];
                    clone[x, y] = new Tile
                    {
                        Id = tile.Id,
                        Color = tile.Color,
                        Special = tile.Special
                    };
                }
            }

            return clone;
        }
    }
}
