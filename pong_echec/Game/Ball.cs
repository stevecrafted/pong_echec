namespace pong_echec.Game
{
    public class Ball
    {
        public int PosX { get; private set; }
        public int PosY { get; private set; }

        public int SpeedX { get; set; } = 5;
        public int SpeedY { get; set; } = 5;
        public int Radius { get; set; } = 10;

        public Ball(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        // Nouvelle méthode pour définir la position (depuis le réseau)
        public void SetPosition(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        // Mise à jour position + rebonds (côté serveur uniquement maintenant)
        public void Update(Terrain terrain)
        {
            PosX += SpeedX;
            PosY += SpeedY;

            // rebond horizontal
            if (PosX - Radius <= 0 || PosX + Radius >= terrain.Width)
                SpeedX = -SpeedX;

            // rebond vertical
            if (PosY - Radius <= 0 || PosY + Radius >= terrain.Height)
                SpeedY = -SpeedY;
        }

        public void Draw(Graphics g)
        {
            g.FillEllipse(Brushes.White, PosX - Radius, PosY - Radius, Radius * 2, Radius * 2);
        }
    }
}