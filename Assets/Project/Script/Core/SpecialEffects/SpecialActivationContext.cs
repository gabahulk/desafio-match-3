using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class SpecialActivationContext
    {
        internal Vector2Int Position { get; }
        internal SpecialActivationPhase Phase { get; }
        internal int? TargetColor { get; }

        internal SpecialActivationContext(
            Vector2Int position,
            SpecialActivationPhase phase,
            int? targetColor = null)
        {
            Position = position;
            Phase = phase;
            TargetColor = targetColor;
        }
    }
}
