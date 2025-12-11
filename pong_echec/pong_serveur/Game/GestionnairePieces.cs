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

            // ---- Deux modes : Symmetric (chess-like) ou Left-packed (custom) ----
            bool useSymmetricOrder = true; // <-- true = Option B (recommandé), false = Option A

            // Ordre complet (classique échiquier, indices 0..7)
            var fullOrder = new List<(TypePiece type, int vie)>
            {
                (TypePiece.Tour, 5),      // 0
                (TypePiece.Cavalier, 4),  // 1
                (TypePiece.Fou, 3),       // 2
                (TypePiece.Reine, 6),     // 3
                (TypePiece.Roi, 5),       // 4
                (TypePiece.Fou, 3),       // 5
                (TypePiece.Cavalier, 4),  // 6
                (TypePiece.Tour, 5)       // 7
            };

            List<(TypePiece type, int vie)> displayOrder;

            if (useSymmetricOrder)
            {
                // Option B: prendre la "fenêtre centrale" de taille nombrePiece puis compresser à gauche
                // Centre indices : middle window of fullOrder with length nombrePiece
                int total = fullOrder.Count;
                if (nombrePiece >= total)
                {
                    displayOrder = new List<(TypePiece, int)>(fullOrder);
                }
                else
                {
                    // start index to get a centered window of size nombrePiece
                    int start = (total - nombrePiece) / 2;
                    displayOrder = fullOrder.Skip(start).Take(nombrePiece).ToList();
                }
            }
            else
            {
                // Option A: ordre custom left-packed demandé précédemment
                displayOrder = new List<(TypePiece type, int vie)>
                {
                    (TypePiece.Reine, 6),     // 0
                    (TypePiece.Roi, 5),       // 1
                    (TypePiece.Fou, 3),       // 2
                    (TypePiece.Fou, 3),       // 3
                    (TypePiece.Cavalier, 4),  // 4
                    (TypePiece.Cavalier, 4),  // 5
                    (TypePiece.Tour, 5),      // 6
                    (TypePiece.Tour, 5)       // 7
                }.Take(nombrePiece).ToList();
            }

            // Ajouter les pièces principales en colonnes 0..(nombrePiece-1)
            for (int i = 0; i < displayOrder.Count; i++)
            {
                var p = displayOrder[i];
                int col = i; // compression à gauche (col 0..)
                pieces.Add(new PieceEchec(startX + col * spacing, startY, 1, p.type, p.vie));
            }

            // Ajouter les pions alignés sous ces mêmes colonnes
            for (int i = 0; i < displayOrder.Count; i++)
            {
                int col = i;
                pieces.Add(new PieceEchec(startX + col * spacing, pawnY, 1, TypePiece.Pion, 2));
            }
        }

        private void AjouterPiecesJoueur2(int nombrePiece)
        {
            int spacing = 110;
            int startX = 10;
            int startY = terrain.Height - 110;
            int pawnY = startY - 110;

            bool useSymmetricOrder = true; // synchronisé avec joueur1

            var fullOrder = new List<(TypePiece type, int vie)>
    {
        (TypePiece.Tour, 5),
        (TypePiece.Cavalier, 4),
        (TypePiece.Fou, 3),
        (TypePiece.Reine, 6),
        (TypePiece.Roi, 5),
        (TypePiece.Fou, 3),
        (TypePiece.Cavalier, 4),
        (TypePiece.Tour, 5)
    };

            List<(TypePiece type, int vie)> displayOrder;

            if (useSymmetricOrder)
            {
                int total = fullOrder.Count;
                if (nombrePiece >= total)
                {
                    displayOrder = new List<(TypePiece, int)>(fullOrder);
                }
                else
                {
                    int start = (total - nombrePiece) / 2;
                    displayOrder = fullOrder.Skip(start).Take(nombrePiece).ToList();
                }
            }
            else
            {
                displayOrder = new List<(TypePiece type, int vie)>
        {
            (TypePiece.Reine, 6),
            (TypePiece.Roi, 5),
            (TypePiece.Fou, 3),
            (TypePiece.Fou, 3),
            (TypePiece.Cavalier, 4),
            (TypePiece.Cavalier, 4),
            (TypePiece.Tour, 5),
            (TypePiece.Tour, 5)
        }.Take(nombrePiece).ToList();
            }

            for (int i = 0; i < displayOrder.Count; i++)
            {
                var p = displayOrder[i];
                int col = i;
                pieces.Add(new PieceEchec(startX + col * spacing, startY, 2, p.type, p.vie));
            }

            for (int i = 0; i < displayOrder.Count; i++)
            {
                int col = i;
                pieces.Add(new PieceEchec(startX + col * spacing, pawnY, 2, TypePiece.Pion, 2));
            }
        }

    }
}
