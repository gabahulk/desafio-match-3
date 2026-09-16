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

namespace Gazeus.DesafioMatch3.Tests.PlayMode
{
    public sealed class BoardResponsivePlayModeTests
    {
        private static readonly Vector2Int[] ViewportSizes =
        {
            new(1080, 1920),
            new(720, 1600),
            new(1920, 1080),
            new(2560, 1080),
            new(2048, 1539)
        };

        private static readonly Vector2Int[] BoardSizes =
        {
            new(10, 10),
            new(10, 5),
            new(8, 10)
        };

        [UnityTest]
        public IEnumerator Board_FitsAvailableArea_ForSupportedViewportsAndDimensions()
        {
            for (int boardIndex = 0; boardIndex < BoardSizes.Length; boardIndex++)
            {
                Vector2Int boardSize = BoardSizes[boardIndex];
                GameController controller = null;
                UnityAction<Scene, LoadSceneMode> startBoard = (scene, _) =>
                {
                    controller = FindController(scene);
                    controller?.StartGame(CreateBoard(boardSize.x, boardSize.y),
                        new[] { 0, 1, 2, 3 });
                };

                SceneManager.sceneLoaded += startBoard;
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
                    SceneManager.sceneLoaded -= startBoard;
                }

                Assert.That(controller, Is.Not.Null);
                yield return null;

                Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
                CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
                AssertCanvasConfiguration(canvasScaler);

                RectTransform canvasRect = (RectTransform)canvas.transform;
                RectTransform boardFrame = (RectTransform)GameObject.Find("BoardFrame").transform;
                Image boardFrameImage = boardFrame.GetComponent<Image>();
                Image background = GameObject.Find("Background").GetComponent<Image>();
                BoardFrameResponsiveController frameResponsiveController =
                    boardFrame.GetComponent<BoardFrameResponsiveController>();
                BoardResponsiveController responsiveController =
                    UnityEngine.Object.FindFirstObjectByType<BoardResponsiveController>();
                RectTransform boardContainer = responsiveController.GetComponent<RectTransform>();
                AspectRatioFitter aspectRatioFitter = responsiveController.GetComponent<AspectRatioFitter>();
                GridLayoutGroup gridLayoutGroup = responsiveController.GetComponent<GridLayoutGroup>();
                BoardView boardView = responsiveController.GetComponent<BoardView>();
                TileSpotView[] tileSpots = UnityEngine.Object.FindObjectsByType<TileSpotView>(
                    FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                AssertBoardFrameHierarchy(
                    boardFrame,
                    boardFrameImage,
                    background,
                    frameResponsiveController);
                Assert.That(aspectRatioFitter.aspectMode,
                    Is.EqualTo(AspectRatioFitter.AspectMode.FitInParent));
                Assert.That(tileSpots, Has.Length.EqualTo(boardSize.x * boardSize.y));
                AssertTileCoordinates(tileSpots, boardSize);

                canvas.renderMode = RenderMode.WorldSpace;
                canvasScaler.enabled = false;
                canvasRect.localScale = Vector3.one;

                for (int viewportIndex = 0; viewportIndex < ViewportSizes.Length; viewportIndex++)
                {
                    Vector2Int viewport = ViewportSizes[viewportIndex];
                    canvasRect.sizeDelta = viewport;
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRect);
                    yield return null;
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(canvasRect);

                    AssertLayout(
                        viewport,
                        boardSize,
                        boardFrame,
                        boardContainer,
                        aspectRatioFitter,
                        gridLayoutGroup,
                        tileSpots);

                    AssertInteractionCoordinates(boardView, tileSpots, boardSize);
                }
            }
        }

