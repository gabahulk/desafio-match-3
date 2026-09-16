using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal static class CombinationEffectUtility
    {
        internal static bool IsStriped(SpecialType type)
        {
            return type == SpecialType.HorizontalStriped ||
                   type == SpecialType.VerticalStriped;
        }

        internal static List<Vector2Int> Area(Board board, Vector2Int center, int radius)
        {
            List<Vector2Int> cells = new();
            for (int y = Mathf.Max(0, center.y - radius);
                 y <= Mathf.Min(board.Height - 1, center.y + radius);
                 y++)
            {
                for (int x = Mathf.Max(0, center.x - radius);
                     x <= Mathf.Min(board.Width - 1, center.x + radius);
                     x++)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }

            return cells;
        }
    }

    internal sealed class StripedStripedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context)
        {
            return context.CombinedWith.HasValue &&
                   CombinationEffectUtility.IsStriped(context.Special) &&
                   CombinationEffectUtility.IsStriped(context.CombinedWith.Value);
        }

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            List<Vector2Int> cells = new();
            for (int x = 0; x < board.Width; x++)
            {
                cells.Add(new Vector2Int(x, context.EffectCenter.y));
            }

            for (int y = 0; y < board.Height; y++)
            {
                cells.Add(new Vector2Int(context.EffectCenter.x, y));
            }

            return new SpecialActivationResult(cells);
        }
    }

    internal sealed class StripedWrappedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context)
        {
            return context.CombinedWith.HasValue &&
                   (CombinationEffectUtility.IsStriped(context.Special) &&
                    context.CombinedWith == SpecialType.Wrapped ||
                    context.Special == SpecialType.Wrapped &&
                    CombinationEffectUtility.IsStriped(context.CombinedWith.Value));
        }

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            List<Vector2Int> cells = new();
            for (int y = Mathf.Max(0, context.EffectCenter.y - 1);
                 y <= Mathf.Min(board.Height - 1, context.EffectCenter.y + 1);
                 y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }

            for (int x = Mathf.Max(0, context.EffectCenter.x - 1);
                 x <= Mathf.Min(board.Width - 1, context.EffectCenter.x + 1);
                 x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }

            return new SpecialActivationResult(cells);
        }
    }

    internal sealed class WrappedWrappedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context)
        {
            return context.Special == SpecialType.Wrapped &&
                   context.CombinedWith == SpecialType.Wrapped;
        }

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            List<Vector2Int> cells = CombinationEffectUtility.Area(board, context.Position, 2);
            if (context.Phase == SpecialActivationPhase.Second)
            {
                return new SpecialActivationResult(cells);
            }

            Vector2Int partnerPosition = context.PartnerPosition ?? context.Position;
            cells.AddRange(CombinationEffectUtility.Area(board, partnerPosition, 2));
            List<Vector2Int> preservedCells = new() { context.Position, partnerPosition };
            List<PendingSpecialActivation> pendingActivations = new()
            {
                new PendingSpecialActivation(
                    board[context.Position.x, context.Position.y].Id,
                    SpecialType.Wrapped,
                    SpecialActivationPhase.Second,
                    SpecialType.Wrapped),
                new PendingSpecialActivation(
                    board[partnerPosition.x, partnerPosition.y].Id,
                    SpecialType.Wrapped,
                    SpecialActivationPhase.Second,
                    SpecialType.Wrapped)
            };

            return new SpecialActivationResult(
                cells,
                preservedCells,
                pendingActivations,
                Array.Empty<SpecialActivationContext>(),
                Array.Empty<SpecialTransformation>());
        }
    }

    internal sealed class ColorBombStripedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context)
        {
            return context.CombinedWith.HasValue &&
                   (context.Special == SpecialType.ColorBomb &&
                    CombinationEffectUtility.IsStriped(context.CombinedWith.Value) ||
                    CombinationEffectUtility.IsStriped(context.Special) &&
                    context.CombinedWith == SpecialType.ColorBomb);
        }

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            return TransformMatchingTiles(board, context, createStripes: true);
        }

        internal static SpecialActivationResult TransformMatchingTiles(
            Board board,
            SpecialActivationContext context,
            bool createStripes)
        {
            Vector2Int partnerPosition = context.PartnerPosition ?? context.Position;
            Vector2Int bombPosition = board[context.Position.x, context.Position.y].Special == SpecialType.ColorBomb
                ? context.Position
                : partnerPosition;
            Vector2Int colorSourcePosition = bombPosition == context.Position
                ? partnerPosition
                : context.Position;
            int targetColor = board[colorSourcePosition.x, colorSourcePosition.y].Color;
            List<SpecialTransformation> transformations = new();
            List<SpecialActivationContext> triggeredActivations = new();
            int stripeCount = 0;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    if (tile.IsEmpty || tile.Color != targetColor)
                    {
                        continue;
                    }

                    SpecialType special = createStripes
                        ? stripeCount++ % 2 == 0
                            ? SpecialType.HorizontalStriped
                            : SpecialType.VerticalStriped
                        : SpecialType.Wrapped;
                    Vector2Int position = new(x, y);
                    transformations.Add(new SpecialTransformation(position, special));
                    triggeredActivations.Add(new SpecialActivationContext(
                        position, SpecialActivationPhase.First, special: special));
                }
            }

            return new SpecialActivationResult(
                new[] { bombPosition },
                Array.Empty<Vector2Int>(),
                Array.Empty<PendingSpecialActivation>(),
                triggeredActivations,
                transformations);
        }
    }

    internal sealed class ColorBombWrappedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context)
        {
            return context.CombinedWith.HasValue &&
                   (context.Special == SpecialType.ColorBomb &&
                    context.CombinedWith == SpecialType.Wrapped ||
                    context.Special == SpecialType.Wrapped &&
                    context.CombinedWith == SpecialType.ColorBomb);
        }

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            return ColorBombStripedEffect.TransformMatchingTiles(
                board, context, createStripes: false);
        }
    }

    internal sealed class ColorBombColorBombEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context)
        {
            return context.Special == SpecialType.ColorBomb &&
                   context.CombinedWith == SpecialType.ColorBomb;
        }

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            List<Vector2Int> cells = new();
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (!board[x, y].IsEmpty)
                    {
                        cells.Add(new Vector2Int(x, y));
                    }
                }
            }

            return new SpecialActivationResult(cells);
        }
    }
}
