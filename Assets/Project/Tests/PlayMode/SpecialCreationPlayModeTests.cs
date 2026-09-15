using System;
using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Controllers;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Tests.PlayMode
{
    public sealed class SpecialCreationPlayModeTests
    {
        private static readonly int[] AvailableColors = { 0, 1, 2, 3 };

        [UnityTest]
        public IEnumerator CreateHorizontalStriped_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard(
                "GBRY",
                "BYRG",
                "YRGR",
                "RGRB");

            yield return RunScenario(
                board,
                new Vector2Int(3, 2),
                new Vector2Int(2, 2),
                new Vector2Int(2, 3),
                SpecialType.HorizontalStriped,
                0,
                11);
        }

        [UnityTest]
        public IEnumerator CreateVerticalStriped_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard(
                "RGBY",
                "GBYR",
                "RRGR",
                "BYRB");

            yield return RunScenario(
                board,
                new Vector2Int(2, 3),
                new Vector2Int(2, 2),
                new Vector2Int(2, 2),
                SpecialType.VerticalStriped,
                0,
                14);
        }

        [UnityTest]
        public IEnumerator CreateWrapped_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard(
                "RGBYR",
                "GBYRG",
                "BYRGB",
                "YRRBY",
                "RRGRR");

            yield return RunScenario(
                board,
                new Vector2Int(1, 4),
                new Vector2Int(2, 4),
                new Vector2Int(2, 4),
                SpecialType.Wrapped,
                0,
                21);
        }

        [UnityTest]
        public IEnumerator CreateColorBomb_ThroughPlayerSwap_RendersAndPersistsSpecial()
        {
            Board board = CreateBoard(
                "RGBYR",
                "GBYRG",
                "BYRGB",
                "YBRGY",
                "RRGRR");

            yield return RunScenario(
                board,
                new Vector2Int(2, 4),
                new Vector2Int(2, 3),
                new Vector2Int(2, 4),
                SpecialType.ColorBomb,
                -1,
                17);
        }

        [UnityTest]
        public IEnumerator Score_PlayerMatch_VisibleTotalMatchesGameService()
        {
            Board board = CreateBoard(
                "RGBY",
                "GBYR",
                "BYRG",
                "RRGR");

            yield return RunGameplay(
                board,
                new Vector2Int(2, 3),
                new Vector2Int(3, 3),
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
        public IEnumerator CombineStripedAndStriped_ThroughPlayerSwap_ClearsOneRowAndColumn()
        {
            Board board = CreateCombinationBoard(
                SpecialType.HorizontalStriped,
                SpecialType.VerticalStriped);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, 40, 41, 36, 5);
                AssertIdSurvived(finalBoard, 0);
            });
        }

        [UnityTest]
        public IEnumerator CombineStripedAndWrapped_ThroughPlayerSwap_ClearsThreeRowsAndColumns()
        {
            Board board = CreateCombinationBoard(
                SpecialType.HorizontalStriped,
                SpecialType.Wrapped);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    40, 41,
                    27, 36, 45,
                    4, 5, 6);
                AssertIdSurvived(finalBoard, 0);
            });
        }

        [UnityTest]
        public IEnumerator CombineWrappedAndWrapped_ThroughPlayerSwap_CompletesBothSecondPhases()
        {
            Board board = CreateCombinationBoard(
                SpecialType.Wrapped,
                SpecialType.Wrapped);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, 40, 41, 20, 61);
            });
        }

        [UnityTest]
        public IEnumerator CombineColorBombAndStriped_ThroughPlayerSwap_ActivatesPartnerAndGeneratedStripes()
        {
            Board board = CreateColorCombinationBoard(
                SpecialType.ColorBomb,
                SpecialType.HorizontalStriped);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    40, 41,
                    10, 16, 64,
                    39, 11, 25, 19);
            });
        }

        [UnityTest]
        public IEnumerator CombineStripedAndColorBomb_ThroughPlayerSwap_ActivatesPartnerAndGeneratedStripes()
        {
            Board board = CreateColorCombinationBoard(
                SpecialType.HorizontalStriped,
                SpecialType.ColorBomb);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    40, 41,
                    10, 16, 64,
                    39, 11, 25, 19);
            });
        }

        [UnityTest]
        public IEnumerator CombineColorBombAndWrapped_ThroughPlayerSwap_CompletesPartnerAndGeneratedWrappedPhases()
        {
            Board board = CreateColorCombinationBoard(
                SpecialType.ColorBomb,
                SpecialType.Wrapped,
                useCornerTargets: true);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    40, 41,
                    0, 8, 72,
                    39,
                    1, 9, 7, 17, 63, 73);
            });
        }

        [UnityTest]
        public IEnumerator CombineWrappedAndColorBomb_ThroughPlayerSwap_CompletesPartnerAndGeneratedWrappedPhases()
        {
            Board board = CreateColorCombinationBoard(
                SpecialType.Wrapped,
                SpecialType.ColorBomb,
                useCornerTargets: true);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    40, 41,
                    0, 8, 72,
                    42,
                    1, 9, 7, 17, 63, 73);
            });
        }

        [UnityTest]
        public IEnumerator CombineColorBombAndColorBomb_ThroughPlayerSwap_ConsumesEveryOriginalTile()
        {
            Board board = CreateCombinationBoard(
                SpecialType.ColorBomb,
                SpecialType.ColorBomb);

            yield return RunCombinationScenario(board, finalBoard =>
            {
                for (int tileId = 0; tileId < 81; tileId++)
                {
                    Assert.That(ContainsTileId(finalBoard, tileId), Is.False,
                        $"Original tile {tileId} survived the board clear.");
                }
            });
        }

        [UnityTest]
        public IEnumerator ChainStripedToStriped_ThroughPlayerSwap_ActivatesSecondLine()
        {
            Board board = CreateIndirectChainBoard(SpecialType.HorizontalStriped, SpecialType.VerticalStriped);

            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, 1);
            });
        }

        [UnityTest]
        public IEnumerator ChainStripedToWrapped_ThroughPlayerSwap_CompletesBothWrappedPhases()
        {
            Board board = CreateIndirectChainBoard(SpecialType.HorizontalStriped, SpecialType.Wrapped);

            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    27,
                    18);
            });
        }

        [UnityTest]
        public IEnumerator ChainStripedToColorBomb_ThroughPlayerSwap_ClearsMostCommonColor()
        {
            Board board = CreateIndirectChainBoard(
                SpecialType.HorizontalStriped,
                SpecialType.ColorBomb,
                useMajorityBoard: true);
            int[] majorityColorIds = GetTileIdsWithColor(board, 0);

            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, majorityColorIds);
            });
        }

        [UnityTest]
        public IEnumerator ChainWrappedToStriped_ThroughPlayerSwap_ActivatesLineAndSecondWrappedPhase()
        {
            Board board = CreateIndirectChainBoard(SpecialType.Wrapped, SpecialType.VerticalStriped);

            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    4,
                    24);
            });
        }

        [UnityTest]
        public IEnumerator ChainWrappedToWrapped_ThroughPlayerSwap_CompletesIndirectWrappedSecondPhase()
        {
            Board board = CreateIndirectChainBoard(SpecialType.Wrapped, SpecialType.Wrapped);

            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard,
                    30,
                    12);
                AssertIdSurvived(finalBoard, 0);
            });
        }

        [UnityTest]
        public IEnumerator ChainWrappedToColorBomb_ThroughPlayerSwap_ClearsMostCommonColorAndSecondWrappedPhase()
        {
            Board board = CreateIndirectChainBoard(
                SpecialType.Wrapped,
                SpecialType.ColorBomb,
                useMajorityBoard: true);
            int[] majorityColorIds = GetTileIdsWithColor(board, 0);

            yield return RunIndirectChainScenario(board, finalBoard =>
            {
                AssertIdsAbsent(finalBoard, majorityColorIds);
                AssertIdsAbsent(finalBoard, 23);
            });
        }

        private static IEnumerator RunScenario(
            Board board,
            Vector2Int from,
            Vector2Int to,
            Vector2Int expectedPosition,
            SpecialType expectedSpecial,
            int expectedColor,
            int expectedTileId)
        {
            yield return RunGameplay(
                board,
                from,
                to,
                controller =>
                {
                    Assert.That(CountSpecials(controller.Board), Is.Zero,
                        "The scenario must begin without special tiles.");
                },
                controller =>
                {
                    Assert.That(CountSpecials(controller.Board, expectedSpecial), Is.EqualTo(1));
                    Tile specialTile = controller.Board[expectedPosition.x, expectedPosition.y];
                    Assert.That(specialTile.Special, Is.EqualTo(expectedSpecial));
                    Assert.That(specialTile.Color, Is.EqualTo(expectedColor));
                    Assert.That(specialTile.Id, Is.EqualTo(expectedTileId));

                    TileSpotView tileSpot = FindTileSpot(expectedPosition);
                    Assert.That(
                        FindDescendant(tileSpot.transform, $"{expectedSpecial} Overlay"),
                        Is.Not.Null,
                        $"{expectedSpecial} was not rendered at {expectedPosition}.");
                });
        }

        private static IEnumerator RunCombinationScenario(
            Board board,
            Action<Board> assertFinalBoard)
        {
            yield return RunGameplay(
                board,
                new Vector2Int(4, 4),
                new Vector2Int(5, 4),
                controller =>
                {
                    Assert.That(CountSpecials(controller.Board), Is.EqualTo(2),
                        "The scenario must begin with exactly the seeded special pair.");
                },
                controller => assertFinalBoard(controller.Board));
        }

        private static IEnumerator RunIndirectChainScenario(
            Board board,
            Action<Board> assertFinalBoard)
        {
            yield return RunGameplay(
                board,
                new Vector2Int(4, 4),
                new Vector2Int(5, 4),
                controller =>
                {
                    Assert.That(CountSpecials(controller.Board), Is.EqualTo(2),
                        "The scenario must begin with exactly the seeded indirect chain specials.");
                },
                controller => assertFinalBoard(controller.Board));
        }

        private static IEnumerator RunGameplay(
            Board board,
            Vector2Int from,
            Vector2Int to,
            Action<GameController> assertInitialState,
            Action<GameController> assertFinalState)
        {
            Random.State originalRandomState = Random.state;
            try
            {
                Random.InitState(48271);
                AssertBoardHasNoMatches(board);
                GameController controller = null;
                UnityAction<Scene, LoadSceneMode> startExplicitBoard = (scene, _) =>
                {
                    controller = FindController(scene);
                    controller?.StartGame(board, AvailableColors);
                };

                SceneManager.sceneLoaded += startExplicitBoard;
                try
                {
                    SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
                    float loadDeadline = Time.realtimeSinceStartup + 5.0f;
                    while (controller == null && Time.realtimeSinceStartup < loadDeadline)
                    {
                        yield return null;
                    }
                }
                finally
                {
                    SceneManager.sceneLoaded -= startExplicitBoard;
                }

                Assert.That(controller, Is.Not.Null);

                yield return null;

                assertInitialState(controller);
                ClickTile(from);
                ClickTile(to);

                yield return WaitForResolution(controller);
                yield return null;

                assertFinalState(controller);
                AssertRenderedBoardMatchesDomain(controller.Board);
            }
            finally
            {
                Random.state = originalRandomState;
            }
        }

        private static GameController FindController(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameController controller = roots[index].GetComponentInChildren<GameController>(true);
                if (controller != null)
                {
                    return controller;
                }
            }

            return null;
        }

        private static IEnumerator WaitForResolution(GameController controller)
        {
            float deadline = Time.realtimeSinceStartup + 20.0f;
            while (controller.IsAnimating && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(controller.IsAnimating, Is.False,
                "Gameplay resolution did not complete within twenty seconds.");
        }

        private static void ClickTile(Vector2Int position)
        {
            TileSpotView tileSpot = FindTileSpot(position);
            Button button = tileSpot.GetComponent<Button>();
            Assert.That(button, Is.Not.Null);
            button.onClick.Invoke();
        }

        private static TileSpotView FindTileSpot(Vector2Int position)
        {
            TileSpotView[] tileSpots = UnityEngine.Object.FindObjectsByType<TileSpotView>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            for (int index = 0; index < tileSpots.Length; index++)
            {
                if (tileSpots[index].Position == position)
                {
                    return tileSpots[index];
                }
            }

            Assert.Fail($"No rendered tile spot was found at {position}.");
            return null;
        }

        private static Text FindScoreText()
        {
            GameObject scoreObject = GameObject.Find("Score");
            Assert.That(scoreObject, Is.Not.Null, "The score display was not found in the Gameplay UI.");
            Text scoreText = scoreObject.GetComponent<Text>();
            Assert.That(scoreText, Is.Not.Null);
            return scoreText;
        }

        private static Transform FindDescendant(Transform parent, string name)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == name)
                {
                    return child;
                }

                Transform descendant = FindDescendant(child, name);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private static int CountSpecials(Board board, SpecialType? special = null)
        {
            int count = 0;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (special.HasValue
                        ? board[x, y].Special == special.Value
                        : board[x, y].Special != SpecialType.None)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static void AssertRenderedBoardMatchesDomain(Board board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    TileSpotView tileSpot = FindTileSpot(new Vector2Int(x, y));
                    Assert.That(tileSpot.transform.childCount,
                        Is.EqualTo(tile.IsEmpty ? 0 : 1),
                        $"Rendered occupancy differs from the domain at ({x}, {y}).");

                    if (tile.Special != SpecialType.None)
                    {
                        Assert.That(
                            FindDescendant(tileSpot.transform, $"{tile.Special} Overlay"),
                            Is.Not.Null,
                            $"{tile.Special} is missing its overlay at ({x}, {y}).");
                    }
                }
            }
        }

        private static void AssertIdsAbsent(Board board, params int[] tileIds)
        {
            for (int index = 0; index < tileIds.Length; index++)
            {
                Assert.That(ContainsTileId(board, tileIds[index]), Is.False,
                    $"Expected affected tile {tileIds[index]} to be consumed.");
            }
        }

        private static void AssertIdSurvived(Board board, int tileId)
        {
            Assert.That(ContainsTileId(board, tileId), Is.True,
                $"Expected unaffected tile {tileId} to survive.");
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

        private static void AssertBoardHasNoMatches(Board board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (x >= 2)
                    {
                        Assert.That(
                            board[x, y].Color == board[x - 1, y].Color &&
                            board[x - 1, y].Color == board[x - 2, y].Color,
                            Is.False,
                            $"Scenario begins with a horizontal match ending at ({x}, {y}).");
                    }

                    if (y >= 2)
                    {
                        Assert.That(
                            board[x, y].Color == board[x, y - 1].Color &&
                            board[x, y - 1].Color == board[x, y - 2].Color,
                            Is.False,
                            $"Scenario begins with a vertical match ending at ({x}, {y}).");
                    }
                }
            }
        }

        private static Board CreateBoard(params string[] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                throw new ArgumentException("At least one row is required.", nameof(rows));
            }

            int width = rows[0].Length;
            Board board = new(width, rows.Length);
            int tileId = 0;
            for (int y = 0; y < rows.Length; y++)
            {
                if (rows[y] == null || rows[y].Length != width)
                {
                    throw new ArgumentException(
                        $"Row {y} must contain exactly {width} symbols.",
                        nameof(rows));
                }

                for (int x = 0; x < width; x++)
                {
                    board[x, y] = new Tile
                    {
                        Id = tileId++,
                        Color = GetColor(rows[y][x]),
                        Special = SpecialType.None
                    };
                }
            }

            return board;
        }

        private static Board CreateCombinationBoard(SpecialType first, SpecialType second)
        {
            Board board = CreateBoard(
                "RGBYRGBYR",
                "GBYRGBYRG",
                "BYRGBYRGB",
                "YRGBYRGBY",
                "RGBYRGBYR",
                "GBYRGBYRG",
                "BYRGBYRGB",
                "YRGBYRGBY",
                "RGBYRGBYR");

            SetSpecial(board[4, 4], first, 0);
            SetSpecial(board[5, 4], second, 1);
            return board;
        }

        private static Board CreateColorCombinationBoard(
            SpecialType first,
            SpecialType second,
            bool useCornerTargets = false)
        {
            Board board = CreateCombinationBoard(first, second);
            Tile partner = first == SpecialType.ColorBomb ? board[5, 4] : board[4, 4];
            partner.Color = 4;

            Vector2Int[] targets = useCornerTargets
                ? new[]
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(8, 0),
                    new Vector2Int(0, 8)
                }
                : new[]
                {
                    new Vector2Int(1, 1),
                    new Vector2Int(7, 1),
                    new Vector2Int(1, 7)
                };

            for (int index = 0; index < targets.Length; index++)
            {
                Vector2Int target = targets[index];
                board[target.x, target.y].Color = 4;
            }

            return board;
        }

        private static Board CreateIndirectChainBoard(
            SpecialType source,
            SpecialType target,
            bool useMajorityBoard = false)
        {
            Board board = useMajorityBoard
                ? CreateBoard(
                    "RRGRRGRRG",
                    "RGRRGRRGR",
                    "GRRGRRGRR",
                    "RRGRRGRRG",
                    "RGRRGRRGR",
                    "GRRGRRGRR",
                    "RRGRRGRRG",
                    "RGRRGRRGR",
                    "GRRGRRGRR")
                : CreateBoard(
                    "RGBYRGBYR",
                    "GBYRGBYRG",
                    "BYRGBYRGB",
                    "YRGBYRGBY",
                    "RGBYRGBYR",
                    "GBYRGBYRG",
                    "BYRGBYRGB",
                    "YRGBYRGBY",
                    "RGBYRGBYR");

            board[6, 4].Color = 0;
            board[7, 4].Color = 0;
            board[8, 4].Color = useMajorityBoard ? 2 : 1;
            board[5, 4].Color = 1;
            if (useMajorityBoard)
            {
                board[3, 4].Color = 1;
                board[4, 3].Color = 1;
                board[4, 5].Color = 1;
                board[7, 3].Color = 1;
                board[7, 5].Color = 1;
                board[5, 2].Color = 2;
            }
            SetSpecial(board[4, 4], source, 0);

            Vector2Int targetPosition = source == SpecialType.Wrapped
                ? new Vector2Int(4, 3)
                : new Vector2Int(1, 4);
            SetSpecial(board[targetPosition.x, targetPosition.y], target, 1);
            return board;
        }

        private static int[] GetTileIdsWithColor(Board board, int color)
        {
            List<int> tileIds = new();
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    if (!tile.IsEmpty && tile.Color == color)
                    {
                        tileIds.Add(tile.Id);
                    }
                }
            }

            return tileIds.ToArray();
        }

        private static void SetSpecial(Tile tile, SpecialType special, int color)
        {
            tile.Special = special;
            tile.Color = special == SpecialType.ColorBomb ? -1 : color;
        }

        private static int GetColor(char symbol)
        {
            return symbol switch
            {
                'R' => 0,
                'G' => 1,
                'B' => 2,
                'Y' => 3,
                _ => throw new ArgumentException($"Unsupported board symbol '{symbol}'.")
            };
        }
    }
}
