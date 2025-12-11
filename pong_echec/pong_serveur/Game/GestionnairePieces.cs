using System.Collections.Generic;
using System.Linq;
using pong_shared.Models;

namespace pong_serveur.Game
{
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

            var pieceSetup = new List<(int col, TypePiece type, int vie)>
            {
                (4, TypePiece.Roi, 5),      // 0 - Centre gauche
                (3, TypePiece.Reine, 6),    // 1 - Centre droit
                (5, TypePiece.Fou, 3),      // 2 - Fou droit
                (2, TypePiece.Fou, 3),      // 3 - Fou gauche
                (6, TypePiece.Cavalier, 4), // 4 - Cavalier droit
                (1, TypePiece.Cavalier, 4), // 5 - Cavalier gauche
                (7, TypePiece.Tour, 5),     // 6 - Tour droite
                (0, TypePiece.Tour, 5)
            };

            for (int i = 0; i < nombrePiece; i++)
            {
                var p = pieceSetup[i];
                pieces.Add(new PieceEchec(startX + p.col * spacing, startY, 1, p.type, p.vie));
            }

            for (int i = 0; i < nombrePiece; i++)
            {
                int col = pieceSetup[i].col;
                pieces.Add(new PieceEchec(startX + col * spacing, pawnY, 1, TypePiece.Pion, 2));
            }
        }

        private void AjouterPiecesJoueur2(int nombrePiece)
        {
            int startX = 10;
            int startY = terrain.Height - 110;
            int pawnY = startY - 110;
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

            for (int i = 0; i < nombrePiece; i++)
            {
                var p = pieceSetup[i];
                pieces.Add(new PieceEchec(startX + p.col * spacing, startY, 2, p.type, p.vie)); // Joueur 2
            }

            for (int i = 0; i < nombrePiece; i++)
            {
                int col = pieceSetup[i].col;
                pieces.Add(new PieceEchec(startX + col * spacing, pawnY, 2, TypePiece.Pion, 2)); // Joueur 2
            }
        }
    }
}
