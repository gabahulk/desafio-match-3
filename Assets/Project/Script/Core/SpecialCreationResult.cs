using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal sealed class SpecialCreationResult
    {
        internal IReadOnlyList<SpecialTileInfo> CreatedSpecialTiles { get; }
        internal IReadOnlyCollection<Vector2Int> ProtectedCells { get; }

        internal SpecialCreationResult(
            IReadOnlyList<SpecialTileInfo> createdSpecialTiles,
            IReadOnlyCollection<Vector2Int> protectedCells)
        {
            CreatedSpecialTiles = createdSpecialTiles;
            ProtectedCells = protectedCells;
        }
    }
}