        private static void AssertCanvasConfiguration(CanvasScaler canvasScaler)
        {
            Assert.That(canvasScaler.uiScaleMode,
                Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(canvasScaler.referenceResolution, Is.EqualTo(new Vector2(1080, 1920)));
            Assert.That(canvasScaler.screenMatchMode,
                Is.EqualTo(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight));
            Assert.That(canvasScaler.matchWidthOrHeight, Is.EqualTo(0.5f));
        }

        private static void AssertBoardFrameHierarchy(
            RectTransform boardFrame,
            Image boardFrameImage,
            Image background,
            BoardFrameResponsiveController frameResponsiveController)
        {
            Assert.That(boardFrame.parent.name, Is.EqualTo("Canvas"));
            Assert.That(boardFrameImage, Is.Not.Null);
            Assert.That(boardFrameImage.type, Is.EqualTo(Image.Type.Sliced));
            Assert.That(boardFrameImage.sprite, Is.Not.Null);
            Assert.That(boardFrameImage.sprite.border, Is.EqualTo(new Vector4(150, 150, 150, 150)));
            Assert.That(boardFrameImage.raycastTarget, Is.False);
            Assert.That(background, Is.Not.Null);
            Assert.That(background.sprite, Is.Not.Null);
            Assert.That(background.raycastTarget, Is.False);
            Assert.That(frameResponsiveController, Is.Not.Null);
        }

        private static void AssertLayout(
            Vector2Int viewport,
            Vector2Int boardSize,
            RectTransform boardFrame,
            RectTransform boardContainer,
            AspectRatioFitter aspectRatioFitter,
            GridLayoutGroup gridLayoutGroup,
            IReadOnlyList<TileSpotView> tileSpots)
        {
            string caseName = $"viewport {viewport.x}x{viewport.y}, board {boardSize.x}x{boardSize.y}";
            float expectedAspectRatio = (float)boardSize.x / boardSize.y;
            float actualAspectRatio = boardContainer.rect.width / boardContainer.rect.height;
            float expectedCellSize = Mathf.Min(
                boardContainer.rect.width / boardSize.x,
                boardContainer.rect.height / boardSize.y);

            Assert.That(aspectRatioFitter.aspectRatio,
                Is.EqualTo(expectedAspectRatio).Within(0.001f), caseName);
            Assert.That(actualAspectRatio,
                Is.EqualTo(expectedAspectRatio).Within(0.001f), caseName);
            Assert.That(gridLayoutGroup.constraint,
                Is.EqualTo(GridLayoutGroup.Constraint.FixedColumnCount), caseName);
            Assert.That(gridLayoutGroup.constraintCount, Is.EqualTo(boardSize.x), caseName);
            Assert.That(gridLayoutGroup.cellSize.x,
                Is.EqualTo(gridLayoutGroup.cellSize.y).Within(0.001f), caseName);
            Assert.That(gridLayoutGroup.cellSize.x,
                Is.EqualTo(expectedCellSize).Within(0.05f), caseName);

            AssertPresentationProfile(viewport, boardFrame, caseName);

            Assert.That(boardContainer.rect.width,
                Is.LessThanOrEqualTo(boardFrame.rect.width + 0.05f), caseName);
            Assert.That(boardContainer.rect.height,
                Is.LessThanOrEqualTo(boardFrame.rect.height + 0.05f), caseName);
            Assert.That(gridLayoutGroup.cellSize.x * boardSize.x,
                Is.LessThanOrEqualTo(boardContainer.rect.width + 0.05f), caseName);
            Assert.That(gridLayoutGroup.cellSize.y * boardSize.y,
                Is.LessThanOrEqualTo(boardContainer.rect.height + 0.05f), caseName);

            Vector3 offsetCenter = boardFrame.TransformPoint(boardFrame.rect.center);
            Vector3 boardCenter = boardContainer.TransformPoint(boardContainer.rect.center);
            Assert.That(Vector3.Distance(offsetCenter, boardCenter), Is.LessThan(0.05f), caseName);

            for (int index = 0; index < tileSpots.Count; index++)
            {
                RectTransform tileRect = (RectTransform)tileSpots[index].transform;
                Assert.That(tileRect.rect.width,
                    Is.EqualTo(tileRect.rect.height).Within(0.05f), caseName);
                Assert.That(tileRect.rect.width,
                    Is.EqualTo(expectedCellSize).Within(0.05f), caseName);
            }
        }

        private static void AssertPresentationProfile(
            Vector2Int viewport,
            RectTransform boardFrame,
            string caseName)
        {
            bool isPortrait = viewport.y > viewport.x;
            Vector2 expectedMin = isPortrait ? new Vector2(0.05f, 0.1f) : new Vector2(0.16f, 0.1f);
            Vector2 expectedMax = isPortrait ? new Vector2(0.95f, 0.9f) : new Vector2(0.84f, 0.9f);

            Assert.That(boardFrame.anchorMin, Is.EqualTo(expectedMin), caseName);
            Assert.That(boardFrame.anchorMax, Is.EqualTo(expectedMax), caseName);
            Assert.That(boardFrame.offsetMin, Is.EqualTo(Vector2.zero), caseName);
            Assert.That(boardFrame.offsetMax, Is.EqualTo(Vector2.zero), caseName);
        }

        private static void AssertTileCoordinates(
            IReadOnlyList<TileSpotView> tileSpots,
            Vector2Int boardSize)
        {
            HashSet<Vector2Int> positions = new();
            for (int index = 0; index < tileSpots.Count; index++)
            {
                Vector2Int position = tileSpots[index].Position;
                Assert.That(position.x, Is.InRange(0, boardSize.x - 1));
                Assert.That(position.y, Is.InRange(0, boardSize.y - 1));
                Assert.That(positions.Add(position), Is.True,
                    $"Duplicate tile coordinate {position}.");
            }
        }

        private static void AssertInteractionCoordinates(
            BoardView boardView,
            IReadOnlyList<TileSpotView> tileSpots,
            Vector2Int boardSize)
        {
            Vector2Int expected = new(boardSize.x - 1, boardSize.y - 1);
            Vector2Int? clicked = null;
            void RecordClick(int x, int y) => clicked = new Vector2Int(x, y);

            boardView.TileClicked += RecordClick;
            try
            {
                for (int index = 0; index < tileSpots.Count; index++)
                {
                    if (tileSpots[index].Position == expected)
                    {
                        tileSpots[index].GetComponent<Button>().onClick.Invoke();
                        break;
                    }
                }
            }
            finally
            {
                boardView.TileClicked -= RecordClick;
            }

            Assert.That(clicked, Is.EqualTo(expected));
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

        private static Board CreateBoard(int width, int height)
        {
            const string symbols = "RGBY";
            string[] rows = new string[height];
            for (int y = 0; y < height; y++)
            {
                char[] row = new char[width];
                for (int x = 0; x < width; x++)
                {
                    row[x] = symbols[(x + y) % symbols.Length];
                }

                rows[y] = new string(row);
            }

            return PlayModeTestFixture.CreateBoard(rows);
        }
    }
}
