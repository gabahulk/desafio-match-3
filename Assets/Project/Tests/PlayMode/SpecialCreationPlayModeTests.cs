using System.Collections;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.TestTools;
using static Gazeus.DesafioMatch3.Tests.PlayMode.PlayModeTestFixture;

namespace Gazeus.DesafioMatch3.Tests.PlayMode
{
    public sealed class SpecialCreationPlayModeTests
    {
        [UnityTest]
        public IEnumerator CreateHorizontalStriped_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard("GBRY", "BYRG", "YRGR", "RGRB");
            yield return RunCreationScenario(board, new Vector2Int(3, 2), new Vector2Int(2, 2),
                new Vector2Int(2, 3), SpecialType.HorizontalStriped, 0, 11);
        }

        [UnityTest]
        public IEnumerator CreateVerticalStriped_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard("RGBY", "GBYR", "RRGR", "BYRB");
            yield return RunCreationScenario(board, new Vector2Int(2, 3), new Vector2Int(2, 2),
                new Vector2Int(2, 2), SpecialType.VerticalStriped, 0, 14);
        }

        [UnityTest]
        public IEnumerator CreateWrapped_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard("RGBYR", "GBYRG", "BYRGB", "YRRBY", "RRGRR");
            yield return RunCreationScenario(board, new Vector2Int(1, 4), new Vector2Int(2, 4),
                new Vector2Int(2, 4), SpecialType.Wrapped, 0, 21);
        }

        [UnityTest]
        public IEnumerator CreateColorBomb_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard("RGBYR", "GBYRG", "BYRGB", "YBRGY", "RRGRR");
            yield return RunCreationScenario(board, new Vector2Int(2, 4), new Vector2Int(2, 3),
                new Vector2Int(2, 4), SpecialType.ColorBomb, -1, 17);
        }
    }
}
