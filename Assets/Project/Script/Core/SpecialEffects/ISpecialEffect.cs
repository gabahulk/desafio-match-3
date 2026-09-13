using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal interface ISpecialEffect
    {
        bool CanHandle(SpecialType special);

        SpecialActivationResult Activate(
            Board board,
            SpecialActivationContext context);
    }
}
