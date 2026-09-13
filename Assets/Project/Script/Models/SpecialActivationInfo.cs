using Gazeus.DesafioMatch3.Core.SpecialEffects;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public sealed class SpecialActivationInfo
    {
        public SpecialType Special { get; set; }
        public SpecialType? CombinedWith { get; set; }
        public Vector2Int? PartnerPosition { get; set; }
        public Vector2Int Position { get; set; }
        public SpecialActivationPhase Phase { get; set; }
        public int? TargetColor { get; set; }
    }
}
