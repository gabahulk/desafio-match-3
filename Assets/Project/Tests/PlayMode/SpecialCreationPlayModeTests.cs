using System;
using System.Collections;
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

        private static IEnumerator RunScenario(
            Board board,
            Vector2Int from,
            Vector2Int to,
            Vector2Int expectedPosition,
            SpecialType expectedSpecial,
            int expectedColor,
            int expectedTileId)
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

                Assert.That(CountSpecials(controller.Board), Is.Zero,
                    "The scenario must begin without special tiles.");
                ClickTile(from);
                ClickTile(to);

                yield return WaitForResolution(controller);
                yield return null;

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
            float deadline = Time.realtimeSinceStartup + 5.0f;
            while (controller.IsAnimating && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(controller.IsAnimating, Is.False,
                "Gameplay resolution did not complete within five seconds.");
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
