using System.Collections.Generic;
using System.Linq;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Tests.EditMode.Fixtures;
using NUnit.Framework;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class StripedCandyTests
    {
        [Test]
        public void TrySwap_HorizontalSwapCreatesMatchFour_CreatesProtectedHorizontalStripedAtTo()
        {
            Board board = BoardFixture.Create(
                "RGBYG",
                "GBYRG",
                "BYGBY",
                "RRRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 4, 3, 3, 3);

            BoardSequence sequence = result.BoardSequences[0];
            AssertCreatedSpecial(
                sequence,
                new Vector2Int(3, 3),
                0,
                SpecialType.HorizontalStriped);
            Assert.That(sequence.MatchedPositions, Is.EqualTo(new[]
            {
                new Vector2Int(0, 3),
                new Vector2Int(1, 3),
                new Vector2Int(2, 3)
            }));
            Assert.That(sequence.AddedTiles.All(tile => tile.Position != new Vector2Int(3, 3)), Is.True);
        }

        [Test]
        public void TrySwap_VerticalSwapCreatesMatchFour_CreatesProtectedVerticalStripedAtTo()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "RBYG",
                "RYGB",
                "GBYR",
                "RYGB"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 4, 0, 3);

            BoardSequence sequence = result.BoardSequences[0];
            AssertCreatedSpecial(
                sequence,
                new Vector2Int(0, 3),
                0,
                SpecialType.VerticalStriped);
            Assert.That(sequence.MatchedPositions, Is.EqualTo(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2)
            }));
        }

        [Test]
        public void TrySwap_WhenToContainsExistingSpecial_FallsBackToNormalFromCell()
        {
            Board board = BoardFixture.Create(
                "RRRRG",
                "GBYGB",
                "BYGBY"
            );
            board[0, 0].Special = SpecialType.HorizontalStriped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            AssertCreatedSpecial(
                result.BoardSequences[0],
                new Vector2Int(0, 0),
                0,
                SpecialType.HorizontalStriped);
            Assert.That(result.BoardSequences[0].MatchedPositions, Has.None.EqualTo(new Vector2Int(0, 0)));
        }

        [Test]
        public void TrySwap_MatchThree_CreatesNoSpecial()
        {
            Board board = BoardFixture.Create(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 3, 3, 3);

            Assert.That(result.BoardSequences[0].CreatedSpecialTiles, Is.Empty);
        }

        [Test]
        public void TrySwap_MatchFive_CreatesColorBombAndPreservesItsCell()
        {
            Board board = BoardFixture.Create(
                "RGBYR",
                "GBYRG",
                "BYRGB",
                "YBRGY",
                "RRGRR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 2, 4, 2, 3);

            BoardSequence sequence = result.BoardSequences[0];
            Assert.That(sequence.CreatedSpecialTiles, Has.Count.EqualTo(1));
            Assert.That(sequence.CreatedSpecialTiles[0].Special, Is.EqualTo(SpecialType.ColorBomb));
            Assert.That(sequence.CreatedSpecialTiles[0].Color, Is.EqualTo(-1));
            Assert.That(sequence.MatchedPositions, Has.Count.EqualTo(4));
        }

        [Test]
        public void TrySwap_MatchedHorizontalStriped_ExpandsDestructionToEntireRow()
        {
            Board board = BoardFixture.Create(
                "GBYRG",
                "BYGBY",
                "RRRBG"
            );
            board[0, 2].Special = SpecialType.HorizontalStriped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 2, 1, 2);

            Assert.That(result.BoardSequences[0].MatchedPositions, Is.EqualTo(new[]
            {
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2)
            }));
        }

        [Test]
        public void TrySwap_MatchedVerticalStriped_ExpandsDestructionToEntireColumn()
        {
            Board board = BoardFixture.Create(
                "RGB",
                "RBY",
                "RYG",
                "GBR",
                "BYG"
            );
            board[0, 0].Special = SpecialType.VerticalStriped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 0, 1);

            Assert.That(result.BoardSequences[0].MatchedPositions, Is.EqualTo(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, 3),
                new Vector2Int(0, 4)
            }));
        }

        [Test]
        public void TrySwap_StripedBlastHitsStriped_ExpandsBothEffectsWithoutDuplicates()
        {
            Board board = BoardFixture.Create(
                "GBYRR",
                "BYRBB",
                "RRRGB",
                "YGBYR",
                "GBYRG"
            );
            board[0, 2].Special = SpecialType.HorizontalStriped;
            board[4, 2].Special = SpecialType.VerticalStriped;
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 2, 1, 2);

            List<Vector2Int> matched = result.BoardSequences[0].MatchedPositions;
            Assert.That(matched, Has.Count.EqualTo(9));
            Assert.That(matched.Distinct().Count(), Is.EqualTo(9));
            Assert.That(matched, Is.EqualTo(new[]
            {
                new Vector2Int(4, 0),
                new Vector2Int(4, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2),
                new Vector2Int(3, 2),
                new Vector2Int(4, 2),
                new Vector2Int(4, 3),
                new Vector2Int(4, 4)
            }));
        }

        [Test]
        public void TrySwap_TwoIndependentMatchFours_CreatesTwoProtectedStripedTiles()
        {
            Board board = BoardFixture.Create(
                "RRRRG",
                "GBYGB",
                "BBBBR"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            BoardSequence sequence = result.BoardSequences[0];
            Assert.That(sequence.CreatedSpecialTiles, Has.Count.EqualTo(2));
            Assert.That(
                sequence.CreatedSpecialTiles.Select(tile => tile.Position).Distinct().Count(),
                Is.EqualTo(2));
            Assert.That(sequence.CreatedSpecialTiles[0].Position, Is.EqualTo(new Vector2Int(1, 0)));
            Assert.That(sequence.CreatedSpecialTiles[1].Position, Is.EqualTo(new Vector2Int(0, 2)));
            Assert.That(sequence.MatchedPositions, Has.None.EqualTo(sequence.CreatedSpecialTiles[0].Position));
            Assert.That(sequence.MatchedPositions, Has.None.EqualTo(sequence.CreatedSpecialTiles[1].Position));
        }

        [Test]
        public void TrySwap_MatchFourContainingOnlyExistingSpecials_DoesNotOverwriteAnyTile()
        {
            Board board = BoardFixture.Create(
                "RRRRG",
                "GBYGB",
                "BYGBY"
            );
            for (int x = 0; x < 4; x++)
            {
                board[x, 0].Special = x % 2 == 0
                    ? SpecialType.HorizontalStriped
                    : SpecialType.VerticalStriped;
            }

            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 0, 0, 1, 0);

            Assert.That(result.BoardSequences[0].CreatedSpecialTiles, Is.Empty);
        }

        [Test]
        public void TrySwap_HorizontalCascadeMatchFour_UsesMovedDestinationAndPatternOrientation()
        {
            Board board = BoardFixture.Create(
                "GBRYGB",
                "RYGBRY",
                "BYGRGB",
                "RRBRGY",
                "GYBYRG",
                "YGRBGB"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 3, 5, 2, 5);

            Assert.That(result.BoardSequences, Has.Count.GreaterThanOrEqualTo(2));
            BoardSequence cascade = result.BoardSequences[1];
            AssertCreatedSpecial(
                cascade,
                new Vector2Int(2, 3),
                0,
                SpecialType.HorizontalStriped);
            Assert.That(cascade.MovedTiles.Any(move => move.To == new Vector2Int(2, 3)), Is.False);
            Assert.That(result.BoardSequences[0].MovedTiles.Any(move => move.To == new Vector2Int(2, 3)), Is.True);
            Assert.That(cascade.MatchedPositions, Has.None.EqualTo(new Vector2Int(2, 3)));
            Assert.That(cascade.MatchedPositions, Has.None.EqualTo(new Vector2Int(4, 3)));
            Assert.That(cascade.MatchedPositions, Has.None.EqualTo(new Vector2Int(5, 3)));
        }

        [Test]
        public void TrySwap_VerticalCascadeMatchFour_UsesFirstRowMajorMovedDestination()
        {
            Board board = BoardFixture.Create(
                "GBYRG",
                "BYGRB",
                "YBBGB",
                "GYBRG",
                "BYGRB",
                "RGBGY"
            );
            var service = new GameService(board, new[] { 0, 1, 2, 3 });

            MoveResult result = TrySwapDeterministically(service, 4, 2, 3, 2);

            Assert.That(result.BoardSequences, Has.Count.GreaterThanOrEqualTo(2));
            BoardSequence cascade = result.BoardSequences[1];
            AssertCreatedSpecial(
                cascade,
                new Vector2Int(3, 1),
                0,
                SpecialType.VerticalStriped);
            Assert.That(result.BoardSequences[0].MovedTiles.Any(move => move.To == new Vector2Int(3, 1)), Is.True);
            Assert.That(cascade.MatchedPositions, Has.None.EqualTo(new Vector2Int(3, 1)));
            Assert.That(cascade.MatchedPositions, Has.None.EqualTo(new Vector2Int(3, 5)));
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

        private static void AssertCreatedSpecial(
            BoardSequence sequence,
            Vector2Int expectedPosition,
            int expectedColor,
            SpecialType expectedSpecial)
        {
            Assert.That(sequence.CreatedSpecialTiles, Has.Count.EqualTo(1));
            SpecialTileInfo created = sequence.CreatedSpecialTiles[0];
            Assert.That(created.Position, Is.EqualTo(expectedPosition));
            Assert.That(created.Color, Is.EqualTo(expectedColor));
            Assert.That(created.Special, Is.EqualTo(expectedSpecial));
        }
    }
}
