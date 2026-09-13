using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal static class CombinationEffectUtility
    {
        internal static bool IsStriped(SpecialType type) => type == SpecialType.HorizontalStriped || type == SpecialType.VerticalStriped;
        internal static List<Vector2Int> Area(Board board, Vector2Int center, int radius)
        {
            List<Vector2Int> cells = new();
            for (int y = Mathf.Max(0, center.y - radius); y <= Mathf.Min(board.Height - 1, center.y + radius); y++)
            for (int x = Mathf.Max(0, center.x - radius); x <= Mathf.Min(board.Width - 1, center.x + radius); x++)
                cells.Add(new Vector2Int(x, y));
            return cells;
        }
    }

    internal sealed class StripedStripedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext c) => c.CombinedWith.HasValue && CombinationEffectUtility.IsStriped(c.Special) && CombinationEffectUtility.IsStriped(c.CombinedWith.Value);
        public SpecialActivationResult Activate(Board board, SpecialActivationContext c)
        {
            List<Vector2Int> cells = new();
            for (int x = 0; x < board.Width; x++) cells.Add(new Vector2Int(x, c.Position.y));
            for (int y = 0; y < board.Height; y++) cells.Add(new Vector2Int(c.Position.x, y));
            return new SpecialActivationResult(cells, System.Array.Empty<Vector2Int>(), System.Array.Empty<PendingSpecialActivation>(), System.Array.Empty<SpecialActivationContext>(), System.Array.Empty<SpecialTransformation>());
        }
    }

    internal sealed class StripedWrappedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext c) => c.CombinedWith.HasValue && ((CombinationEffectUtility.IsStriped(c.Special) && c.CombinedWith == SpecialType.Wrapped) || (c.Special == SpecialType.Wrapped && CombinationEffectUtility.IsStriped(c.CombinedWith.Value)));
        public SpecialActivationResult Activate(Board board, SpecialActivationContext c)
        {
            List<Vector2Int> cells = new();
            for (int y = Mathf.Max(0, c.Position.y - 1); y <= Mathf.Min(board.Height - 1, c.Position.y + 1); y++) for (int x = 0; x < board.Width; x++) cells.Add(new Vector2Int(x, y));
            for (int x = Mathf.Max(0, c.Position.x - 1); x <= Mathf.Min(board.Width - 1, c.Position.x + 1); x++) for (int y = 0; y < board.Height; y++) cells.Add(new Vector2Int(x, y));
            return new SpecialActivationResult(cells, System.Array.Empty<Vector2Int>(), System.Array.Empty<PendingSpecialActivation>(), System.Array.Empty<SpecialActivationContext>(), System.Array.Empty<SpecialTransformation>());
        }
    }

    internal sealed class WrappedWrappedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext c) => c.Special == SpecialType.Wrapped && c.CombinedWith == SpecialType.Wrapped;
        public SpecialActivationResult Activate(Board board, SpecialActivationContext c)
        {
            List<Vector2Int> cells = CombinationEffectUtility.Area(board, c.Position, 2);
            if (c.Phase == SpecialActivationPhase.Second)
                return new SpecialActivationResult(cells, System.Array.Empty<Vector2Int>(), System.Array.Empty<PendingSpecialActivation>(), System.Array.Empty<SpecialActivationContext>(), System.Array.Empty<SpecialTransformation>());
            Vector2Int partner = c.PartnerPosition ?? c.Position;
            cells.AddRange(CombinationEffectUtility.Area(board, partner, 2));
            List<Vector2Int> preserved = new() { c.Position, partner };
            List<PendingSpecialActivation> pending = new()
            {
                new PendingSpecialActivation(board[c.Position.x, c.Position.y].Id, SpecialType.Wrapped, SpecialActivationPhase.Second, SpecialType.Wrapped),
                new PendingSpecialActivation(board[partner.x, partner.y].Id, SpecialType.Wrapped, SpecialActivationPhase.Second, SpecialType.Wrapped)
            };
            return new SpecialActivationResult(cells, preserved, pending, System.Array.Empty<SpecialActivationContext>(), System.Array.Empty<SpecialTransformation>());
        }
    }

    internal sealed class ColorBombStripedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext c) => c.CombinedWith.HasValue && (c.Special == SpecialType.ColorBomb && CombinationEffectUtility.IsStriped(c.CombinedWith.Value) || CombinationEffectUtility.IsStriped(c.Special) && c.CombinedWith == SpecialType.ColorBomb);
        public SpecialActivationResult Activate(Board board, SpecialActivationContext c) => Transform(board, c, true);
        internal static SpecialActivationResult Transform(Board board, SpecialActivationContext c, bool striped)
        {
            Vector2Int partner = c.PartnerPosition ?? c.Position;
            Vector2Int bomb = board[c.Position.x, c.Position.y].Special == SpecialType.ColorBomb ? c.Position : partner;
            Vector2Int source = bomb == c.Position ? partner : c.Position;
            int color = board[source.x, source.y].Color;
            List<SpecialTransformation> transforms = new(); List<SpecialActivationContext> triggers = new(); int count = 0;
            for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++) if (!board[x,y].IsEmpty && board[x,y].Color == color)
            {
                SpecialType type = striped ? (count++ % 2 == 0 ? SpecialType.HorizontalStriped : SpecialType.VerticalStriped) : SpecialType.Wrapped;
                Vector2Int p = new(x,y); transforms.Add(new SpecialTransformation(p, type)); triggers.Add(new SpecialActivationContext(p, SpecialActivationPhase.First, special:type));
            }
            return new SpecialActivationResult(new[] { bomb }, System.Array.Empty<Vector2Int>(), System.Array.Empty<PendingSpecialActivation>(), triggers, transforms);
        }
    }

    internal sealed class ColorBombWrappedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext c) => c.CombinedWith.HasValue && (c.Special == SpecialType.ColorBomb && c.CombinedWith == SpecialType.Wrapped || c.Special == SpecialType.Wrapped && c.CombinedWith == SpecialType.ColorBomb);
        public SpecialActivationResult Activate(Board board, SpecialActivationContext c) => ColorBombStripedEffect.Transform(board, c, false);
    }

    internal sealed class ColorBombColorBombEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext c) => c.Special == SpecialType.ColorBomb && c.CombinedWith == SpecialType.ColorBomb;
        public SpecialActivationResult Activate(Board board, SpecialActivationContext c)
        {
            List<Vector2Int> cells = new(); for (int y=0;y<board.Height;y++) for(int x=0;x<board.Width;x++) if(!board[x,y].IsEmpty) cells.Add(new Vector2Int(x,y));
            return new SpecialActivationResult(cells, System.Array.Empty<Vector2Int>(), System.Array.Empty<PendingSpecialActivation>(), System.Array.Empty<SpecialActivationContext>(), System.Array.Empty<SpecialTransformation>());
        }
    }
}
