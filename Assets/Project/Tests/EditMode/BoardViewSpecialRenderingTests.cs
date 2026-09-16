using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.ScriptableObjects;
using Gazeus.DesafioMatch3.Views;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Tests.EditMode
{
    public sealed class BoardViewSpecialRenderingTests
    {
        [Test]
        public void CreateBoardAndApplyCreatedSpecials_RenderSpecialOverlays()
        {
            GameObject root = new("Board View Test Root", typeof(RectTransform));
            GameObject tileSpotObject = new("Tile Spot", typeof(RectTransform), typeof(Button));
            GameObject tilePrefab = new("Tile", typeof(RectTransform), typeof(Image));
            TilePrefabRepository repository = ScriptableObject.CreateInstance<TilePrefabRepository>();

            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(400, 100);
                GridLayoutGroup grid = root.AddComponent<GridLayoutGroup>();
                AspectRatioFitter aspectRatioFitter = root.AddComponent<AspectRatioFitter>();
                BoardResponsiveController responsiveController =
                    root.AddComponent<BoardResponsiveController>();
                BoardView boardView = root.AddComponent<BoardView>();
                TileSpotView tileSpot = tileSpotObject.AddComponent<TileSpotView>();

                SerializedObject repositoryData = new(repository);
                SerializedProperty prefabs = repositoryData.FindProperty("_colorPrefabList");
                prefabs.arraySize = 1;
                prefabs.GetArrayElementAtIndex(0).objectReferenceValue = tilePrefab;
                repositoryData.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject responsiveData = new(responsiveController);
                responsiveData.FindProperty("_boardContainer").objectReferenceValue = rootRect;
                responsiveData.FindProperty("_aspectRatioFitter").objectReferenceValue = aspectRatioFitter;
                responsiveData.FindProperty("_gridLayoutGroup").objectReferenceValue = grid;
                responsiveData.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject boardViewData = new(boardView);
                boardViewData.FindProperty("_boardContainer").objectReferenceValue = grid;
                boardViewData.FindProperty("_responsiveController").objectReferenceValue =
                    responsiveController;
                boardViewData.FindProperty("_tilePrefabRepository").objectReferenceValue = repository;
                boardViewData.FindProperty("_tileSpotPrefab").objectReferenceValue = tileSpot;
                boardViewData.ApplyModifiedPropertiesWithoutUndo();

                Board board = new(4, 1);
                board[0, 0] = new Tile
                {
                    Id = 0,
                    Color = 0,
                    Special = SpecialType.HorizontalStriped
                };
                board[1, 0] = new Tile
                {
                    Id = 1,
                    Color = 0,
                    Special = SpecialType.Wrapped
                };
                board[2, 0] = new Tile
                {
                    Id = 2,
                    Color = -1,
                    Special = SpecialType.ColorBomb
                };
                board[3, 0] = new Tile
                {
                    Id = 3,
                    Color = 0,
                    Special = SpecialType.None
                };

                boardView.CreateBoard(board);
                Assert.That(
                    FindDescendant(root.transform, "HorizontalStriped Overlay"),
                    Is.Not.Null);
                Assert.That(
                    FindDescendant(root.transform, "Wrapped Overlay"),
                    Is.Not.Null);
                Assert.That(
                    FindDescendant(root.transform, "ColorBomb Overlay"),
                    Is.Not.Null);

                boardView.ApplyCreatedSpecials(new List<SpecialTileInfo>
                {
                    new()
                    {
                        Position = new Vector2Int(3, 0),
                        Color = 0,
                        Special = SpecialType.VerticalStriped
                    }
                });

                Assert.That(
                    FindDescendant(root.transform, "VerticalStriped Overlay"),
                    Is.Not.Null);

                Transform horizontalOverlay = FindDescendant(
                    root.transform,
                    "HorizontalStriped Overlay");
                GameObject transformedTile = horizontalOverlay.parent.gameObject;
                boardView.ApplySpecialTransformations(new List<SpecialTileInfo>
                {
                    new()
                    {
                        Position = new Vector2Int(0, 0),
                        Color = 0,
                        Special = SpecialType.Wrapped
                    }
                });

                Assert.That(
                    transformedTile.transform.Find("Wrapped Overlay"),
                    Is.Not.Null);
                Assert.That(
                    transformedTile.transform.Find("HorizontalStriped Overlay"),
                    Is.Null,
                    "A transformation must remove the previous special overlay.");
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(tileSpotObject);
                Object.DestroyImmediate(tilePrefab);
                Object.DestroyImmediate(repository);
            }
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
    }
}
