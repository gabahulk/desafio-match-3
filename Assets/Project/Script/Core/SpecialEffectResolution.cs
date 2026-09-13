using System.Collections.Generic;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal sealed class SpecialEffectResolution
    {
        internal HashSet<Vector2Int> DestructionCells { get; }
        internal IReadOnlyList<PendingSpecialActivation> PendingActivations { get; }
        internal IReadOnlyList<SpecialActivationInfo> Activations { get; }

        internal SpecialEffectResolution(
            HashSet<Vector2Int> destructionCells,
            IReadOnlyList<PendingSpecialActivation> pendingActivations,
            IReadOnlyList<SpecialActivationInfo> activations)
        {
            DestructionCells = destructionCells;
            PendingActivations = pendingActivations;
            Activations = activations;
        }
    }
}
