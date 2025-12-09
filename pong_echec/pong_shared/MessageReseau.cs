using System.Text.Json;
using pong_shared.Models;

namespace pong_shared
{
    // Types de messages
    public enum TypeMessage
    {
        UpdateBall,      // Mise à jour position balle
        ConnexionClient,
        DeconnexionClient
    }

    // Classe de base pour les messages
    public class MessageReseau
    {
        public TypeMessage Type { get; set; }
        public string Data { get; set; } = string.Empty;

        public static MessageReseau CreerUpdateBall(int posX, int posY, int speedX, int speedY)
        {
            var ballData = new BallData 
            { 
                PosX = posX, 
                PosY = posY,
                SpeedX = speedX,
                SpeedY = speedY
            };
            
            return new MessageReseau
            {
                Type = TypeMessage.UpdateBall,
                Data = JsonSerializer.Serialize(ballData)
            };
        }

        public BallData? ExtraireDataBall()
        {
            if (Type == TypeMessage.UpdateBall)
            {
                return JsonSerializer.Deserialize<BallData>(Data);
            }
            return null;
        }

        public string Serialiser()
        {
            return JsonSerializer.Serialize(this);
        }

        public static MessageReseau? Deserialiser(string json)
        {
            return JsonSerializer.Deserialize<MessageReseau>(json);
        }
    }

    // Structure pour les données de la balle

}