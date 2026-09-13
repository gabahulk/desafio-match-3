using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class SpecialTransformation
    {
        internal Vector2Int Position { get; }
        internal SpecialType Special { get; }
        internal SpecialTransformation(Vector2Int position, SpecialType special)
        {
            Position = position;
            Special = special;
        }
    }
}
