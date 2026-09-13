using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class SpecialActivationResult
    {
        internal IReadOnlyList<Vector2Int> AffectedCells { get; }
        internal IReadOnlyList<Vector2Int> PreservedCells { get; }
        internal IReadOnlyList<PendingSpecialActivation> PendingActivations { get; }
        internal IReadOnlyList<SpecialActivationContext> TriggeredActivations { get; }
        internal IReadOnlyList<SpecialTransformation> Transformations { get; }

        internal SpecialActivationResult(
            IReadOnlyList<Vector2Int> affectedCells,
            bool preserveSource,
            PendingSpecialActivation pendingActivation)
        {
            AffectedCells = affectedCells;
            PreservedCells = preserveSource ? new[] { Vector2Int.zero } : System.Array.Empty<Vector2Int>();
            PendingActivations = pendingActivation == null ? System.Array.Empty<PendingSpecialActivation>() : new[] { pendingActivation };
            TriggeredActivations = System.Array.Empty<SpecialActivationContext>();
            Transformations = System.Array.Empty<SpecialTransformation>();
        }

        internal SpecialActivationResult(IReadOnlyList<Vector2Int> affectedCells,
            IReadOnlyList<Vector2Int> preservedCells,
            IReadOnlyList<PendingSpecialActivation> pendingActivations,
            IReadOnlyList<SpecialActivationContext> triggeredActivations,
            IReadOnlyList<SpecialTransformation> transformations)
        {
            AffectedCells = affectedCells;
            PreservedCells = preservedCells;
            PendingActivations = pendingActivations;
            TriggeredActivations = triggeredActivations;
            Transformations = transformations;
        }
    }
}
