using System.Net;
using System.Net.Sockets;
using System.Text;

namespace pong_echec.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Serveur en écoute sur le port 5000...");
            
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Client connecté !");
                
                // Chaque client géré dans un thread
                new Thread(() => HandleClient(client)).Start();
            }
        }

        static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[1024];
                int len = stream.Read(buffer, 0, buffer.Length);

                string msg = Encoding.UTF8.GetString(buffer, 0, len);
                Console.WriteLine("Reçu du client: " + msg);

                // renvoyer une réponse
                string response = "pong du serveur";
                byte[] data = Encoding.UTF8.GetBytes(response);
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
