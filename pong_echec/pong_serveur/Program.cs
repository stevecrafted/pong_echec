using System.Net;
using System.Net.Sockets;
using System.Text;
using pong_shared;
using pong_serveur.Models;
using pong_serveur.Game;
using pong_shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace pong_serveur
{
    class Program
    {
        private static GestionnairePieces gestionnairePieces;
        private static PieceConfig pieceConfig;
        private static bool balleLancee = false;
        private static List<ClientInfo> clients = new List<ClientInfo>();
        private static object lockClients = new object();
        private static int prochainJoueurId = 1;
        private static GameStateType currentGameState = GameStateType.WaitingForPlayers;

        // État de la balle (autorité du serveur)
        private static int ballPosX = 400;
        private static int ballPosY = 300;
        private static int ballSpeedX = 5;
        private static int ballSpeedY = 5;
        private static int ballRadius = 10;

        // Variables pour éviter les collisions multiples
        private static int dernierJoueurTouche = 0;
        private static int framesSansCollision = 0;
        private const int FRAMES_COOLDOWN = 5;
        private static int TERRAIN_WIDTH = 900;
        private static int TERRAIN_HEIGHT = 900;

        public async static Task InitialiserJeu(int nbPieces)
        {
            ConfigurationJeu configurationJeu = ConfigurationJeu.ObtenirConfiguration(nbPieces); 
            pieceConfig = await PieceConfig.CreerAsync();

            Terrain terrain = new Terrain(TERRAIN_WIDTH, TERRAIN_HEIGHT);
            gestionnairePieces = new GestionnairePieces(terrain, pieceConfig);
            gestionnairePieces.InitialiserPieces(nbPieces);

            TERRAIN_WIDTH = configurationJeu.TerrainWidth;
            TERRAIN_HEIGHT = configurationJeu.TerrainHeight;
            ballPosX = configurationJeu.BallStartX;
            ballPosY = configurationJeu.BallStartY;

            balleLancee = false;
            ballSpeedX = 0;
            ballSpeedY = 0;

            Console.WriteLine($"Jeu initialisé avec {nbPieces} pièces");
            Console.WriteLine($"Terrain: {TERRAIN_WIDTH}x{TERRAIN_HEIGHT}");
            Console.WriteLine($"Balle départ: ({ballPosX}, {ballPosY})");
            Console.WriteLine($"Pièces créées: {gestionnairePieces.Pieces.Count}");
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Serveur Pong ===");
            Console.WriteLine("Démarrage du serveur...");

            TcpListener serveur = new TcpListener(IPAddress.Any, 5000);
            serveur.Start();
            Console.WriteLine("Serveur en écoute sur le port 5000");

            _ = Task.Run(GameLoop);

            while (true)
            {
                TcpClient client = await serveur.AcceptTcpClientAsync();

                int joueurId;
                lock (lockClients)
                {
                    joueurId = prochainJoueurId;
                    prochainJoueurId++;
                    if (prochainJoueurId > 2) prochainJoueurId = 1;

                    ClientInfo clientInfo = new ClientInfo(client, joueurId);
                    clients.Add(clientInfo);

                    Console.WriteLine($"Nouveau client connecté: {client.Client.RemoteEndPoint} - Joueur {joueurId}");
                }

                var msgAssignation = MessageReseau.CreerAssignerJoueur(joueurId);
                await EnvoyerAUnClient(client, msgAssignation);
                var msgGameState = MessageReseau.CreerUpdateGameState(currentGameState);
                await EnvoyerAUnClient(client, msgGameState);

                // NOUVEAU : Envoyer l'état initial de la balle
                var msgBallActive = MessageReseau.CreerBallActiveChange(balleLancee);
                await EnvoyerAUnClient(client, msgBallActive);

                Console.WriteLine("Envoi au client de son assignation réussi");

                _ = Task.Run(() => GererClient(client, joueurId));
            }
        }

        static async Task GameLoop()
        {
            while (true)
            {
                if (currentGameState == GameStateType.InProgress && balleLancee)
                {
                    int ancienPosY = ballPosY;
                    int ancienPosX = ballPosX;

                    ballPosX += ballSpeedX;
                    ballPosY += ballSpeedY;

                    if (ballPosX - ballRadius <= 0 || ballPosX + ballRadius >= TERRAIN_WIDTH)
                        ballSpeedX = -ballSpeedX;
                    if (ballPosY - ballRadius <= 0 || ballPosY + ballRadius >= TERRAIN_HEIGHT)
                        ballSpeedY = -ballSpeedY;

                    framesSansCollision++;

                    VerifierCollisionsRaquettes(ancienPosY);

                    await VerifierCollisionPiece(ancienPosY, ancienPosX);
                }

                var messageBalle = MessageReseau.CreerUpdateBall(ballPosX, ballPosY, ballSpeedX, ballSpeedY);
                await EnvoyerATousLesClients(messageBalle);

                // Envoyer les pièces seulement si le gestionnaire est initialisé
                if (gestionnairePieces != null)
                {
                    var piecesData = gestionnairePieces.Pieces.Select(p => new PieceData
                    {
                        PosX = p.PosX,
                        PosY = p.PosY,
                        JoueurIdMaitre = p.JoueurIdMaitre,
                        Type = p.Type,
                        Vie = p.Vie,
                        VieMax = p.VieMax,
                        EstVivant = p.EstVivant
                    }).ToList();
                    var messagePieces = MessageReseau.CreerUpdatePieces(piecesData);
                    await EnvoyerATousLesClients(messagePieces);
                }

                await Task.Delay(16);
            }
        }

        static async Task VerifierCollisionPiece(int anciennePosY, int anciennePosX)
        {
            if (framesSansCollision < FRAMES_COOLDOWN)
                return;

            foreach (var piece in gestionnairePieces.Pieces)
            {
                if (!piece.EstVivant) continue;

                if (piece.CollisionAvecBalle(ballPosX, ballPosY, ballRadius))
                {
                    bool balleVientDuHaut = anciennePosY < piece.PosY;
                    bool balleVientDuBas = anciennePosY > piece.PosY + piece.Height;
                    bool balleVientDuGauche = anciennePosX < piece.PosX;
                    bool balleVientDuDroite = anciennePosX > piece.PosX + piece.Width;

                    bool collisionVerticale =
                        (balleVientDuHaut && ballSpeedY > 0) ||
                        (balleVientDuBas && ballSpeedY < 0);

                    bool collisionHorizontale =
                        (balleVientDuGauche && ballSpeedX > 0) ||
                        (balleVientDuDroite && ballSpeedX < 0);

                    if (collisionVerticale && Math.Abs(ballSpeedY) >= Math.Abs(ballSpeedX))
                    {
                        ballSpeedY = -ballSpeedY;

                        if (balleVientDuHaut)
                            ballPosY = piece.PosY - ballRadius - 2;
                        else
                            ballPosY = piece.PosY + piece.Height + ballRadius + 2;

                        float positionRelative = (ballPosX - piece.PosX) / (float)piece.Width;
                        float centrage = (positionRelative - 0.5f) * 2;
                        ballSpeedX += (int)(centrage * 2);
                    }
                    else if (collisionHorizontale)
                    {
                        ballSpeedX = -ballSpeedX;

                        if (balleVientDuGauche)
                            ballPosX = piece.PosX - ballRadius - 2;
                        else
                            ballPosX = piece.PosX + piece.Width + ballRadius + 2;
                    }

                    ballSpeedX = Math.Clamp(ballSpeedX, -10, 10);
                    ballSpeedY = Math.Clamp(ballSpeedY, -10, 10);

                    piece.PrendreDegats(1);

                    framesSansCollision = 0;
                    Console.WriteLine($"Pièce touchée! Vie restante: {piece.Vie}");

                    if (!piece.EstVivant && piece.Type == TypePiece.Roi)
                    {
                        Console.WriteLine($"Le Roi du joueur {piece.JoueurIdMaitre} est mort! Game Over.");
                        await SetGameState(GameStateType.GameOver);
                    }
                    break;
                }
            }
        }

        static void VerifierCollisionsRaquettes(int anciennePosY)
        {
            if (framesSansCollision < FRAMES_COOLDOWN)
                return;

            lock (lockClients)
            {
                foreach (var clientInfo in clients)
                {
                    const int RAQUETTE_WIDTH = 100;
                    const int RAQUETTE_HEIGHT = 10;

                    float raqX = clientInfo.RaquettePosX;
                    float raqY = clientInfo.RaquettePosY;

                    if (Utils.Utils.CollisionBallRaquette(ballPosX, ballPosY, ballRadius,
                                              raqX, raqY, RAQUETTE_WIDTH, RAQUETTE_HEIGHT))
                    {
                        bool balleVientDuHaut = anciennePosY < raqY;
                        bool balleVientDuBas = anciennePosY > raqY + RAQUETTE_HEIGHT;

                        if ((balleVientDuHaut && ballSpeedY > 0) || (balleVientDuBas && ballSpeedY < 0))
                        {
                            ballSpeedY = -ballSpeedY;

                            if (balleVientDuHaut)
                            {
                                ballPosY = (int)(raqY - ballRadius - 2);
                            }
                            else
                            {
                                ballPosY = (int)(raqY + RAQUETTE_HEIGHT + ballRadius + 2);
                            }

                            float positionRelative = (ballPosX - raqX) / RAQUETTE_WIDTH;
                            float centrage = (positionRelative - 0.5f) * 2;
                            ballSpeedX += (int)(centrage * 2);
                            ballSpeedX = Math.Clamp(ballSpeedX, -10, 10);
                            ballSpeedY = Math.Clamp(ballSpeedY, -10, 10);

                            dernierJoueurTouche = clientInfo.JoueurId;
                            framesSansCollision = 0;

                            Console.WriteLine($"✓ Collision valide avec raquette du Joueur {clientInfo.JoueurId}!");
                            break;
                        }
                    }
                }
            }
        }

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

                foreach (var clientInfo in clientsASupprimer)
                {
                    clients.Remove(clientInfo);
                    Console.WriteLine($"Joueur {clientInfo.JoueurId} déconnecté");
                }
            }
        }

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

        static async Task TraiterMessage(MessageReseau message, int joueurId)
        {
            switch (message.Type)
            {
                case TypeMessage.LancerBalle:
                    if (!balleLancee && joueurId == 1)
                    {
                        var direction = message.ExtraireDirectionBalle();
                        ballSpeedX = direction.SpeedX;
                        ballSpeedY = direction.SpeedY;
                        balleLancee = true;

                        // NOUVEAU : Notifier tous les clients que la balle est active
                        var msgBallActive = MessageReseau.CreerBallActiveChange(true);
                        await EnvoyerATousLesClients(msgBallActive);

                        Console.WriteLine($"Joueur 1 lance la balle: ({ballSpeedX}, {ballSpeedY})");
                    }
                    break;

                case TypeMessage.ConfigurationPartie:
                    int nombrePieces = message.ExtraireNombrePieces();

                    if (gestionnairePieces == null)
                    {
                        Console.WriteLine($"📋 Configuration reçue: {nombrePieces} pièces");
                        await InitialiserJeu(nombrePieces);

                        Console.WriteLine("✅ Jeu initialisé et prêt!");
                        
                        // Envoyer immédiatement l'état des pièces aux clients connectés
                        if (gestionnairePieces != null)
                        {
                            var piecesData = gestionnairePieces.Pieces.Select(p => new PieceData
                            {
                                PosX = p.PosX,
                                PosY = p.PosY,
                                JoueurIdMaitre = p.JoueurIdMaitre,
                                Type = p.Type,
                                Vie = p.Vie,
                                VieMax = p.VieMax,
                                EstVivant = p.EstVivant
                            }).ToList();
                            var messagePieces = MessageReseau.CreerUpdatePieces(piecesData);
                            await EnvoyerATousLesClients(messagePieces);
                            Console.WriteLine($"📤 État des pièces envoyé: {piecesData.Count} pièces");
                        }
                    }
                    break;

                case TypeMessage.UpdateRaquette:
                    var raquetteData = message.ExtraireDataRaquette();
                    if (raquetteData != null)
                    {
                        lock (lockClients)
                        {
                            var clientInfo = clients.FirstOrDefault(c => c.JoueurId == joueurId);
                            if (clientInfo != null)
                            {
                                clientInfo.RaquettePosX = raquetteData.PosX;
                                clientInfo.RaquettePosY = raquetteData.PosY;
                            }
                        }
                        await EnvoyerATousLesClients(message);
                    }
                    break;

                case TypeMessage.PlayerReady:
                    bool allReady = false;
                    lock (lockClients)
                    {
                        var clientInfo = clients.FirstOrDefault(c => c.JoueurId == joueurId);
                        if (clientInfo != null)
                        {
                            clientInfo.IsReady = true;
                            Console.WriteLine($"Joueur {joueurId} est prêt!");
                        }

                        if (clients.Count == 2 && clients.All(c => c.IsReady))
                        {
                            allReady = true;
                        }
                    }

                    if (allReady)
                    {
                        Console.WriteLine("Tous les joueurs sont prêts! La partie commence.");
                        await SetGameState(GameStateType.InProgress);

                        // NOUVEAU : Réinitialiser l'état de la balle pour la nouvelle partie
                        balleLancee = false;
                        var msgBallActive = MessageReseau.CreerBallActiveChange(false);
                        await EnvoyerATousLesClients(msgBallActive);
                    }
                    break;
            }
        }

        static async Task SetGameState(GameStateType newState)
        {
            currentGameState = newState;
            var gameStateMessage = MessageReseau.CreerUpdateGameState(currentGameState);
            await EnvoyerATousLesClients(gameStateMessage);
        }
    }
}