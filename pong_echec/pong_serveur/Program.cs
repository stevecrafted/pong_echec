using System.Net;
using System.Net.Sockets;
using System.Text;
using pong_shared;
using pong_serveur.Models;
using pong_echec.Game;

namespace pong_serveur
{
    class Program
    {
        private static GestionnairePieces gestionnairePieces;
        private static List<ClientInfo> clients = new List<ClientInfo>();
        private static object lockClients = new object();
        private static int prochainJoueurId = 1;

        // État de la balle (autorité du serveur)
        private static int ballPosX = 400;
        private static int ballPosY = 300;
        private static int ballSpeedX = 5;
        private static int ballSpeedY = 5;
        private static int ballRadius = 10;

        // Variables pour éviter les collisions multiples
        private static int dernierJoueurTouche = 0;
        private static int framesSansCollision = 0;
        private const int FRAMES_COOLDOWN = 10; // Délai anti-rebond multiple

        private static int TERRAIN_WIDTH = 900;
        private static int TERRAIN_HEIGHT = 900;

        public static void InitialiserJeu(int nbPieces)
        {
            ConfigurationJeu configurationJeu = ConfigurationJeu.ObtenirConfiguration(nbPieces);

            // Initialisation des pieces de chaque joueur
            Terrain terrain = new Terrain(TERRAIN_WIDTH, TERRAIN_HEIGHT);
            gestionnairePieces = new GestionnairePieces(terrain);
            gestionnairePieces.InitialiserPieces(nbPieces);

            // Appliquer la configuration
            TERRAIN_WIDTH = configurationJeu.TerrainWidth;
            TERRAIN_HEIGHT = configurationJeu.TerrainHeight;
            ballPosX = configurationJeu.BallStartX;
            ballPosY = configurationJeu.BallStartY;

            Console.WriteLine($"Jeu initialisé avec {nbPieces} pièces");
            Console.WriteLine($"Terrain: {TERRAIN_WIDTH}x{TERRAIN_HEIGHT}");
            Console.WriteLine($"Balle départ: ({ballPosX}, {ballPosY})");
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Serveur Pong ===");
            Console.WriteLine("Démarrage du serveur...");

            int nombrePiece = 4;
            InitialiserJeu(nombrePiece);

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
                Console.WriteLine("Envoi au client de sont assignation reussi");

                // Gérer le client dans un thread séparé
                _ = Task.Run(() => GererClient(client, joueurId));
            }
        }

        // Boucle de jeu principale
        static async Task GameLoop()
        {
            while (true)
            {
                // Sauvegarder l'ancienne position pour détecter la direction
                int ancienPosX = ballPosX;
                int ancienPosY = ballPosY;

                // Mise à jour de la balle
                ballPosX += ballSpeedX;
                ballPosY += ballSpeedY;

                // Rebonds horizontaux (gauche/droite)
                if (ballPosX - ballRadius <= 0 || ballPosX + ballRadius >= TERRAIN_WIDTH)
                    ballSpeedX = -ballSpeedX;

                // Rebonds verticaux (haut/bas)
                if (ballPosY - ballRadius <= 0 || ballPosY + ballRadius >= TERRAIN_HEIGHT)
                    ballSpeedY = -ballSpeedY;

                // Incrémenter le compteur de cooldown
                framesSansCollision++;

                // Vérifier les collisions avec les raquettes
                VerifierCollisionsRaquettes(ancienPosY);

                // Envoyer la mise à jour à tous les clients
                var message = MessageReseau.CreerUpdateBall(ballPosX, ballPosY, ballSpeedX, ballSpeedY);
                await EnvoyerATousLesClients(message);

                // 60 FPS (~16ms)
                await Task.Delay(16);
            }
        }

        // Vérifier les collisions entre la balle et les raquettes
        static void VerifierCollisionsRaquettes(int anciennePosY)
        {
            // Cooldown : éviter les collisions multiples successives
            if (framesSansCollision < FRAMES_COOLDOWN)
                return;

            lock (lockClients)
            {
                foreach (var clientInfo in clients)
                {
                    // Dimensions de la raquette (correspond à Raquette.cs)
                    const int RAQUETTE_WIDTH = 100;
                    const int RAQUETTE_HEIGHT = 10;

                    float raqX = clientInfo.RaquettePosX;
                    float raqY = clientInfo.RaquettePosY;

                    // Vérifier si la balle touche la raquette
                    if (Utils.Utils.CollisionBallRaquette(ballPosX, ballPosY, ballRadius,
                                              raqX, raqY, RAQUETTE_WIDTH, RAQUETTE_HEIGHT))
                    {
                        // IMPORTANT : Vérifier la DIRECTION de la balle
                        // La balle doit venir de la bonne direction pour rebondir
                        bool balleVientDuHaut = anciennePosY < raqY;
                        bool balleVientDuBas = anciennePosY > raqY + RAQUETTE_HEIGHT;

                        // Ne rebondir QUE si la balle vient du bon côté
                        if ((balleVientDuHaut && ballSpeedY > 0) ||  // Vient du haut, va vers le bas
                            (balleVientDuBas && ballSpeedY < 0))     // Vient du bas, va vers le haut
                        {
                            // Inverser la vitesse verticale (rebond)
                            ballSpeedY = -ballSpeedY;

                            // Repositionner la balle LOIN de la raquette pour éviter qu'elle reste coincée
                            if (balleVientDuHaut)
                            {
                                // La balle venait du haut, la mettre AU-DESSUS de la raquette
                                ballPosY = (int)(raqY - ballRadius - 2);
                            }
                            else
                            {
                                // La balle venait du bas, la mettre EN-DESSOUS de la raquette
                                ballPosY = (int)(raqY + RAQUETTE_HEIGHT + ballRadius + 2);
                            }

                            // Effet de rebond selon où la balle frappe la raquette
                            float positionRelative = (ballPosX - raqX) / RAQUETTE_WIDTH; // 0 à 1
                            float centrage = (positionRelative - 0.5f) * 2; // -1 à 1

                            // Modifier légèrement la vitesse horizontale selon l'endroit du contact
                            ballSpeedX += (int)(centrage * 2);

                            // Limiter la vitesse pour éviter qu'elle devienne trop rapide
                            ballSpeedX = Math.Clamp(ballSpeedX, -10, 10);
                            ballSpeedY = Math.Clamp(ballSpeedY, -10, 10);

                            // Marquer qu'il y a eu une collision (cooldown)
                            dernierJoueurTouche = clientInfo.JoueurId;
                            framesSansCollision = 0;

                            Console.WriteLine($"✓ Collision valide avec raquette du Joueur {clientInfo.JoueurId}!");

                            // Une seule collision à la fois
                            break;
                        }
                        else
                        {
                            // Collision détectée mais depuis le mauvais côté (balle derrière la raquette)
                            Console.WriteLine($"✗ Collision ignorée (mauvaise direction) - Joueur {clientInfo.JoueurId}");
                        }
                    }
                }
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
                        Console.WriteLine("Traitement message en cours");
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

                                Console.WriteLine($"Joueur {joueurId} - Raquette: ({raquetteData.PosX:F1}, {raquetteData.PosY:F1})");
                                clientInfo.RaquettePosX = raquetteData.PosX;
                                clientInfo.RaquettePosY = raquetteData.PosY;

                            }
                        }

                        Console.WriteLine("Envoie a tous les client");
                        // Redistribuer à tous les clients
                        await EnvoyerATousLesClients(message);
                    }
                    break;
            }
        }
    }
}
