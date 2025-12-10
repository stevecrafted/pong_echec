using pong_echec.Game;
using pong_echec.Input;
using pong_echec.Reseau;
using pong_shared;
using pong_shared.Models;

namespace pong_echec.UI
{
    public partial class MainForm : Form
    {
        ConfigurationJeu configurationJeu;
        Terrain terrain;
        Ball ball;
        System.Windows.Forms.Timer gameLoop;

        // Raquettes
        Raquette raquetteJoueur1;  // Toujours à gauche
        Raquette raquetteJoueur2;  // Toujours à droite

        // Pièces d'échecs
        GestionnairePieces gestionnairePieces;

        // Client réseau
        ClientReseau clientReseau;
        bool modeReseau = false;
        int monJoueurId = -1;

        // Pour limiter l'envoi réseau
        private DateTime dernierEnvoi = DateTime.Now;
        private const int INTERVALLE_ENVOI_MS = 16; // ~60 envois/seconde

        public MainForm()
        {
            Console.WriteLine("Main form anh");
            int nombrePiece = 8;
            configurationJeu = ConfigurationJeu.ObtenirConfiguration(nombrePiece);

            InitializeComponent(configurationJeu);
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            Console.WriteLine("Initialisation tapitra");
            terrain = new Terrain(configurationJeu.TerrainWidth, configurationJeu.TerrainHeight);
            ball = new Ball(configurationJeu.BallStartX, configurationJeu.BallStartY);

            // Créer les deux raquettes
            raquetteJoueur1 = new Raquette(configurationJeu.Raquette1X, configurationJeu.Raquette1Y);
            raquetteJoueur1.Couleur = Brushes.Blue;
            raquetteJoueur1.Width = configurationJeu.RaquetteWidth;
            raquetteJoueur2 = new Raquette(configurationJeu.Raquette2X, configurationJeu.Raquette2Y);
            raquetteJoueur2.Couleur = Brushes.Red;
            raquetteJoueur2.Width = configurationJeu.RaquetteWidth;

            Console.WriteLine(" Initialiser le gestionnaire de pièces");
            // Initialiser le gestionnaire de pièces
            gestionnairePieces = new GestionnairePieces(terrain);
            gestionnairePieces.InitialiserPieces(nombrePiece);

            Console.WriteLine(" Initialiser le client réseau");
            // Initialiser le client réseau
            clientReseau = new ClientReseau();
            clientReseau.OnBallUpdate += ClientReseau_OnBallUpdate;
            clientReseau.OnRaquetteUpdate += ClientReseau_OnRaquetteUpdate;
            clientReseau.OnJoueurAssigne += ClientReseau_OnJoueurAssigne;

            // Demander si mode réseau
            PremiereEntree.DemanderModeReseau(clientReseau, modeReseau);

            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 10;
            Console.WriteLine("Hiditra Game loop");
            gameLoop.Tick += GameLoop_Tick;
            Console.WriteLine("Game loop tick voahantso");
            gameLoop.Start();
        }

