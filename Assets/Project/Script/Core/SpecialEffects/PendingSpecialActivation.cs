using Gazeus.DesafioMatch3.Models;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class PendingSpecialActivation
    {
        internal int TileId { get; }
        internal SpecialType Special { get; }
        internal SpecialActivationPhase Phase { get; }
        internal SpecialType? CombinedWith { get; }

        internal PendingSpecialActivation(
            int tileId,
            SpecialType special,
            SpecialActivationPhase phase,
            SpecialType? combinedWith = null)
        {
            TileId = tileId;
            Special = special;
            Phase = phase;
            CombinedWith = combinedWith;
        }
    }
}
