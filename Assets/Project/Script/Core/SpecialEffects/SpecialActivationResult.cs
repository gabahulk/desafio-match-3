using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class SpecialActivationResult
    {
        internal IReadOnlyList<Vector2Int> AffectedCells { get; }
        internal bool PreserveSource { get; }
        internal PendingSpecialActivation PendingActivation { get; }

        internal SpecialActivationResult(
            IReadOnlyList<Vector2Int> affectedCells,
            bool preserveSource,
            PendingSpecialActivation pendingActivation)
        {
            AffectedCells = affectedCells;
            PreserveSource = preserveSource;
            PendingActivation = pendingActivation;
        }
    }
}
