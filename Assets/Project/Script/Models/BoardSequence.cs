using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Models
{
    public class BoardSequence
    {
        public List<MovedTileInfo> MovedTiles { get; set; }
        public List<AddedTileInfo> AddedTiles { get; set; }
        public List<Vector2Int> MatchedPosition { get; set; }
        public List<SpecialTileInfo> CreatedSpecialTiles { get; set; }
        public List<SpecialActivationInfo> SpecialActivations { get; set; }
        public int ScoreGained { get; set; }
        public int TotalScore { get; set; }
    }
}
