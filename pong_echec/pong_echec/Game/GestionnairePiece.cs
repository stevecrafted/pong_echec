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
        public void InitialiserPieces(int nombrePiece)
        {
            pieces.Clear();
            AjouterPiecesJoueur1(nombrePiece);
            AjouterPiecesJoueur2(nombrePiece);
        }

        private void AjouterPiecesJoueur1(int nombrePiece)
        {
            int spacing = 110;
            int startX = 10;
            int startY = 10;
            int pawnY = startY + 110;

            // Liste des pièces dans l'ordre d'un vrai échiquier
            var pieceSetup = new List<(int col, TypePiece type, int vie)>
            {
                (0, TypePiece.Tour, 5),
                (1, TypePiece.Cavalier, 4),
                (2, TypePiece.Fou, 3),
                (3, TypePiece.Reine, 6),
                (4, TypePiece.Roi, 5),
                (5, TypePiece.Fou, 3),
                (6, TypePiece.Cavalier, 4),
                (7, TypePiece.Tour, 5)
            };

            // Ajouter les pièces principales selon nombrePiece
            for (int i = 0; i < nombrePiece; i++)
            {
                var p = pieceSetup[i];
                pieces.Add(new PieceEchec(
                    startX + p.col * spacing,
                    startY,
                    1,
                    p.type,
                    p.vie
                ));
            }

            // Ajouter les pions en face des pièces sélectionnées
            for (int i = 0; i < nombrePiece; i++)
            {
                int col = pieceSetup[i].col;
                pieces.Add(new PieceEchec(
                    startX + col * spacing,
                    pawnY,
                    1,
                    TypePiece.Pion,
                    2
                ));
            }
        }

        private void AjouterPiecesJoueur2(int nombrePiece)
        {
            int startX = 10;
            int startY = terrain.Height - 110;                    // En haut de l'écran
            int pawnY = startY - 110;            // Ligne des pions
            int spacing = 110;

            var pieceSetup = new List<(int col, TypePiece type, int vie)>
            {
                (0, TypePiece.Tour, 5),
                (1, TypePiece.Cavalier, 4),
                (2, TypePiece.Fou, 3),
                (3, TypePiece.Reine, 6),
                (4, TypePiece.Roi, 5),
                (5, TypePiece.Fou, 3),
                (6, TypePiece.Cavalier, 4),
                (7, TypePiece.Tour, 5)
            };

            // Ajouter les pièces principales selon nombrePiece
            for (int i = 0; i < nombrePiece; i++)
            {
                var p = pieceSetup[i];
                pieces.Add(new PieceEchec(
                    startX + p.col * spacing,
                    startY,
                    1,
                    p.type,
                    p.vie
                ));
            }

            // Ajouter les pions en face des pièces sélectionnées
            for (int i = 0; i < nombrePiece; i++)
            {
                int col = pieceSetup[i].col;
                pieces.Add(new PieceEchec(
                    startX + col * spacing,
                    pawnY,
                    1,
                    TypePiece.Pion,
                    2
                ));
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