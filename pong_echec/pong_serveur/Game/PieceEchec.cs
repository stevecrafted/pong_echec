
using pong_shared.Models;

namespace pong_serveur.Game
{
    public class PieceEchec
    {
        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public int Vie { get; set; }
        public int VieMax { get; set; }
        public int JoueurIdMaitre { get; private set; }
        public TypePiece Type { get; set; }
        public bool EstVivant => Vie > 0;

        public PieceEchec(int x, int y, int joueurId, TypePiece type, int vieMax)
        {
            PosX = x;
            PosY = y;
            JoueurIdMaitre = joueurId;
            Type = type;

            VieMax = vieMax;
            Vie = VieMax;
        }

        public void PrendreDegats(int degats)
        {
            Vie -= degats;
            if (Vie < 0) Vie = 0;
        }

        public bool CollisionAvecBalle(int ballX, int ballY, int ballRadius)
        {
            return ballX + ballRadius > PosX &&
                   ballX - ballRadius < PosX + Width &&
                   ballY + ballRadius > PosY &&
                   ballY - ballRadius < PosY + Height;
        }
    }
}
