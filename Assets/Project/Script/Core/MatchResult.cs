using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;

namespace Gazeus.DesafioMatch3.Core
{
    public sealed class MatchResult
    {
        public IReadOnlyList<MatchPattern> StandardPatterns { get; }
        public SpecialMatch SpecialMatch { get; }
        public bool IsValid => StandardPatterns.Count > 0 || SpecialMatch != null;

        internal MatchResult(IReadOnlyList<MatchPattern> standardPatterns, SpecialMatch specialMatch)
        {
            StandardPatterns = standardPatterns;
            SpecialMatch = specialMatch;
        }
    }
}
