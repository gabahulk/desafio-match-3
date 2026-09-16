using System.Collections;
using Gazeus.DesafioMatch3.Models;
using UnityEngine.TestTools;
using static Gazeus.DesafioMatch3.Tests.PlayMode.PlayModeTestFixture;

namespace Gazeus.DesafioMatch3.Tests.PlayMode
{
    public sealed class SpecialChainPlayModeTests
    {
        [UnityTest]
        public IEnumerator ChainStripedToStriped_ThroughPlayerSwap_ActivatesSecondLine()
        {
            Board board = CreateIndirectChainBoard(SpecialType.HorizontalStriped, SpecialType.VerticalStriped);
            yield return RunIndirectChainScenario(board, finalBoard => AssertIdsAbsent(finalBoard, 1));
        }

        [UnityTest]
        public IEnumerator ChainStripedToWrapped_ThroughPlayerSwap_CompletesBothWrappedPhases()
        {
            Board board = CreateIndirectChainBoard(SpecialType.HorizontalStriped, SpecialType.Wrapped);
            yield return RunIndirectChainScenario(board, finalBoard => AssertIdsAbsent(finalBoard, 27, 18));
        }

        [UnityTest]
        public IEnumerator ChainStripedToColorBomb_ThroughPlayerSwap_ClearsMostCommonColor()
        {
            Board board = CreateIndirectChainBoard(
                SpecialType.HorizontalStriped, SpecialType.ColorBomb, useMajorityBoard: true);
            int[] majorityColorIds = GetTileIdsWithColor(board, 0);
            yield return RunIndirectChainScenario(board,
                finalBoard => AssertIdsAbsent(finalBoard, majorityColorIds));
        }

        [UnityTest]
        public IEnumerator ChainWrappedToStriped_ThroughPlayerSwap_ActivatesLineAndSecondWrappedPhase()
        {
            Board board = CreateIndirectChainBoard(SpecialType.Wrapped, SpecialType.VerticalStriped);
            yield return RunIndirectChainScenario(board, finalBoard => AssertIdsAbsent(finalBoard, 4, 24));
        }

        [UnityTest]
        public IEnumerator ChainWrappedToWrapped_ThroughPlayerSwap_CompletesIndirectWrappedSecondPhase()
        {
            Board board = CreateIndirectChainBoard(SpecialType.Wrapped, SpecialType.Wrapped);
            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, 30, 12);
                AssertIdSurvived(finalBoard, 0);
            });
        }

        [UnityTest]
        public IEnumerator ChainWrappedToColorBomb_ThroughPlayerSwap_ClearsMostCommonColorAndSecondWrappedPhase()
        {
            Board board = CreateIndirectChainBoard(
                SpecialType.Wrapped, SpecialType.ColorBomb, useMajorityBoard: true);
            int[] majorityColorIds = GetTileIdsWithColor(board, 0);
            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, majorityColorIds);
                AssertIdsAbsent(finalBoard, 23);
            });
        }
    }
}
