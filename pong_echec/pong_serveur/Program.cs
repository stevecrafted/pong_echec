using System.Net;
using System.Net.Sockets;
using System.Text;
using pong_shared;

namespace PongServeur
{
    class Program
    {
        private static List<TcpClient> clients = new List<TcpClient>();
        private static object lockClients = new object();
        
        // État de la balle (autorité du serveur)
        private static int ballPosX = 200;
        private static int ballPosY = 150;
        private static int ballSpeedX = 5;
        private static int ballSpeedY = 5;
        private static int ballRadius = 10;
        
        private const int TERRAIN_WIDTH = 1600;
        private const int TERRAIN_HEIGHT = 900;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Serveur Pong ===");
            Console.WriteLine("Démarrage du serveur...");

            // Démarrer le serveur TCP
            TcpListener serveur = new TcpListener(IPAddress.Any, 5000);
            serveur.Start();
            Console.WriteLine("Serveur en écoute sur le port 5000");

            // Démarrer la boucle de jeu
            _ = Task.Run(GameLoop);

            // Accepter les connexions
            while (true)
            {
                TcpClient client = await serveur.AcceptTcpClientAsync();
                Console.WriteLine($"Nouveau client connecté: {client.Client.RemoteEndPoint}");
                
                lock (lockClients)
                {
                    clients.Add(client);
                }

                // Gérer le client dans un thread séparé
                _ = Task.Run(() => GererClient(client));
            }
        }

        // Boucle de jeu principale
        static async Task GameLoop()
        {
            while (true)
            {
                // Mise à jour de la balle
                ballPosX += ballSpeedX;
                ballPosY += ballSpeedY;

                // Rebonds horizontaux
                if (ballPosX - ballRadius <= 0 || ballPosX + ballRadius >= TERRAIN_WIDTH)
                    ballSpeedX = -ballSpeedX;

                // Rebonds verticaux
                if (ballPosY - ballRadius <= 0 || ballPosY + ballRadius >= TERRAIN_HEIGHT)
                    ballSpeedY = -ballSpeedY;

                // Envoyer la mise à jour à tous les clients
                var message = MessageReseau.CreerUpdateBall(ballPosX, ballPosY, ballSpeedX, ballSpeedY);
                await EnvoyerATousLesClients(message);

                // 60 FPS (~16ms)
                await Task.Delay(16);
            }
        }

        // Envoyer un message à tous les clients connectés
        static async Task EnvoyerATousLesClients(MessageReseau message)
        {
            string json = message.Serialiser() + "\n"; // Délimiteur de ligne
            byte[] data = Encoding.UTF8.GetBytes(json);

            List<TcpClient> clientsASupprimer = new List<TcpClient>();

            lock (lockClients)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        if (client.Connected)
                        {
                            client.GetStream().WriteAsync(data, 0, data.Length);
                        }
                        else
                        {
                            clientsASupprimer.Add(client);
                        }
                    }
                    catch
                    {
                        clientsASupprimer.Add(client);
                    }
                }

                // Supprimer les clients déconnectés
                foreach (var client in clientsASupprimer)
                {
                    clients.Remove(client);
                    Console.WriteLine("Client déconnecté");
                }
            }
        }

        // Gérer un client individuel
        static async Task GererClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Reçu: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur client: {ex.Message}");
            }
            finally
            {
                lock (lockClients)
                {
                    clients.Remove(client);
                }
                client.Close();
            }
        }
    }
}