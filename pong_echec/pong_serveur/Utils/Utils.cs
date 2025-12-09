namespace pong_serveur.Utils
{
    class Utils
    {
        public static bool CollisionBallRaquette(int ballX, int ballY, int ballRadius,
                                      float raqX, float raqY, int raqWidth, int raqHeight)
        {
            // Trouver le point le plus proche de la balle sur le rectangle de la raquette
            float closestX = Math.Clamp(ballX, raqX, raqX + raqWidth);
            float closestY = Math.Clamp(ballY, raqY, raqY + raqHeight);

            // Calculer la distance entre le centre de la balle et ce point
            float distanceX = ballX - closestX;
            float distanceY = ballY - closestY;
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

            // Collision si la distance est inférieure au rayon
            return distanceSquared < (ballRadius * ballRadius);
        }
    }
}