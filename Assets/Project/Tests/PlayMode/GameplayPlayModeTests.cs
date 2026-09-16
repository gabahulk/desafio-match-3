using System.Collections;
using Gazeus.DesafioMatch3.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static Gazeus.DesafioMatch3.Tests.PlayMode.PlayModeTestFixture;

namespace Gazeus.DesafioMatch3.Tests.PlayMode
{
    public sealed class GameplayPlayModeTests
    {
        [UnityTest]
        public IEnumerator Score_PlayerMatch_VisibleTotalMatchesGameService()
        {
            Board board = CreateBoard("RGBY", "GBYR", "BYRG", "RRGR");
            yield return RunGameplay(board, new Vector2Int(2, 3), new Vector2Int(3, 3),
                controller =>
                {
                    Text scoreText = FindScoreText();
                    Assert.That(controller.Score, Is.Zero);
                    Assert.That(scoreText.text, Is.EqualTo("SCORE\n0"));
                    Assert.That(scoreText.gameObject.activeInHierarchy, Is.True);
                },
                controller =>
                {
                    Text scoreText = FindScoreText();
                    Assert.That(controller.Score, Is.GreaterThan(0));
                    Assert.That(scoreText.text, Is.EqualTo($"SCORE\n{controller.Score}"));
                });
        }

        [UnityTest]
        public IEnumerator SameCellClick_CancelsSelectionWithoutStartingSwapAnimation()
        {
            Board board = CreateBoard("RGBY", "GBYR", "BYRG", "RRGR");
            Vector2Int position = new(1, 1);
            int tileId = -1;
            yield return RunGameplay(board, position, position,
                controller => tileId = controller.Board[position.x, position.y].Id,
                controller =>
                {
                    Assert.That(controller.IsAnimating, Is.False);
                    Assert.That(controller.Score, Is.Zero);
                    Assert.That(controller.Board[position.x, position.y].Id, Is.EqualTo(tileId));
                });
        }
    }
}
