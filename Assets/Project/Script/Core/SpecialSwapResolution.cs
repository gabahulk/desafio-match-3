using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal sealed class SpecialSwapResolution
    {
        internal HashSet<Vector2Int> InitialCells { get; }
        internal SpecialActivationInfo Activation { get; }

        internal SpecialSwapResolution(HashSet<Vector2Int> initialCells, SpecialActivationInfo activation)
        {
            InitialCells = initialCells;
            Activation = activation;
        }
    }
}
