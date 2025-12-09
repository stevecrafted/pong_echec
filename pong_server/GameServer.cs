using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PongShared.Messages;
using PongShared.Models;

namespace PongServer
{
    public class GameServer
    {
        private TcpListener _listener;
        private List<TcpClient> _clients = new List<TcpClient>();
        private BallState _ballState;
        private System.Timers.Timer _gameTimer;
        private int _terrainWidth = 1600;
        private int _terrainHeight = 900;
        private int _ballRadius = 10;
        private object _lock = new object();

        public GameServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            
            // Initialiser la balle au centre
            _ballState = new BallState(
                _terrainWidth / 2, 
                _terrainHeight / 2, 
                5, 
                5
            );
        }

        public async Task Start()
        {
            _listener.Start();
            Console.WriteLine($"Serveur démarré sur le port {((IPEndPoint)_listener.LocalEndpoint).Port}");
            Console.WriteLine($"Adresse IP locale: {GetLocalIPAddress()}");

            // Démarrer la boucle de jeu
            StartGameLoop();

            // Accepter les connexions
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine($"Client connecté: {client.Client.RemoteEndPoint}");
                
                lock (_lock)
                {
                    _clients.Add(client);
                }

                // Gérer ce client dans un thread séparé
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private void StartGameLoop()
        {
            _gameTimer = new System.Timers.Timer(16); // ~60 FPS
            _gameTimer.Elapsed += (s, e) => UpdateGame();
            _gameTimer.Start();
        }

        private void UpdateGame()
        {
            lock (_lock)
            {
                // Mettre à jour la position de la balle
                _ballState.PosX += _ballState.SpeedX;
                _ballState.PosY += _ballState.SpeedY;

                // Rebonds horizontaux
                if (_ballState.PosX - _ballRadius <= 0 || 
                    _ballState.PosX + _ballRadius >= _terrainWidth)
                {
                    _ballState.SpeedX = -_ballState.SpeedX;
                }

                // Rebonds verticaux
                if (_ballState.PosY - _ballRadius <= 0 || 
                    _ballState.PosY + _ballRadius >= _terrainHeight)
                {
                    _ballState.SpeedY = -_ballState.SpeedY;
                }

                _ballState.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Envoyer l'état à tous les clients
                BroadcastBallState();
            }
        }

        private void BroadcastBallState()
        {
            var message = new BallStateMessage
            {
                PosX = _ballState.PosX,
                PosY = _ballState.PosY,
                SpeedX = _ballState.SpeedX,
                SpeedY = _ballState.SpeedY,
                Timestamp = _ballState.Timestamp
            };

            string json = message.Serialize() + "\n";
            byte[] data = Encoding.UTF8.GetBytes(json);

            List<TcpClient> disconnectedClients = new List<TcpClient>();

            foreach (var client in _clients)
            {
                try
                {
                    if (client.Connected)
                    {
                        client.GetStream().Write(data, 0, data.Length);
                    }
                    else
                    {
                        disconnectedClients.Add(client);
                    }
                }
                catch
                {
                    disconnectedClients.Add(client);
                }
            }

            // Nettoyer les clients déconnectés
            foreach (var client in disconnectedClients)
            {
                _clients.Remove(client);
                client.Close();
                Console.WriteLine("Client déconnecté");
            }
        }

        private async Task HandleClient(TcpClient client)
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
                    Console.WriteLine($"Message reçu: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
            finally
            {
                lock (_lock)
                {
                    _clients.Remove(client);
                }
                client.Close();
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
    }
}