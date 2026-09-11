using System.Collections.Generic;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal sealed class SpecialEffectResolution
    {
        internal HashSet<Vector2Int> DestructionCells { get; }
        internal IReadOnlyList<PendingSpecialActivation> PendingActivations { get; }

        internal SpecialEffectResolution(
            HashSet<Vector2Int> destructionCells,
            IReadOnlyList<PendingSpecialActivation> pendingActivations)
        {
            DestructionCells = destructionCells;
            PendingActivations = pendingActivations;
        }
    }
}
