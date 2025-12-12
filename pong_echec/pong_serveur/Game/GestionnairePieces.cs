using System;
using System.Collections.Generic;
using pong_shared.Models;

namespace pong_serveur.Game
{
    public class GestionnairePieces
    {
        private List<PieceEchec> pieces;
        private Terrain terrain;
        private PieceConfig config; 

        public IReadOnlyList<PieceEchec> Pieces => pieces.AsReadOnly();

        public GestionnairePieces(Terrain terrain, PieceConfig config)
        {
            this.terrain = terrain ?? throw new ArgumentNullException(nameof(terrain));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            pieces = new List<PieceEchec>();
        }

        public void InitialiserPieces(int nombrePiece)
        {
            if (nombrePiece <= 0 || nombrePiece > 8)
                throw new ArgumentException("Le nombre de pièces doit être entre 1 et 8.");

            if (config.VieParType == null || config.VieParType.Count == 0)
                throw new InvalidOperationException("La configuration des pièces n'est pas chargée.");

            pieces.Clear();

            AjouterPiecesPourJoueur(1, nombrePiece, 10, 10);
            AjouterPiecesPourJoueur(2, nombrePiece, 10, terrain.Height - 110);

            Console.WriteLine($"✓ {pieces.Count} pièces initialisées ({nombrePiece} par joueur)");
        }

        private void AjouterPiecesPourJoueur(int joueur, int nombrePiece, int startX, int startY)
        {
            int spacing = 110;
            int pawnY = (joueur == 1) ? startY + 110 : startY - 110;

            // ➜ On prend juste l’ordre classique des échecs
            var ordre = PieceConfig.OrdreEchecs;

            // ➜ On ne prend que nombrePiece premiers
            var liste = ordre.GetRange(0, nombrePiece);

            // --- Pièces principales ---
            for (int i = 0; i < liste.Count; i++)
            {
                TypePiece type = liste[i];
                int vie = config.ObtenirVie(type);

                pieces.Add(new PieceEchec(
                    startX + i * spacing,
                    startY,
                    joueur,
                    type,
                    vie
                ));
            }

            // --- Pions associés ---
            int viePion = config.ObtenirVie(TypePiece.Pion);

            for (int i = 0; i < liste.Count; i++)
            {
                pieces.Add(new PieceEchec(
                    startX + i * spacing,
                    pawnY,
                    joueur,
                    TypePiece.Pion,
                    viePion
                ));
            }
        }
    }
}
