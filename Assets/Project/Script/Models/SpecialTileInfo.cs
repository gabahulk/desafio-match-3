using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public struct SpecialTileInfo
    {
        public Vector2Int Position { get; set; }
        public int Color { get; set; }
        public SpecialType Special { get; set; }
    }
}
