using System;

namespace PongShared.Models
{
    [Serializable]
    public class BallState
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int SpeedX { get; set; }
        public int SpeedY { get; set; }
        public long Timestamp { get; set; }

        public BallState() { }

        public BallState(int posX, int posY, int speedX, int speedY)
        {
            PosX = posX;
            PosY = posY;
            SpeedX = speedX;
            SpeedY = speedY;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}