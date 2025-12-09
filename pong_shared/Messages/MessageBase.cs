using System;
using System.Text.Json;

namespace PongShared.Messages
{
    [Serializable]
    public abstract class MessageBase
    {
        public string MessageType { get; set; }
        
        protected MessageBase(string messageType)
        {
            MessageType = messageType;
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this, this.GetType());
        }

        public static T Deserialize<T>(string json) where T : MessageBase
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }

    [Serializable]
    public class BallStateMessage : MessageBase
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int SpeedX { get; set; }
        public int SpeedY { get; set; }
        public long Timestamp { get; set; }

        public BallStateMessage() : base("BallState") { }
    }

    [Serializable]
    public class ConnectionMessage : MessageBase
    {
        public string ClientId { get; set; }
        
        public ConnectionMessage() : base("Connection") { }
    }
}