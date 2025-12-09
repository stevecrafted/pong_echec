using System.Net.Sockets;
using System.Text;
using pong_shared;
using pong_shared.Models;

namespace pong_echec.Reseau
{
    public class ClientReseau
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private StreamReader? reader;
        public bool EstConnecte => client?.Connected ?? false;
        
        // Événement déclenché quand la balle est mise à jour
        public event Action<BallData>? OnBallUpdate;
        
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
                    if (ballData != null)
                    {
                        OnBallUpdate?.Invoke(ballData);
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

        public void Deconnecter()
        {
            reader?.Close();
            stream?.Close();
            client?.Close();
        }
    }
}