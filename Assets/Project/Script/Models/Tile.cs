namespace Gazeus.DesafioMatch3.Models
{
    public class Tile
    {
        public int Id { get; set; }
        public int Color { get; set; }
        public SpecialType Special { get; set; }

        public bool IsEmpty =>
            Id < 0 &&
            Color < 0 &&
            Special == SpecialType.None;
    }
}
