using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal interface ISpecialEffect
    {
        bool CanHandle(SpecialType special);

        void Expand(
            Board board,
            Vector2Int position,
            List<Vector2Int> affectedCells);
    }
}
