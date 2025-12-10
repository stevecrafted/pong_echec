using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using pong_shared.Models;

namespace pong_echec.Game
{
    /// <summary>
    /// Gère toutes les pièces d'échecs des joueurs côté client
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
        /// Met à jour la liste des pièces à partir des données du serveur
        /// </summary>
        public void UpdatePieces(List<PieceData> piecesData)
        {
            pieces.Clear();
            foreach (var pieceData in piecesData)
            {
                if (pieceData.EstVivant)
                {
                    var newPiece = new PieceEchec(
                        pieceData.PosX,
                        pieceData.PosY,
                        pieceData.JoueurIdMaitre,
                        pieceData.Type,
                        pieceData.VieMax
                    );
                    newPiece.Vie = pieceData.Vie;
                    pieces.Add(newPiece);
                }
            }
        }

        /// <summary>
        /// Dessine toutes les pièces vivantes
        /// </summary>
        public void Draw(Graphics g)
        {
            foreach (var piece in pieces)
            {
                piece.Draw(g);
            }
        }

        /// <summary>
        /// Compter les pièces vivantes d'un joueur
        /// </summary>
        public int CompterPiecesVivantes(int joueurId)
        {
            return pieces.Count(p => p.JoueurIdMaitre == joueurId);
        }
    }
}