using System.Drawing;

namespace pong_echec.Game
{
    public class Raquette
    {
        // Position
        public float PosX { get; set; }
        public float PosY { get; set; }

        // Vitesse
        public float VitesseX { get; set; } = 0f;
        public float VitesseY { get; set; } = 0f;

        // Taille de la raquette
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 10;

        // Couleur
        public Brush Couleur { get; set; } = Brushes.White;

        // Constructeur
        public Raquette(float x, float y)
        {
            PosX = x;
            PosY = y;
        }

        // Méthode pour dessiner la raquette
        public void Draw(Graphics g)
        {
            g.FillRectangle(Couleur, PosX, PosY, Width, Height);
        }

        // Méthode pour déplacer la raquette
        public void Move(float deltaX, float deltaY)
        {
            PosX += deltaX * VitesseX;
            PosY += deltaY * VitesseY;
        }
        
        // Optionnel : mise à jour simple avec vitesse
        public void UpdatePosition(Terrain terrain)
        {
            PosX += VitesseX;
            PosY += VitesseY;

            if (PosX >= terrain.Width)
            {
                VitesseX = 0;
            }

            if (PosY >= terrain.Width)
            {
                VitesseY = 0;
            }
        }
    }
}
