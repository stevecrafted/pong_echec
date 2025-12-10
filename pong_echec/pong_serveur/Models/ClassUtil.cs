using System.Net;
using System.Net.Sockets;
using System.Text;
using pong_shared;
using pong_echec.Game;

namespace pong_serveur.Models
{
    class ClientInfo
    {
        public TcpClient Client { get; set; }
        public int JoueurId { get; set; }
        public float RaquettePosX { get; set; }
        public float RaquettePosY { get; set; }
        private static List<PieceEchec> PieceEchecs = new List<PieceEchec>();

        public ClientInfo(TcpClient client, int joueurId)
        {
            Client = client;
            JoueurId = joueurId;
            RaquettePosX = joueurId == 1 ? 50 : 1500;  // Position initiale
            RaquettePosY = 400;
        }
    }

}
