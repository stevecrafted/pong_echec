using System.Net.Sockets;
using System.Text;
using pong_shared;
using pong_shared.Models;
using System.Collections.Generic;

namespace pong_echec.Reseau
{
    public class ClientReseau
    {
        private bool balleActive = false;
        private TcpClient? client;
        private NetworkStream? stream;
        private StreamReader? reader;
        public bool EstConnecte => client?.Connected ?? false;
        public int MonJoueurId { get; private set; } = -1;

        // Événements
        public event Action<BallData>? OnBallUpdate;
        public event Action<RaquetteData>? OnRaquetteUpdate;
        public event Action<int>? OnJoueurAssigne;
        public event Action<List<PieceData>>? OnPiecesUpdate;
        public event Action<GameState>? OnGameStateUpdate;
        public event Action<bool>? OnBallActiveChange; // Direction am voloany

        public async Task<bool> ConnecterAsync(string adresseServeur, int port)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(adresseServeur, port);
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8);

                Console.WriteLine("Connecté au serveur!");

                // Démarrer l'écoute des messages
                _ = Task.Run(EcouterServeur);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion: {ex.Message}");
                return false;
            }
        }

        private async Task EcouterServeur()
        {
            try
            {
                while (EstConnecte && reader != null)
                {
                    string? ligne = await reader.ReadLineAsync();
                    if (ligne == null) break;

                    // Désérialiser le message
                    var message = MessageReseau.Deserialiser(ligne);
                    if (message != null)
                    {
                        TraiterMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de lecture: {ex.Message}");
            }
        }

        private void TraiterMessage(MessageReseau message)
        {
            switch (message.Type)
            {
                case TypeMessage.UpdateBall:
                    var ballData = message.ExtraireDataBall();

                    // Détecter si la balle bouge
                    bool nouvelleActivite = (ballData.SpeedX != 0 || ballData.SpeedY != 0);
                    if (nouvelleActivite != balleActive)
                    {
                        balleActive = nouvelleActivite;
                        OnBallActiveChange?.Invoke(balleActive); // NOUVEAU
                    }

                    OnBallUpdate?.Invoke(ballData); // AJOUTER cette ligne
                    break;

                case TypeMessage.UpdateRaquette:
                    var raquetteData = message.ExtraireDataRaquette();
                    if (raquetteData != null)
                    {
                        OnRaquetteUpdate?.Invoke(raquetteData);
                    }
                    break;

                case TypeMessage.AssignerJoueurRaquette:
                    MonJoueurId = message.ExtraireJoueurId();
                    Console.WriteLine($"Je suis le joueur {MonJoueurId}");
                    OnJoueurAssigne?.Invoke(MonJoueurId);
                    break;

                case TypeMessage.UpdatePieces:
                    var piecesData = message.ExtraireDataPieces();
                    if (piecesData != null)
                    {
                        OnPiecesUpdate?.Invoke(piecesData);
                    }
                    break;

                case TypeMessage.UpdateGameState:
                    var gameState = message.ExtraireDataGameState();
                    if (gameState != null)
                    {
                        OnGameStateUpdate?.Invoke(gameState);
                    }
                    break;
            }
        }

        public async Task EnvoyerMessageAsync(MessageReseau message)
        {
            if (stream != null && EstConnecte)
            {
                string json = message.Serialiser() + "\n";
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }

        public async Task LancerBalleAsync(int speedX, int speedY)
        {
            var message = MessageReseau.CreerLancerBalle(speedX, speedY);
            await EnvoyerMessageAsync(message);
        }

        public void Deconnecter()
        {
            reader?.Close();
            stream?.Close();
            client?.Close();
        }
    }
}