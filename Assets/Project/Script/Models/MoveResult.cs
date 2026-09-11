using System.Collections.Generic;

namespace Gazeus.DesafioMatch3.Models
{
    public sealed class MoveResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<BoardSequence> BoardSequences { get; }

        internal MoveResult(bool isValid, IReadOnlyList<BoardSequence> boardSequences)
        {
            IsValid = isValid;
            BoardSequences = boardSequences;
        }
    }
}
