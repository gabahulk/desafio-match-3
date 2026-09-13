using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class ColorBombTests
    {
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
    }
}
