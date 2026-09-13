using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal interface ISpecialEffect
    {
        bool CanHandle(SpecialActivationContext context);

        SpecialActivationResult Activate(
            Board board,
            SpecialActivationContext context);
    }
}
