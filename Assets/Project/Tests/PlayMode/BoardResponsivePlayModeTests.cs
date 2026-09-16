using System;
using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Controllers;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using NUnit.Framework;
using TMPro;
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
            new(1080, 2340),
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
                RectTransform boardRegion = (RectTransform)GameObject.Find("BoardRegion").transform;
                RectTransform boardFrame = (RectTransform)GameObject.Find("BoardFrame").transform;
                RectTransform boardContent = (RectTransform)GameObject.Find("BoardContent").transform;
                RectTransform score = (RectTransform)GameObject.Find("Score").transform;
                Image boardFrameImage = boardFrame.GetComponent<Image>();
                Image plaque = score.Find("Plaque").GetComponent<Image>();
                TMP_Text label = score.Find("Label").GetComponent<TMP_Text>();
                TMP_Text value = score.Find("Value").GetComponent<TMP_Text>();
                Image background = GameObject.Find("Background").GetComponent<Image>();
                BoardFrameResponsiveController frameResponsiveController =
                    boardRegion.GetComponent<BoardFrameResponsiveController>();
                BoardResponsiveController responsiveController =
                    UnityEngine.Object.FindFirstObjectByType<BoardResponsiveController>();
                RectTransform boardContainer = responsiveController.GetComponent<RectTransform>();
                AspectRatioFitter aspectRatioFitter = responsiveController.GetComponent<AspectRatioFitter>();
                AspectRatioFitter frameAspectRatioFitter = boardFrame.GetComponent<AspectRatioFitter>();
                GridLayoutGroup gridLayoutGroup = responsiveController.GetComponent<GridLayoutGroup>();
                BoardView boardView = responsiveController.GetComponent<BoardView>();
                TileSpotView[] tileSpots = UnityEngine.Object.FindObjectsByType<TileSpotView>(
                    FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                AssertBoardFrameHierarchy(
                    boardRegion,
                    boardFrame,
                    boardContent,
                    boardFrameImage,
                    score,
                    plaque,
                    background,
                    frameResponsiveController);
                Assert.That(aspectRatioFitter.aspectMode,
                    Is.EqualTo(AspectRatioFitter.AspectMode.FitInParent));
                AssertScoreAnchoring(score, label, value);
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
                        boardRegion,
                        boardFrame,
                        boardContent,
                        boardContainer,
                        aspectRatioFitter,
                        frameAspectRatioFitter,
                        gridLayoutGroup,
                        tileSpots,
                        background);

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
            RectTransform boardRegion,
            RectTransform boardFrame,
            RectTransform boardContent,
            Image boardFrameImage,
            RectTransform score,
            Image plaque,
            Image background,
            BoardFrameResponsiveController frameResponsiveController)
        {
            Assert.That(boardRegion.parent.name, Is.EqualTo("Canvas"));
            Assert.That(boardFrame.parent, Is.EqualTo(boardRegion));
            Assert.That(boardContent.parent, Is.EqualTo(boardFrame));
            Assert.That(boardContent.GetComponent<RectMask2D>(), Is.Not.Null);
            Assert.That(boardFrame.GetComponent<RectMask2D>(), Is.Null);
            Assert.That(boardFrameImage, Is.Not.Null);
            Assert.That(boardFrameImage.type, Is.EqualTo(Image.Type.Sliced));
            Assert.That(boardFrameImage.sprite, Is.Not.Null);
            Assert.That(boardFrameImage.sprite.border, Is.EqualTo(new Vector4(150, 150, 150, 150)));
            Assert.That(boardFrameImage.raycastTarget, Is.False);
            Assert.That(score.parent.name, Is.EqualTo("Canvas"));
            Assert.That(plaque.type, Is.EqualTo(Image.Type.Sliced));
            Assert.That(plaque.preserveAspect, Is.True);
            Assert.That(plaque.raycastTarget, Is.False);
            Assert.That(background, Is.Not.Null);
            Assert.That(background.sprite, Is.Not.Null);
            Assert.That(background.raycastTarget, Is.False);
            Assert.That(frameResponsiveController, Is.Not.Null);
        }

        private static void AssertLayout(
            Vector2Int viewport,
            Vector2Int boardSize,
            RectTransform boardRegion,
            RectTransform boardFrame,
            RectTransform boardContent,
            RectTransform boardContainer,
            AspectRatioFitter aspectRatioFitter,
            AspectRatioFitter frameAspectRatioFitter,
            GridLayoutGroup gridLayoutGroup,
            IReadOnlyList<TileSpotView> tileSpots,
            Image background)
        {
            string caseName = $"viewport {viewport.x}x{viewport.y}, board {boardSize.x}x{boardSize.y}";
            float expectedAspectRatio = (float)boardSize.x / boardSize.y;
            float actualAspectRatio = boardContainer.rect.width / boardContainer.rect.height;
            float expectedCellSize = Mathf.Min(
                boardContainer.rect.width / boardSize.x,
                boardContainer.rect.height / boardSize.y);

            Assert.That(aspectRatioFitter.aspectRatio,
                Is.EqualTo(expectedAspectRatio).Within(0.001f), caseName);
            Assert.That(frameAspectRatioFitter.aspectMode,
                Is.EqualTo(AspectRatioFitter.AspectMode.FitInParent), caseName);
            Assert.That(frameAspectRatioFitter.aspectRatio,
                Is.EqualTo(expectedAspectRatio).Within(0.001f), caseName);
            Assert.That(boardFrame.rect.width / boardFrame.rect.height,
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

            AssertPresentationProfile(viewport, boardRegion, background, caseName);

            Assert.That(boardFrame.rect.width,
                Is.LessThanOrEqualTo(boardRegion.rect.width + 0.05f), caseName);
            Assert.That(boardFrame.rect.height,
                Is.LessThanOrEqualTo(boardRegion.rect.height + 0.05f), caseName);
            Assert.That(boardContent.offsetMin, Is.EqualTo(new Vector2(80, 80)), caseName);
            Assert.That(boardContent.offsetMax, Is.EqualTo(new Vector2(-80, -80)), caseName);

            Assert.That(boardContainer.rect.width,
                Is.LessThanOrEqualTo(boardContent.rect.width + 0.05f), caseName);
            Assert.That(boardContainer.rect.height,
                Is.LessThanOrEqualTo(boardContent.rect.height + 0.05f), caseName);
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

        private static void AssertScoreAnchoring(RectTransform score, TMP_Text label, TMP_Text value)
        {
            Assert.That(score.parent.name, Is.EqualTo("Canvas"));
            Assert.That(score.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.9f)));
            Assert.That(score.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.9f)));
            Assert.That(score.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(score.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(score.sizeDelta, Is.EqualTo(new Vector2(420.0f, 140.0f)));
            Assert.That(label.text, Is.EqualTo("SCORE"));
            Assert.That(label.font, Is.Not.Null);
            Assert.That(label.font.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Dynamic));
            Assert.That(value.text, Is.EqualTo("0"));
            Assert.That(value.font, Is.EqualTo(label.font));
        }

        private static void AssertPresentationProfile(
            Vector2Int viewport,
            RectTransform boardRegion,
            Image background,
            string caseName)
        {
            bool isPortrait = viewport.y > viewport.x;
            Vector2 expectedMin = isPortrait ? new Vector2(0, 0.17f) : new Vector2(0.17f, 0.05f);
            Vector2 expectedMax = isPortrait ? new Vector2(1, 0.83f) : new Vector2(0.84f, 0.85f);

            Assert.That(boardRegion.anchorMin, Is.EqualTo(expectedMin), caseName);
            Assert.That(boardRegion.anchorMax, Is.EqualTo(expectedMax), caseName);
            Assert.That(boardRegion.offsetMin, Is.EqualTo(Vector2.zero), caseName);
            Assert.That(boardRegion.offsetMax, Is.EqualTo(Vector2.zero), caseName);
            Assert.That(background.sprite.name, Is.EqualTo(isPortrait ? "bg-portrait" : "bg"), caseName);
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
