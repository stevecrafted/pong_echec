namespace pong_shared.Models
{
    public class PieceData
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int JoueurIdMaitre { get; set; }
        public TypePiece Type { get; set; }
        public int Vie { get; set; }
        public int VieMax { get; set; }
        public bool EstVivant { get; set; }
    }
}
