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
    public sealed class BoardViewTileVisualTests
    {
        [Test]
        public void TileVisualRepositoryAsset_MapsGeneratedSpritesAndUsesUiImportSettings()
        {
            TileVisualRepository repository = AssetDatabase.LoadAssetAtPath<TileVisualRepository>(
                "Assets/Project/ScriptableObjects/TileVisualRepository.asset");
            string[] suits = { "spades", "clubs", "diamond", "heart" };

            Assert.That(repository, Is.Not.Null);
            Assert.That(repository.TilePrefab, Is.Not.Null);
            Image prefabImage = repository.TilePrefab.GetComponent<Image>();
            Assert.That(prefabImage, Is.Not.Null);
            Assert.That(prefabImage.color, Is.EqualTo(Color.white));
            Assert.That(prefabImage.preserveAspect, Is.True);

            for (int color = 0; color < suits.Length; color++)
            {
                AssertSprite(repository.GetSprite(color, SpecialType.None),
                    $"tile-{suits[color]}");
                AssertSprite(repository.GetSprite(color, SpecialType.HorizontalStriped),
                    $"tile-{suits[color]}-horizontal");
                AssertSprite(repository.GetSprite(color, SpecialType.VerticalStriped),
                    $"tile-{suits[color]}-vertical");
                AssertSprite(repository.GetSprite(color, SpecialType.Wrapped),
                    $"tile-{suits[color]}-wrapped");
            }

            Sprite joker = repository.GetSprite(-1, SpecialType.ColorBomb);
            AssertSprite(joker, "tile-joker");
            Assert.That(repository.GetSprite(0, SpecialType.ColorBomb), Is.SameAs(joker));
            Assert.That(repository.GetSprite(3, SpecialType.ColorBomb), Is.SameAs(joker));
        }

        [Test]
        public void TileVisuals_RenderByColorAndSpecial_AndUpdateExistingObjects()
        {
            GameObject root = new("Board View Test Root", typeof(RectTransform));
            GameObject tileSpotObject = new("Tile Spot", typeof(RectTransform), typeof(Button));
            GameObject tilePrefab = new("Tile", typeof(RectTransform), typeof(Image));
            TileVisualRepository repository = ScriptableObject.CreateInstance<TileVisualRepository>();
            Texture2D texture = new(1, 1);
            List<Sprite> sprites = new();

            try
            {
                Sprite[] baseSprites = CreateSprites(texture, "base", sprites);
                Sprite[] horizontalSprites = CreateSprites(texture, "horizontal", sprites);
                Sprite[] verticalSprites = CreateSprites(texture, "vertical", sprites);
                Sprite[] wrappedSprites = CreateSprites(texture, "wrapped", sprites);
                Sprite jokerSprite = CreateSprite(texture, "joker", sprites);

                ConfigureRepository(
                    repository,
                    tilePrefab,
                    baseSprites,
                    horizontalSprites,
                    verticalSprites,
                    wrappedSprites,
                    jokerSprite);
                BoardView boardView = ConfigureBoardView(root, tileSpotObject, repository);

                Board board = new(6, 1);
                board[0, 0] = CreateTile(0, 0, SpecialType.None);
                board[1, 0] = CreateTile(1, 1, SpecialType.HorizontalStriped);
                board[2, 0] = CreateTile(2, 2, SpecialType.VerticalStriped);
                board[3, 0] = CreateTile(3, 3, SpecialType.Wrapped);
                board[4, 0] = CreateTile(4, -1, SpecialType.ColorBomb);
                board[5, 0] = CreateTile(-1, -1, SpecialType.None);

                boardView.CreateBoard(board);

                AssertTileSprite(root, 0, baseSprites[0]);
                AssertTileSprite(root, 1, horizontalSprites[1]);
                AssertTileSprite(root, 2, verticalSprites[2]);
                AssertTileSprite(root, 3, wrappedSprites[3]);
                AssertTileSprite(root, 4, jokerSprite);

                GameObject createdSpecialTile = GetTileObject(root, 0);
                int createdSpecialInstanceId = createdSpecialTile.GetInstanceID();
                boardView.ApplyCreatedSpecials(new List<SpecialTileInfo>
                {
                    new()
                    {
                        Position = new Vector2Int(0, 0),
                        Color = 0,
                        Special = SpecialType.HorizontalStriped
                    }
                });
                Assert.That(GetTileObject(root, 0).GetInstanceID(), Is.EqualTo(createdSpecialInstanceId));
                AssertTileSprite(root, 0, horizontalSprites[0]);

                GameObject transformedTile = GetTileObject(root, 1);
                int transformedInstanceId = transformedTile.GetInstanceID();
                boardView.ApplySpecialTransformations(new List<SpecialTileInfo>
                {
                    new()
                    {
                        Position = new Vector2Int(1, 0),
                        Color = 1,
                        Special = SpecialType.Wrapped
                    }
                });
                Assert.That(GetTileObject(root, 1).GetInstanceID(), Is.EqualTo(transformedInstanceId));
                AssertTileSprite(root, 1, wrappedSprites[1]);

                for (int color = 0; color < 4; color++)
                {
                    Assert.That(repository.GetSprite(color, SpecialType.None), Is.SameAs(baseSprites[color]));
                    Assert.That(repository.GetSprite(color, SpecialType.HorizontalStriped),
                        Is.SameAs(horizontalSprites[color]));
                    Assert.That(repository.GetSprite(color, SpecialType.VerticalStriped),
                        Is.SameAs(verticalSprites[color]));
                    Assert.That(repository.GetSprite(color, SpecialType.Wrapped),
                        Is.SameAs(wrappedSprites[color]));
                    Assert.That(repository.GetSprite(color, SpecialType.ColorBomb), Is.SameAs(jokerSprite));
                }
            }
            finally
            {
                for (int index = 0; index < sprites.Count; index++)
                {
                    Object.DestroyImmediate(sprites[index]);
                }

                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(tileSpotObject);
                Object.DestroyImmediate(tilePrefab);
                Object.DestroyImmediate(repository);
            }
        }

        private static BoardView ConfigureBoardView(
            GameObject root,
            GameObject tileSpotObject,
            TileVisualRepository repository)
        {
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(600, 100);
            GridLayoutGroup grid = root.AddComponent<GridLayoutGroup>();
            AspectRatioFitter aspectRatioFitter = root.AddComponent<AspectRatioFitter>();
            BoardResponsiveController responsiveController =
                root.AddComponent<BoardResponsiveController>();
            BoardView boardView = root.AddComponent<BoardView>();
            TileSpotView tileSpot = tileSpotObject.AddComponent<TileSpotView>();

            SerializedObject responsiveData = new(responsiveController);
            responsiveData.FindProperty("_boardContainer").objectReferenceValue = rootRect;
            responsiveData.FindProperty("_aspectRatioFitter").objectReferenceValue = aspectRatioFitter;
            responsiveData.FindProperty("_gridLayoutGroup").objectReferenceValue = grid;
            responsiveData.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject boardViewData = new(boardView);
            boardViewData.FindProperty("_boardContainer").objectReferenceValue = grid;
            boardViewData.FindProperty("_responsiveController").objectReferenceValue =
                responsiveController;
            boardViewData.FindProperty("_tileVisualRepository").objectReferenceValue = repository;
            boardViewData.FindProperty("_tileSpotPrefab").objectReferenceValue = tileSpot;
            boardViewData.ApplyModifiedPropertiesWithoutUndo();

            return boardView;
        }

        private static void ConfigureRepository(
            TileVisualRepository repository,
            GameObject tilePrefab,
            Sprite[] baseSprites,
            Sprite[] horizontalSprites,
            Sprite[] verticalSprites,
            Sprite[] wrappedSprites,
            Sprite jokerSprite)
        {
            SerializedObject repositoryData = new(repository);
            repositoryData.FindProperty("_tilePrefab").objectReferenceValue = tilePrefab;
            SetSprites(repositoryData.FindProperty("_baseSprites"), baseSprites);
            SetSprites(repositoryData.FindProperty("_horizontalSprites"), horizontalSprites);
            SetSprites(repositoryData.FindProperty("_verticalSprites"), verticalSprites);
            SetSprites(repositoryData.FindProperty("_wrappedSprites"), wrappedSprites);
            repositoryData.FindProperty("_colorBombSprite").objectReferenceValue = jokerSprite;
            repositoryData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSprites(SerializedProperty property, IReadOnlyList<Sprite> sprites)
        {
            property.arraySize = sprites.Count;
            for (int index = 0; index < sprites.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = sprites[index];
            }
        }

        private static Sprite[] CreateSprites(
            Texture2D texture,
            string prefix,
            ICollection<Sprite> allSprites)
        {
            Sprite[] sprites = new Sprite[4];
            for (int color = 0; color < sprites.Length; color++)
            {
                sprites[color] = CreateSprite(texture, $"{prefix}-{color}", allSprites);
            }

            return sprites;
        }

        private static Sprite CreateSprite(
            Texture2D texture,
            string name,
            ICollection<Sprite> allSprites)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            sprite.name = name;
            allSprites.Add(sprite);
            return sprite;
        }

        private static Tile CreateTile(int id, int color, SpecialType special)
        {
            return new Tile
            {
                Id = id,
                Color = color,
                Special = special
            };
        }

        private static void AssertTileSprite(GameObject root, int x, Sprite expected)
        {
            Assert.That(GetTileObject(root, x).GetComponent<Image>().sprite, Is.SameAs(expected));
        }

        private static void AssertSprite(Sprite sprite, string expectedName)
        {
            Assert.That(sprite, Is.Not.Null, $"Missing sprite {expectedName}.");
            Assert.That(sprite.name, Is.EqualTo(expectedName));

            string assetPath = AssetDatabase.GetAssetPath(sprite);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), assetPath);
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), assetPath);
            Assert.That(importer.alphaIsTransparency, Is.True, assetPath);
        }

        private static GameObject GetTileObject(GameObject root, int x)
        {
            Transform tileSpot = root.transform.GetChild(x);
            Assert.That(tileSpot.childCount, Is.EqualTo(1));
            return tileSpot.GetChild(0).gameObject;
        }
    }
}
