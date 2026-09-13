using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public sealed class SpecialMatch
    {
        public Vector2Int FirstPosition { get; }
        public Vector2Int SecondPosition { get; }

        internal SpecialMatch(Vector2Int firstPosition, Vector2Int secondPosition)
        {
            FirstPosition = firstPosition;
            SecondPosition = secondPosition;
        }
    }
}
