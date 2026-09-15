using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class GameServiceScoringTests
    {
        [Test]
        public void TrySwap_SimpleMatch_AwardsTenPointsPerDestroyedTile()
        {
            var service = new GameService(CreateSimpleMatchBoard(), new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 3, 3, 3);

            Assert.That(result.BoardSequences[0].MatchedPosition, Has.Count.EqualTo(3));
            Assert.That(result.BoardSequences[0].ScoreGained, Is.EqualTo(30));
            Assert.That(result.BoardSequences[0].TotalScore, Is.EqualTo(30));
        }

        [Test]
        public void TrySwap_StripedEffect_AwardsPointsForEveryDestroyedCell()
        {
            Board board = CreateTriggeredSpecialBoard();
            SetSpecial(board[4, 4], SpecialType.HorizontalStriped, 0);
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 4, 4, 5, 4);

            Assert.That(result.BoardSequences[0].MatchedPosition, Has.Count.EqualTo(9));
            Assert.That(result.BoardSequences[0].ScoreGained, Is.EqualTo(90));
        }

        [Test]
        public void TrySwap_DirectCombination_AwardsPointsForEffectiveDestructionCells()
        {
            Board board = CreateSpecialPairBoard(
                SpecialType.HorizontalStriped,
                SpecialType.VerticalStriped);
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 4, 4, 5, 4);

            Assert.That(result.BoardSequences[0].MatchedPosition, Has.Count.EqualTo(17));
            Assert.That(result.BoardSequences[0].ScoreGained, Is.EqualTo(170));
        }

        [Test]
        public void TrySwap_ChainReaction_AwardsPointsForEffectiveDestructionCells()
        {
            Board board = CreateTriggeredSpecialBoard();
            SetSpecial(board[4, 4], SpecialType.HorizontalStriped, 0);
            SetSpecial(board[1, 4], SpecialType.VerticalStriped, 1);
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 4, 4, 5, 4);

            BoardSequence first = result.BoardSequences[0];
            Assert.That(first.SpecialActivations.Any(info =>
                info.Special == SpecialType.VerticalStriped), Is.True);
            Assert.That(first.MatchedPosition, Has.Count.EqualTo(17));
            Assert.That(first.ScoreGained, Is.EqualTo(170));
        }

        [Test]
        public void TrySwap_RefillCreatesMatch_IncreasesCascadeMultiplier()
        {
            var service = new GameService(CreateSimpleMatchBoard(), new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 3, 3, 3);

            Assert.That(result.BoardSequences, Has.Count.EqualTo(2));
            Assert.That(result.BoardSequences[0].ScoreGained,
                Is.EqualTo(result.BoardSequences[0].MatchedPosition.Count * 10));
            Assert.That(result.BoardSequences[1].ScoreGained,
                Is.EqualTo(result.BoardSequences[1].MatchedPosition.Count * 10 * 2));
            Assert.That(result.BoardSequences[1].TotalScore,
                Is.EqualTo(result.BoardSequences.Sum(sequence => sequence.ScoreGained)));
            Assert.That(service.Score, Is.EqualTo(result.BoardSequences[1].TotalScore));
        }

        [Test]
        public void TrySwap_WrappedSecondPhase_KeepsInitialResolutionMultiplier()
        {
            Board board = CreateTriggeredSpecialBoard();
            SetSpecial(board[4, 4], SpecialType.Wrapped, 0);
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 4, 4, 5, 4);

            BoardSequence first = result.BoardSequences[0];
            BoardSequence second = result.BoardSequences[1];
            Assert.That(first.SpecialActivations.Single().Phase, Is.EqualTo(SpecialActivationPhase.First));
            Assert.That(second.SpecialActivations.Single().Phase, Is.EqualTo(SpecialActivationPhase.Second));
            Assert.That(first.ScoreGained, Is.EqualTo(first.MatchedPosition.Count * 10));
            Assert.That(second.ScoreGained, Is.EqualTo(second.MatchedPosition.Count * 10));
            Assert.That(second.TotalScore, Is.EqualTo(first.ScoreGained + second.ScoreGained));
        }

        [Test]
        public void StartGame_AfterScoring_ResetsScoreToZero()
        {
            var service = new GameService(CreateSimpleMatchBoard(), new[] { 0, 1, 2, 3 });
            TrySwapDeterministically(service, 2, 3, 3, 3);
            Assert.That(service.Score, Is.GreaterThan(0));

            service.StartGame(CreateSimpleMatchBoard(), new[] { 0, 1, 2, 3 });

            Assert.That(service.Score, Is.Zero);
        }

        private static Board CreateSimpleMatchBoard()
        {
            return BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR");
        }

        private static Board CreateTriggeredSpecialBoard()
        {
            Board board = CreateNineByNineBoard();
            board[6, 4].Color = 0;
            board[7, 4].Color = 0;
            board[8, 4].Color = 1;
            return board;
        }

        private static Board CreateSpecialPairBoard(SpecialType first, SpecialType second)
        {
            Board board = CreateNineByNineBoard();
            SetSpecial(board[4, 4], first, 0);
            SetSpecial(board[5, 4], second, 1);
            return board;
        }

        private static Board CreateNineByNineBoard()
        {
            return BoardFixture.Create(
                "RGBYRGBYR",
                "GBYRGBYRG",
                "BYRGBYRGB",
                "YRGBYRGBY",
                "RGBYRGBYR",
                "GBYRGBYRG",
                "BYRGBYRGB",
                "YRGBYRGBY",
                "RGBYRGBYR");
        }

        private static void SetSpecial(Tile tile, SpecialType special, int color)
        {
            tile.Special = special;
            tile.Color = special == SpecialType.ColorBomb ? -1 : color;
        }

        private static MoveResult TrySwapDeterministically(
            GameService service,
            int fromX,
            int fromY,
            int toX,
            int toY)
        {
            Random.State originalRandomState = Random.state;
            try
            {
                Random.InitState(48271);
                MoveResult result = service.TrySwap(fromX, fromY, toX, toY);
                Assert.That(result.IsValid, Is.True);
                return result;
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }
    }
}
