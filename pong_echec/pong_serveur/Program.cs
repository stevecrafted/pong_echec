using System.Net;
using System.Net.Sockets;
using System.Text;
using pong_shared;
using pong_serveur.Models;

namespace PongServeur
{
    class Program
    {
        private static List<ClientInfo> clients = new List<ClientInfo>();
        private static object lockClients = new object();
        private static int prochainJoueurId = 1;
        
        // État de la balle (autorité du serveur)
        private static int ballPosX = 400;
        private static int ballPosY = 300;
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
                
                int joueurId;
                lock (lockClients)
                {
                    // Assigner un ID de joueur (1 ou 2)
                    joueurId = prochainJoueurId;
                    prochainJoueurId++;
                    if (prochainJoueurId > 2) prochainJoueurId = 1; // Limite à 2 joueurs
                    
                    ClientInfo clientInfo = new ClientInfo(client, joueurId);
                    clients.Add(clientInfo);
                    
                    Console.WriteLine($"Nouveau client connecté: {client.Client.RemoteEndPoint} - Joueur {joueurId}");
                }

                // Envoyer l'ID du joueur au client
                var msgAssignation = MessageReseau.CreerAssignerJoueur(joueurId);
                await EnvoyerAUnClient(client, msgAssignation);

                // Gérer le client dans un thread séparé
                _ = Task.Run(() => GererClient(client, joueurId));
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
            string json = message.Serialiser() + "\n";
            byte[] data = Encoding.UTF8.GetBytes(json);

            List<ClientInfo> clientsASupprimer = new List<ClientInfo>();

            lock (lockClients)
            {
                foreach (var clientInfo in clients)
                {
                    try
                    {
                        if (clientInfo.Client.Connected)
                        {
                            clientInfo.Client.GetStream().WriteAsync(data, 0, data.Length);
                        }
                        else
                        {
                            clientsASupprimer.Add(clientInfo);
                        }
                    }
                    catch
                    {
                        clientsASupprimer.Add(clientInfo);
                    }
                }

                // Supprimer les clients déconnectés
                foreach (var clientInfo in clientsASupprimer)
                {
                    clients.Remove(clientInfo);
                    Console.WriteLine($"Joueur {clientInfo.JoueurId} déconnecté");
                }
            }
        }

        // Envoyer un message à un client spécifique
        static async Task EnvoyerAUnClient(TcpClient client, MessageReseau message)
        {
            try
            {
                string json = message.Serialiser() + "\n";
                byte[] data = Encoding.UTF8.GetBytes(json);
                await client.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur envoi: {ex.Message}");
            }
        }

        // Gérer un client individuel
        static async Task GererClient(TcpClient client, int joueurId)
        {
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);

            try
            {
                while (client.Connected)
                {
                    string? ligne = await reader.ReadLineAsync();
                    if (ligne == null) break;

                    var message = MessageReseau.Deserialiser(ligne);
                    if (message != null)
                    {
                        await TraiterMessage(message, joueurId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur client {joueurId}: {ex.Message}");
            }
            finally
            {
                lock (lockClients)
                {
                    clients.RemoveAll(c => c.JoueurId == joueurId);
                }
                client.Close();
                Console.WriteLine($"Joueur {joueurId} déconnecté");
            }
        }

        // Traiter les messages reçus des clients
        static async Task TraiterMessage(MessageReseau message, int joueurId)
        {
            switch (message.Type)
            {
                case TypeMessage.UpdateRaquette:
                    var raquetteData = message.ExtraireDataRaquette();
                    if (raquetteData != null)
                    {
                        // Mettre à jour la position de la raquette du joueur
                        lock (lockClients)
                        {
                            var clientInfo = clients.FirstOrDefault(c => c.JoueurId == joueurId);
                            if (clientInfo != null)
                            {
                                clientInfo.RaquettePosX = raquetteData.PosX;
                                clientInfo.RaquettePosY = raquetteData.PosY;
                                
                                // Console.WriteLine($"Joueur {joueurId} - Raquette: ({raquetteData.PosX:F1}, {raquetteData.PosY:F1})");
                            }
                        }
                        
                        // Redistribuer à tous les clients
                        await EnvoyerATousLesClients(message);
                    }
                    break;
            }
        }
    }
}