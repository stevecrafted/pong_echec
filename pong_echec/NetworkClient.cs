using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using pong_shared.Messages;

namespace PongClient
{
    public class NetworkClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected;

        public event Action<BallStateMessage> OnBallStateReceived;
        public event Action OnDisconnected;

        public bool IsConnected => _isConnected && _client?.Connected == true;

        public async Task<bool> Connect(string serverIP, int port)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(serverIP, port);
                _stream = _client.GetStream();
                _isConnected = true;

                Console.WriteLine($"Connecté au serveur {serverIP}:{port}");

                // Démarrer la réception des messages
                _ = Task.Run(() => ReceiveMessages());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        private async Task ReceiveMessages()
        {
            byte[] buffer = new byte[4096];
            StringBuilder messageBuilder = new StringBuilder();

            try
            {
                while (_isConnected && _client.Connected)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        // Connexion fermée
                        break;
                    }

                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(data);

                    // Traiter les messages complets (délimités par \n)
                    string allMessages = messageBuilder.ToString();
                    string[] messages = allMessages.Split('\n');

                    // Traiter tous les messages complets
                    for (int i = 0; i < messages.Length - 1; i++)
                    {
                        ProcessMessage(messages[i]);
                    }

                    // Garder le dernier message incomplet
                    messageBuilder.Clear();
                    messageBuilder.Append(messages[messages.Length - 1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur réception: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        private void ProcessMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                var ballState = MessageBase.Deserialize<BallStateMessage>(message);
                OnBallStateReceived?.Invoke(ballState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur traitement message: {ex.Message}");
            }
        }

        public void SendMessage(MessageBase message)
        {
            if (!IsConnected)
                return;

            try
            {
                string json = message.Serialize() + "\n";
                byte[] data = Encoding.UTF8.GetBytes(json);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur envoi: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
            
            _stream?.Close();
            _client?.Close();

            OnDisconnected?.Invoke();
            Console.WriteLine("Déconnecté du serveur");
        }
    }
}