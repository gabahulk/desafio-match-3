using System;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TileVisualRepository", menuName = "Gameplay/TileVisualRepository")]
    public sealed class TileVisualRepository : ScriptableObject
    {
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private Sprite[] _baseSprites;
        [SerializeField] private Sprite[] _horizontalSprites;
        [SerializeField] private Sprite[] _verticalSprites;
        [SerializeField] private Sprite[] _wrappedSprites;
        [SerializeField] private Sprite _colorBombSprite;

        public GameObject TilePrefab => _tilePrefab;

        public Sprite GetSprite(int color, SpecialType special)
        {
            if (special == SpecialType.ColorBomb)
            {
                return _colorBombSprite;
            }

            Sprite[] sprites = special switch
            {
                SpecialType.None => _baseSprites,
                SpecialType.HorizontalStriped => _horizontalSprites,
                SpecialType.VerticalStriped => _verticalSprites,
                SpecialType.Wrapped => _wrappedSprites,
                _ => throw new ArgumentOutOfRangeException(nameof(special), special, null)
            };

            if (color < 0 || color >= sprites.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(color));
            }

            return sprites[color];
        }
    }
}
