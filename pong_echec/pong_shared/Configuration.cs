namespace pong_shared
{
    public class ConfigurationJeu
    {
        public int NombrePieces { get; set; }
        public int TerrainWidth { get; set; }
        public int TerrainHeight { get; set; }
        public int BallStartX { get; set; }
        public int BallStartY { get; set; }
        public float Raquette1X { get; set; }
        public float Raquette1Y { get; set; }
        public float Raquette2X { get; set; }
        public float Raquette2Y { get; set; }
        public int RaquetteWidth { get; set; }

        // Méthode statique pour obtenir la configuration selon le nombre de pièces
        public static ConfigurationJeu ObtenirConfiguration(int nombrePieces)
        {
            int spacing = 110; // Espacement entre les pièces
            int largeurPlateau = nombrePieces * spacing + 20; // Marge de 10 de chaque côté
            int hauteurPlateau = 900; // Hauteur fixe

            return new ConfigurationJeu
            {
                NombrePieces = nombrePieces,
                TerrainWidth = largeurPlateau,
                TerrainHeight = hauteurPlateau,

                // Balle au centre du terrain
                BallStartX = largeurPlateau / 2,
                BallStartY = hauteurPlateau / 2,

                // Raquette 1 à gauche, centrée verticalement
                Raquette1X = 50,
                Raquette1Y = 250, // -5 pour centrer avec hauteur raquette 10

                // Raquette 2 à droite, centrée verticalement
                Raquette2X = 50, // -10 pour la largeur de la raquette
                Raquette2Y = hauteurPlateau - 250,

                // Largeur de raquette adaptée au nombre de pièces
                RaquetteWidth = Math.Min(nombrePieces * 40, 200) // Max 200 pixels
            };
        }
    }
}