        private void ClientReseau_OnJoueurAssigne(int joueurId)
        {
            // Cette méthode est appelée depuis un thread réseau
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientReseau_OnJoueurAssigne(joueurId)));
                return;
            }

            monJoueurId = joueurId;
            string couleur = joueurId == 1 ? "BLEUE (gauche)" : "ROUGE (droite)";
            MessageBox.Show($"Vous êtes le Joueur {joueurId}\nVous contrôlez la raquette {couleur}",
                "Assignation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClientReseau_OnRaquetteUpdate(RaquetteData raquetteData)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientReseau_OnRaquetteUpdate(raquetteData)));
                return;
            }

            // Ne pas mettre à jour MA propre raquette (éviter les conflits)
            if (raquetteData.JoueurId == monJoueurId)
                return;

            // Mettre à jour la raquette de l'adversaire
            if (raquetteData.JoueurId == 1)
            {
                raquetteJoueur1.PosX = raquetteData.PosX;
                raquetteJoueur1.PosY = raquetteData.PosY;
            }
            else if (raquetteData.JoueurId == 2)
            {
                raquetteJoueur2.PosX = raquetteData.PosX;
                raquetteJoueur2.PosY = raquetteData.PosY;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            InputManager.Instance.OnKeyDown(e.KeyCode);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            InputManager.Instance.OnKeyUp(e.KeyCode);
        }

        private void ClientReseau_OnBallUpdate(BallData ballData)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientReseau_OnBallUpdate(ballData)));
                return;
            }

            typeof(Ball).GetProperty("PosX")?.SetValue(ball, ballData.PosX);
            typeof(Ball).GetProperty("PosY")?.SetValue(ball, ballData.PosY);
            ball.SpeedX = ballData.SpeedX;
            ball.SpeedY = ballData.SpeedY;
        }

        private async void GameLoop_Tick(object? sender, EventArgs e)
        {
            InputManager.Instance.Update();

            // Mise à jour de la balle (seulement en mode local)
            if (!modeReseau)
            {
                Console.WriteLine("mode local");
                ball.Update(terrain);
            }

            Console.WriteLine("mode reseau");
            // Contrôler MA raquette
            if (modeReseau && monJoueurId != -1)
            {
                Console.WriteLine("Controlle raquette");
                Raquette maRaquette = monJoueurId == 1 ? raquetteJoueur1 : raquetteJoueur2;

                float ancienneX = maRaquette.PosX;
                float ancienneY = maRaquette.PosY;

                // Déplacer la raquette avec les touches
                float vitesse = 7f;
                if (InputManager.Instance.EstPresse(Keys.Up))
                    maRaquette.PosY -= vitesse;
                if (InputManager.Instance.EstPresse(Keys.Down))
                    maRaquette.PosY += vitesse;
                if (InputManager.Instance.EstPresse(Keys.Left))
                    maRaquette.PosX -= vitesse;
                if (InputManager.Instance.EstPresse(Keys.Right))
                    maRaquette.PosX += vitesse;

                // Limiter dans les bords
                maRaquette.PosX = Math.Clamp(maRaquette.PosX, 0, terrain.Width - maRaquette.Width);
                maRaquette.PosY = Math.Clamp(maRaquette.PosY, 0, terrain.Height - maRaquette.Height);

                // Envoyer la position au serveur si elle a changé
                if ((maRaquette.PosX != ancienneX || maRaquette.PosY != ancienneY) &&
                    (DateTime.Now - dernierEnvoi).TotalMilliseconds >= INTERVALLE_ENVOI_MS)
                {
                    var message = MessageReseau.CreerUpdateRaquette(monJoueurId, maRaquette.PosX, maRaquette.PosY);
                    Console.WriteLine("Raquette " + monJoueurId + " Mihetsika");
                    await clientReseau.EnvoyerMessageAsync(message);
                    dernierEnvoi = DateTime.Now;
                }
            }
            // else if (!modeReseau)
            // {
            //     // Mode local : contrôler la raquette 1 simplement
            //     float vitesse = 7f;
            //     if (InputManager.Instance.EstPresse(Keys.Up))
            //         raquetteJoueur1.PosY -= vitesse;
            //     if (InputManager.Instance.EstPresse(Keys.Down))
            //         raquetteJoueur1.PosY += vitesse;

            //     raquetteJoueur1.PosY = Math.Clamp(raquetteJoueur1.PosY, 0, terrain.Height - raquetteJoueur1.Height);
            // } 

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Fond
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, terrain.Width, terrain.Height);

            // Dessiner les pièces d'échecs (en arrière-plan)
            gestionnairePieces.Draw(e.Graphics);

            // Dessiner balle
            ball.Draw(e.Graphics);

            // Dessiner les raquettes (au premier plan)
            raquetteJoueur1.Draw(e.Graphics);
            raquetteJoueur2.Draw(e.Graphics);

            // Afficher des infos de debug
            if (modeReseau)
            {
                string debug = $"Joueur {monJoueurId} | ";
                debug += monJoueurId == 1 ? "Raquette BLEUE" : "Raquette ROUGE";
                e.Graphics.DrawString(debug, new Font("Arial", 14, FontStyle.Bold), Brushes.White, 10, 10);

                // Afficher le nombre de pièces
                int piecesJ1 = gestionnairePieces.CompterPiecesVivantes(1);
                int piecesJ2 = gestionnairePieces.CompterPiecesVivantes(2);
                string infoPieces = $"Pièces J1: {piecesJ1} | Pièces J2: {piecesJ2}";
                e.Graphics.DrawString(infoPieces, new Font("Arial", 12), Brushes.Yellow, 10, 35);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            clientReseau.Deconnecter();
        }
    }
}