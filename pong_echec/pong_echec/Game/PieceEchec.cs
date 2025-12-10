namespace pong_echec.Game
{
    public enum TypePiece
    {
        Pion,
        Tour,
        Cavalier,
        Fou,
        Reine,
        Roi
    }
    
    public class PieceEchec
    {
        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 100;
        public int Vie { get; set; }
        public int VieMax { get ; set; }      // Ilaina anaovana affichage fotsiny le vie max 
        public int JoueurIdMaitre { get; private set; }
        public TypePiece Type { get; set; }
        public Brush CouleurBase { get; set; }
        public bool EstVivant => Vie > 0;

        public PieceEchec(int x, int y, int joueurId, TypePiece type, int vieMax)
        {
            PosX = x;
            PosY = y;
            JoueurIdMaitre = joueurId;
            Type = type;

            VieMax = vieMax;
            Vie = VieMax;

            // Couleur selon le joueur
            CouleurBase = joueurId == 1 ? Brushes.LightBlue : Brushes.LightCoral;
        }

        public void Draw(Graphics g)
        {
            if (!EstVivant)
                return;

            // Fond de la pièce (carré)
            g.FillRectangle(CouleurBase, PosX, PosY, Width, Height);

            // Bordure
            Pen bordure = new Pen(Color.White, 3);
            g.DrawRectangle(bordure, PosX, PosY, Width, Height);

            // Afficher le type de pièce (texte)
            string symbole = ObtenirSymbole();
            Font font = new Font("Arial", 40, FontStyle.Bold);
            SizeF tailleTexte = g.MeasureString(symbole, font);

            float textX = PosX + (Width - tailleTexte.Width) / 2;
            float textY = PosY + (Height - tailleTexte.Height) / 2;

            g.DrawString(symbole, font, Brushes.Black, textX, textY);

            // Barre de vie
            DessinerBarreVie(g);
        }

        private void DessinerBarreVie(Graphics g)
        {
            int barreWidth = Width - 10;
            int barreHeight = 8;
            int barreX = PosX + 5;
            int barreY = PosY + Height - barreHeight - 5;

            // Fond de la barre (rouge)
            g.FillRectangle(Brushes.DarkRed, barreX, barreY, barreWidth, barreHeight);

            // Vie actuelle (vert)
            float pourcentageVie = (float)Vie / VieMax;
            int vieWidth = (int)(barreWidth * pourcentageVie);

            Brush couleurVie = pourcentageVie > 0.6f ? Brushes.Green :
                              pourcentageVie > 0.3f ? Brushes.Orange : Brushes.Red;

            g.FillRectangle(couleurVie, barreX, barreY, vieWidth, barreHeight);

            // Bordure de la barre
            g.DrawRectangle(Pens.Black, barreX, barreY, barreWidth, barreHeight);
        }

        private string ObtenirSymbole()
        {
            return Type switch
            {
                TypePiece.Pion => "♟",
                TypePiece.Tour => "♜",
                TypePiece.Cavalier => "♞",
                TypePiece.Fou => "♝",
                TypePiece.Reine => "♛",
                TypePiece.Roi => "♚",
                _ => "?"
            };
        }
        public void PrendreDegats(int degats)
        {
            Vie -= degats;
            if (Vie < 0) Vie = 0;
        }

        // Vérifier si la balle touche cette pièce
        public bool CollisionAvecBalle(int ballX, int ballY, int ballRadius)
        {
            // Simple AABB collision
            return ballX + ballRadius > PosX &&
                   ballX - ballRadius < PosX + Width &&
                   ballY + ballRadius > PosY &&
                   ballY - ballRadius < PosY + Height;
        }
    }

}