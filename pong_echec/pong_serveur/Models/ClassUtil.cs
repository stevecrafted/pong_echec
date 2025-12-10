using System.Net;
using System.Net.Sockets;
using System.Text;
using pong_shared;
using pong_serveur.Game; // Changed from pong_shared.Models

namespace pong_serveur.Models
{
    class ClientInfo
    {
        public TcpClient Client { get; set; }
        public int JoueurId { get; set; }
        public float RaquettePosX { get; set; }
        public float RaquettePosY { get; set; }
        public bool IsReady { get; set; } = false; // Added for ready status 

        public ClientInfo(TcpClient client, int joueurId)
        {
            Client = client;
            JoueurId = joueurId;
            RaquettePosX = joueurId == 1 ? 50 : 1500;  // Position initiale
            RaquettePosY = 400;
        }
    }
}
