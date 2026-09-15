using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class SpecialActivationContext
    {
        internal Vector2Int Position { get; }
        internal SpecialType Special { get; }
        internal SpecialType? CombinedWith { get; }
        internal Vector2Int? PartnerPosition { get; }
        internal SpecialActivationPhase Phase { get; }
        internal int? TargetColor { get; set; }

        internal SpecialActivationContext(
            Vector2Int position,
            SpecialActivationPhase phase,
            int? targetColor = null,
            SpecialType? special = null,
            SpecialType? combinedWith = null,
            Vector2Int? partnerPosition = null)
        {
            Position = position;
            Phase = phase;
            TargetColor = targetColor;
            Special = special ?? SpecialType.None;
            CombinedWith = combinedWith;
            PartnerPosition = partnerPosition;
        }
    }
}
