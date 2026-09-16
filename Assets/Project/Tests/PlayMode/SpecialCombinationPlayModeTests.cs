using System.Collections;
using Gazeus.DesafioMatch3.Models;
using NUnit.Framework;
using UnityEngine.TestTools;
using static Gazeus.DesafioMatch3.Tests.PlayMode.PlayModeTestFixture;

namespace Gazeus.DesafioMatch3.Tests.PlayMode
{
    public sealed class SpecialCombinationPlayModeTests
    {
        [UnityTest]
        public IEnumerator CombineStripedAndStriped_ThroughPlayerSwap_ClearsOneRowAndColumn()
        {
            Board board = CreateCombinationBoard(SpecialType.HorizontalStriped, SpecialType.VerticalStriped);
            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, 40, 41, 36, 5);
                AssertIdSurvived(finalBoard, 0);
            });
        }

        [UnityTest]
        public IEnumerator CombineStripedAndWrapped_ThroughPlayerSwap_ClearsThreeRowsAndColumns()
        {
            Board board = CreateCombinationBoard(SpecialType.HorizontalStriped, SpecialType.Wrapped);
            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, 40, 41, 27, 36, 45, 4, 5, 6);
                AssertIdSurvived(finalBoard, 0);
            });
        }

        [UnityTest]
        public IEnumerator CombineWrappedAndWrapped_ThroughPlayerSwap_CompletesBothSecondPhases()
        {
            Board board = CreateCombinationBoard(SpecialType.Wrapped, SpecialType.Wrapped);
            yield return RunCombinationScenario(board,
                finalBoard => AssertIdsAbsent(finalBoard, 40, 41, 20, 61));
        }

        [UnityTest]
        public IEnumerator CombineColorBombAndStriped_ThroughPlayerSwap_ActivatesPartnerAndGeneratedStripes()
        {
            Board board = CreateColorCombinationBoard(SpecialType.ColorBomb, SpecialType.HorizontalStriped);
            yield return RunCombinationScenario(board,
                finalBoard => AssertIdsAbsent(finalBoard, 40, 41, 10, 16, 64, 39, 11, 25, 19));
        }

        [UnityTest]
        public IEnumerator CombineStripedAndColorBomb_ThroughPlayerSwap_ActivatesPartnerAndGeneratedStripes()
        {
            Board board = CreateColorCombinationBoard(SpecialType.HorizontalStriped, SpecialType.ColorBomb);
            yield return RunCombinationScenario(board,
                finalBoard => AssertIdsAbsent(finalBoard, 40, 41, 10, 16, 64, 39, 11, 25, 19));
        }

        [UnityTest]
        public IEnumerator CombineColorBombAndWrapped_ThroughPlayerSwap_CompletesPartnerAndGeneratedWrappedPhases()
        {
            Board board = CreateColorCombinationBoard(
                SpecialType.ColorBomb, SpecialType.Wrapped, useCornerTargets: true);
            yield return RunCombinationScenario(board,
                finalBoard => AssertIdsAbsent(finalBoard, 40, 41, 0, 8, 72, 39, 1, 9, 7, 17, 63, 73));
        }

        [UnityTest]
        public IEnumerator CombineWrappedAndColorBomb_ThroughPlayerSwap_CompletesPartnerAndGeneratedWrappedPhases()
        {
            Board board = CreateColorCombinationBoard(
                SpecialType.Wrapped, SpecialType.ColorBomb, useCornerTargets: true);
            yield return RunCombinationScenario(board,
                finalBoard => AssertIdsAbsent(finalBoard, 40, 41, 0, 8, 72, 42, 1, 9, 7, 17, 63, 73));
        }

        [UnityTest]
        public IEnumerator CombineColorBombAndColorBomb_ThroughPlayerSwap_ConsumesEveryOriginalTile()
        {
            Board board = CreateCombinationBoard(SpecialType.ColorBomb, SpecialType.ColorBomb);
            yield return RunCombinationScenario(board, finalBoard =>
            {
                for (int tileId = 0; tileId < 81; tileId++)
                {
                    Assert.That(ContainsTileId(finalBoard, tileId), Is.False,
                        $"Original tile {tileId} survived the board clear.");
                }
            });
        }
    }
}
