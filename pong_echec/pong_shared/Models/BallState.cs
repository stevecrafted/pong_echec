using pong_echec.Game;

namespace pong_shared.Models
{
    public class BallData
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int SpeedX { get; set; }
        public int SpeedY { get; set; }
    }

    // Structure pour les données de la raquette
    public class RaquetteData
    {
        public int JoueurId { get; set; }  // 1 ou 2
        public float PosX { get; set; }
        public float PosY { get; set; }
    }

    public class PieceData
    {
        public int Id { get; set; }
        public int JoueurId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public string TypePiece { get; set; }
        public bool EstVivante { get; set; }
    }

    public class PiecesInitData
    {
        public List<PieceData> Pieces { get; set; }
    }
}
