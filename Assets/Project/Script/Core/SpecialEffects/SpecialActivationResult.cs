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
            IReadOnlyList<Vector2Int> affectedCells)
            : this(
                affectedCells,
                System.Array.Empty<Vector2Int>(),
                System.Array.Empty<PendingSpecialActivation>(),
                System.Array.Empty<SpecialActivationContext>(),
                System.Array.Empty<SpecialTransformation>())
        {
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
