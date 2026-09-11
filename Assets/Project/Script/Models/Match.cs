using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public sealed class Match
    {
        public int TileType { get; }
        public MatchOrientation Orientation { get; }
        public IReadOnlyList<Vector2Int> Cells { get; }
        public int Size => Cells.Count;

        internal Match(
            int tileType,
            MatchOrientation orientation,
            IReadOnlyList<Vector2Int> cells)
        {
            TileType = tileType;
            Orientation = orientation;
            Cells = cells;
        }
    }
}
