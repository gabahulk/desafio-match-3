using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public sealed class MatchGroup
    {
        public int TileType { get; }
        public IReadOnlyList<Match> Matches { get; }
        public IReadOnlyCollection<Vector2Int> Cells { get; }
        public MatchShape Shape { get; }
        public int Size => Cells.Count;

        internal MatchGroup(IReadOnlyList<Match> matches, MatchShape shape)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            if (matches.Count == 0)
            {
                throw new ArgumentException("A match group requires at least one match.", nameof(matches));
            }

            TileType = matches[0].TileType;
            List<Match> matchList = new(matches.Count);
            HashSet<Vector2Int> uniqueCells = new();
            List<Vector2Int> cells = new();

            for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
            {
                Match match = matches[matchIndex];
                if (match.TileType != TileType)
                {
                    throw new ArgumentException("All matches in a group must have the same tile type.", nameof(matches));
                }

                matchList.Add(match);
                for (int cellIndex = 0; cellIndex < match.Cells.Count; cellIndex++)
                {
                    Vector2Int cell = match.Cells[cellIndex];
                    if (uniqueCells.Add(cell))
                    {
                        cells.Add(cell);
                    }
                }
            }

            Matches = matchList.AsReadOnly();
            Cells = cells.AsReadOnly();
            Shape = shape;
        }
    }
}
