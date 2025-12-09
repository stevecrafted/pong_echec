using System.Drawing;

namespace pong_echec.Game
{
    /// <summary>
    /// Gère toutes les pièces d'échecs des joueurs
    /// </summary>
    public class GestionnairePieces
    {
        private List<PieceEchec> pieces;
        private Terrain terrain;

        public IReadOnlyList<PieceEchec> Pieces => pieces.AsReadOnly();

        public GestionnairePieces(Terrain terrain)
        {
            this.terrain = terrain;
            pieces = new List<PieceEchec>();
        }

        /// <summary>
        /// Initialise les pièces de départ pour les deux joueurs
        /// </summary>
        public void InitialiserPieces()
        {
            pieces.Clear();

            // Pièces du Joueur 1 (en bas/gauche)
            AjouterPiecesJoueur1();

            // Pièces du Joueur 2 (en haut/droite)
            AjouterPiecesJoueur2();
        }

        private void AjouterPiecesJoueur1()
        {
            int startY = 10; // En bas de l'écran
            int pawnY = startY + 110;           // Ligne des pions
            int spacing = 110;

            // Ligne de pièces principales
            pieces.Add(new PieceEchec(10, startY, 1, TypePiece.Tour, 5));
            pieces.Add(new PieceEchec(10 + spacing, startY, 1, TypePiece.Cavalier, 4));
            pieces.Add(new PieceEchec(10 + spacing * 2, startY, 1, TypePiece.Fou, 3));
            pieces.Add(new PieceEchec(10 + spacing * 3, startY, 1, TypePiece.Reine, 6));
            pieces.Add(new PieceEchec(10 + spacing * 4, startY, 1, TypePiece.Roi, 5));
            pieces.Add(new PieceEchec(10 + spacing * 5, startY, 1, TypePiece.Fou, 3));
            pieces.Add(new PieceEchec(10 + spacing * 6, startY, 1, TypePiece.Cavalier, 4));
            pieces.Add(new PieceEchec(10 + spacing * 7, startY, 1, TypePiece.Tour, 5));

            // Ligne des pions
            for (int i = 0; i < 8; i++)
            {
                pieces.Add(new PieceEchec(10 + spacing * i, pawnY, 1, TypePiece.Pion, 2));
            }
        }

        private void AjouterPiecesJoueur2()
        {
            int startY = terrain.Height - 110;                    // En haut de l'écran
            int pawnY = startY - 110;            // Ligne des pions
            int spacing = 110; 

            // Ligne de pièces principales
            pieces.Add(new PieceEchec(10, startY, 2, TypePiece.Tour, 5));
            pieces.Add(new PieceEchec(10 + spacing, startY, 2, TypePiece.Cavalier, 4));
            pieces.Add(new PieceEchec(10 + spacing * 2, startY, 2, TypePiece.Fou, 3));
            pieces.Add(new PieceEchec(10 + spacing * 3, startY, 2, TypePiece.Reine, 6));
            pieces.Add(new PieceEchec(10 + spacing * 4, startY, 2, TypePiece.Roi, 5));
            pieces.Add(new PieceEchec(10 + spacing * 5, startY, 2, TypePiece.Fou, 3));
            pieces.Add(new PieceEchec(10 + spacing * 6, startY, 2, TypePiece.Cavalier, 4));
            pieces.Add(new PieceEchec(10 + spacing * 7, startY, 2, TypePiece.Tour, 5));

            // Ligne des pions
            for (int i = 0; i < 8; i++)
            {
                pieces.Add(new PieceEchec(10 + spacing * i, pawnY, 2, TypePiece.Pion, 2));
            }
        }

        /// <summary>
        /// Ajoute une pièce manuellement
        /// </summary>
        public void AjouterPiece(PieceEchec piece)
        {
            pieces.Add(piece);
        }

        /// <summary>
        /// Supprime une pièce
        /// </summary>
        public void SupprimerPiece(PieceEchec piece)
        {
            pieces.Remove(piece);
        }

        /// <summary>
        /// Dessine toutes les pièces vivantes
        /// </summary>
        public void Draw(Graphics g)
        {
            foreach (var piece in pieces)
            {
                if (piece.EstVivant)
                {
                    piece.Draw(g);
                }
            }
        }

        /// <summary>
        /// Vérifie les collisions avec la balle
        /// </summary>
        public PieceEchec? VerifierCollisionBalle(int ballX, int ballY, int ballRadius)
        {
            foreach (var piece in pieces)
            {
                if (piece.EstVivant && piece.CollisionAvecBalle(ballX, ballY, ballRadius))
                {
                    return piece;
                }
            }
            return null;
        }

        /// <summary>
        /// Obtenir toutes les pièces d'un joueur
        /// </summary>
        public List<PieceEchec> ObtenirPiecesJoueur(int joueurId)
        {
            return pieces.Where(p => p.JoueurIdMaitre == joueurId && p.EstVivant).ToList();
        }

        /// <summary>
        /// Compter les pièces vivantes d'un joueur
        /// </summary>
        public int CompterPiecesVivantes(int joueurId)
        {
            return pieces.Count(p => p.JoueurIdMaitre == joueurId && p.EstVivant);
        }

        /// <summary>
        /// Nettoyer les pièces mortes
        /// </summary>
        public void NettoyerPiecesMortes()
        {
            pieces.RemoveAll(p => !p.EstVivant);
        }
    }
}