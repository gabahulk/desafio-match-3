using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode.SpecialEffects
{
    public sealed class SpecialEffectsTests
    {
        [TestCase(SpecialType.HorizontalStriped, SpecialType.VerticalStriped)]
        [TestCase(SpecialType.HorizontalStriped, SpecialType.Wrapped)]
        [TestCase(SpecialType.Wrapped, SpecialType.Wrapped)]
        [TestCase(SpecialType.ColorBomb, SpecialType.HorizontalStriped)]
        [TestCase(SpecialType.ColorBomb, SpecialType.Wrapped)]
        [TestCase(SpecialType.ColorBomb, SpecialType.ColorBomb)]
        public void FindSwapMatch_SupportedSpecialPair_ReturnsSpecialMatch(
            SpecialType first, SpecialType second)
        {
            Board board = CreateSpecialPairBoard(first, second);
            (board[0, 0], board[1, 0]) = (board[1, 0], board[0, 0]);

            MatchResult result = MatchFinder.FindSwapMatch(board,
                new Vector2Int(0, 0), new Vector2Int(1, 0));

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.SpecialMatch, Is.Not.Null);
            Assert.That(result.StandardPatterns, Is.Empty);
        }

        [Test]
        public void FindSwapMatch_ColorBombAndNormal_ReturnsSpecialMatchWithoutPatterns()
        {
            Board board = CreateColorBombBoard();
            (board[0, 0], board[1, 0]) = (board[1, 0], board[0, 0]);

            MatchResult result = MatchFinder.FindSwapMatch(board,
                new Vector2Int(0, 0), new Vector2Int(1, 0));

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.StandardPatterns, Is.Empty);
            Assert.That(result.SpecialMatch, Is.Not.Null);
        }

        [Test]
        public void FindSwapMatch_NormalAndColorBomb_ReturnsSpecialMatchWithoutPatterns()
        {
            Board board = CreateColorBombBoard();
            (board[0, 0], board[1, 0]) = (board[1, 0], board[0, 0]);

            MatchResult result = MatchFinder.FindSwapMatch(board,
                new Vector2Int(1, 0), new Vector2Int(0, 0));

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.StandardPatterns, Is.Empty);
            Assert.That(result.SpecialMatch, Is.Not.Null);
        }

        [Test]
        public void FindSwapMatch_OrdinaryInvalidSwap_ReturnsNoMatch()
        {
            Board board = BoardFixture.Create("RGB", "GBY", "BYR");
            (board[0, 0], board[1, 0]) = (board[1, 0], board[0, 0]);

            MatchResult result = MatchFinder.FindSwapMatch(board,
                new Vector2Int(0, 0), new Vector2Int(1, 0));

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.SpecialMatch, Is.Null);
        }

        [Test]
        public void FindSwapMatch_SpecialPairTakesPrecedenceOverGeometricPattern()
        {
            Board board = CreateSpecialPairBoard(SpecialType.HorizontalStriped, SpecialType.Wrapped);
            board[1, 0].Color = 0;
            board[2, 0].Color = 0;
            (board[0, 0], board[1, 0]) = (board[1, 0], board[0, 0]);

            MatchResult result = MatchFinder.FindSwapMatch(board,
                new Vector2Int(0, 0), new Vector2Int(1, 0));

            Assert.That(result.SpecialMatch, Is.Not.Null);
            Assert.That(result.StandardPatterns, Is.Empty);
        }

        [Test]
        public void TrySwap_ColorBombAndNormal_IsValidAndReportsTargetColor()
        {
            Board board = CreateColorBombBoard();
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            BoardSequence first = result.BoardSequences[0];
            Assert.That(first.MatchedPosition, Does.Contain(new Vector2Int(1, 0)));
            Assert.That(first.MatchedPosition, Does.Contain(new Vector2Int(0, 0)));
            Assert.That(first.SpecialActivations, Has.Count.EqualTo(1));
            SpecialActivationInfo activation = first.SpecialActivations[0];
            Assert.That(activation.Special, Is.EqualTo(SpecialType.ColorBomb));
            Assert.That(activation.Position, Is.EqualTo(new Vector2Int(1, 0)));
            Assert.That(activation.TargetColor, Is.EqualTo(1));
        }

        [Test]
        public void TrySwap_NormalAndColorBomb_IsAlsoValid()
        {
            var service = new GameService(CreateColorBombBoard(), new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 1, 0, 0, 0);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.BoardSequences[0].SpecialActivations.Single().TargetColor, Is.EqualTo(1));
        }

        [Test]
        public void TrySwap_ColorBombHitsTargetColoredStriped_ActivatesItsLine()
        {
            Board board = CreateColorBombBoard();
            board[2, 2].Color = 1;
            board[2, 2].Special = SpecialType.HorizontalStriped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            Assert.That(result.BoardSequences[0].MatchedPosition,
                Does.Contain(new Vector2Int(4, 2)));
            Assert.That(result.BoardSequences[0].SpecialActivations.Any(info =>
                info.Special == SpecialType.HorizontalStriped && info.Position == new Vector2Int(2, 2)), Is.True);
        }

        [Test]
        public void TrySwap_ColorBombHitsTargetColoredWrapped_UsesBothWrappedPhases()
        {
            Board board = CreateColorBombBoard();
            board[2, 2].Color = 1;
            board[2, 2].Special = SpecialType.Wrapped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            Assert.That(result.BoardSequences.SelectMany(sequence => sequence.SpecialActivations).Any(info =>
                info.Special == SpecialType.Wrapped && info.Phase == SpecialActivationPhase.First), Is.True);
            Assert.That(result.BoardSequences.SelectMany(sequence => sequence.SpecialActivations).Any(info =>
                info.Special == SpecialType.Wrapped && info.Phase == SpecialActivationPhase.Second), Is.True);
        }

        [Test]
        public void TrySwap_StripedAndWrapped_UsesSingleCombinationActivation()
        {
            Board board = CreateSpecialPairBoard(SpecialType.HorizontalStriped, SpecialType.Wrapped);
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            Assert.That(result.BoardSequences[0].SpecialActivations, Has.Count.EqualTo(1));
            Assert.That(result.BoardSequences[0].SpecialActivations[0].CombinedWith,
                Is.EqualTo(SpecialType.HorizontalStriped));
        }

        [Test]
        public void TrySwap_WrappedAndWrapped_QueuesTwoIndependentSecondPhases()
        {
            Board board = CreateSpecialPairBoard(SpecialType.Wrapped, SpecialType.Wrapped);
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            Assert.That(result.BoardSequences[0].SpecialActivations.Count(info =>
                info.Special == SpecialType.Wrapped && info.CombinedWith == SpecialType.Wrapped &&
                info.Phase == SpecialActivationPhase.First), Is.EqualTo(1));
            Assert.That(result.BoardSequences.Skip(1).SelectMany(sequence => sequence.SpecialActivations).Count(info =>
                info.Special == SpecialType.Wrapped && info.Phase == SpecialActivationPhase.Second), Is.EqualTo(2));
        }

        [Test]
        public void TrySwap_TwoColorBombs_ClearBoardWithoutActivatingIndirectBomb()
        {
            Board board = CreateSpecialPairBoard(SpecialType.ColorBomb, SpecialType.ColorBomb);
            board[2, 0].Special = SpecialType.ColorBomb;
            board[2, 0].Color = -1;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            BoardSequence first = result.BoardSequences[0];
            Assert.That(first.MatchedPosition, Has.Count.EqualTo(board.Width * board.Height));
            Assert.That(first.SpecialActivations.Count(info => info.Special == SpecialType.ColorBomb), Is.EqualTo(1));
        }

        [TestCase(SpecialType.HorizontalStriped)]
        [TestCase(SpecialType.Wrapped)]
        public void TrySwap_ColorBombAndSpecial_UsesSpecialColorAndActivatesSpecialOnce(SpecialType special)
        {
            Board board = CreateSpecialPairBoard(SpecialType.ColorBomb, special);
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            BoardSequence first = result.BoardSequences[0];
            Assert.That(first.SpecialActivations.Single(info => info.Special == SpecialType.ColorBomb).TargetColor,
                Is.EqualTo(1));
            Assert.That(first.SpecialActivations.Single(info => info.Special == SpecialType.ColorBomb).CombinedWith,
                Is.EqualTo(special));
        }

        [TestCase(SpecialType.ColorBomb, SpecialType.HorizontalStriped, 0)]
        [TestCase(SpecialType.HorizontalStriped, SpecialType.ColorBomb, 1)]
        [TestCase(SpecialType.ColorBomb, SpecialType.Wrapped, 0)]
        [TestCase(SpecialType.Wrapped, SpecialType.ColorBomb, 1)]
        public void TrySwap_ColorBombAndDirectSpecial_ActivatesAndConsumesPartner(
            SpecialType first,
            SpecialType second,
            int partnerXAfterSwap)
        {
            Board board = CreateSpecialPairBoard(first, second);
            Tile partner = first == SpecialType.ColorBomb ? board[1, 0] : board[0, 0];
            partner.Color = 4;
            int partnerId = partner.Id;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            SpecialType partnerSpecial = first == SpecialType.ColorBomb ? second : first;
            Assert.That(result.BoardSequences.SelectMany(sequence => sequence.SpecialActivations).Count(info =>
                info.Special == partnerSpecial && info.Phase == SpecialActivationPhase.First), Is.EqualTo(1));
            Assert.That(result.BoardSequences[0].MatchedPosition,
                Does.Contain(partnerSpecial == SpecialType.HorizontalStriped
                    ? new Vector2Int(4, 0)
                    : new Vector2Int(partnerXAfterSwap == 0 ? 1 : 0, 1)));
            if (partnerSpecial == SpecialType.Wrapped)
            {
                Assert.That(result.BoardSequences.SelectMany(sequence => sequence.SpecialActivations).Count(info =>
                    info.Special == SpecialType.Wrapped && info.Phase == SpecialActivationPhase.Second),
                    Is.EqualTo(1));
            }

            Assert.That(ContainsTileId(service.Board, partnerId), Is.False);
        }

        [Test]
        public void TrySwap_IndirectlyHitColorBomb_IsDestroyedWithoutColorBombActivation()
        {
            Board board = BoardFixture.Create("RGBYG", "GBYRG", "RRRBG");
            board[0, 2].Special = SpecialType.HorizontalStriped;
            board[4, 2].Special = SpecialType.ColorBomb;
            board[4, 2].Color = -1;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 2, 1, 2);

            Assert.That(result.BoardSequences[0].MatchedPosition, Does.Contain(new Vector2Int(4, 2)));
            Assert.That(result.BoardSequences[0].SpecialActivations.Any(info =>
                info.Special == SpecialType.ColorBomb), Is.False);
        }

        private static Board CreateColorBombBoard()
        {
            Board board = BoardFixture.Create("RGBYB", "BRYGB", "YBGRY", "GBYRG");
            board[0, 0].Special = SpecialType.ColorBomb;
            board[0, 0].Color = -1;
            return board;
        }

        private static Board CreateSpecialPairBoard(SpecialType first, SpecialType second)
        {
            Board board = BoardFixture.Create("RGBYB", "BRYGB", "YBGRY", "GBYRG");
            board[0, 0].Special = first;
            board[0, 0].Color = first == SpecialType.ColorBomb ? -1 : 0;
            board[1, 0].Special = second;
            board[1, 0].Color = second == SpecialType.ColorBomb ? -1 : 1;
            return board;
        }

        private static MoveResult TrySwapDeterministically(GameService service, int fromX, int fromY, int toX, int toY)
        {
            Random.State state = Random.state;
            try
            {
                Random.InitState(48271);
                return service.TrySwap(fromX, fromY, toX, toY);
            }
            finally
            {
                Random.state = state;
            }
        }

        private static bool ContainsTileId(Board board, int tileId)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board[x, y].Id == tileId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
