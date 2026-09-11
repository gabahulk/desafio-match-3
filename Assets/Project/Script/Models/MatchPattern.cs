using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public sealed class MatchPattern
    {
        public int Color { get; }
        public MatchShape Shape { get; }
        public IReadOnlyList<Vector2Int> Cells { get; }
        public int Size => Cells.Count;
        public bool CreatesSpecial
        {
            get
            {
                if (Shape == MatchShape.Straight)
                {
                    return Size >= 4;
                }

                return Shape == MatchShape.L ||
                       Shape == MatchShape.T ||
                       Shape == MatchShape.Cross;
            }
        }

        internal MatchPattern(
            int color,
            MatchShape shape,
            IReadOnlyList<Vector2Int> cells)
        {
            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            Color = color;
            Shape = shape;

            HashSet<Vector2Int> uniqueCells = new();
            List<Vector2Int> cellList = new(cells.Count);
            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                Vector2Int cell = cells[cellIndex];
                if (uniqueCells.Add(cell))
                {
                    cellList.Add(cell);
                }
            }

            Cells = cellList.AsReadOnly();
        }
    }
}
