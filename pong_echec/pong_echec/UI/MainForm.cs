using pong_echec.Game;
using pong_echec.Input;
using pong_echec.Reseau;
using pong_shared;
using pong_shared.Models;

namespace pong_echec.UI
{
    public partial class MainForm : Form
    {
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
            InitializeComponent();
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            terrain = new Terrain(900, 900);
            ball = new Ball(400, 300);

            // Créer les deux raquettes
            raquetteJoueur1 = new Raquette(50, 400);
            raquetteJoueur1.Couleur = Brushes.Blue;
            
            raquetteJoueur2 = new Raquette(1500, 400);
            raquetteJoueur2.Couleur = Brushes.Red;

            // Initialiser le gestionnaire de pièces
            gestionnairePieces = new GestionnairePieces(terrain);
            gestionnairePieces.InitialiserPieces();

            // Initialiser le client réseau
            clientReseau = new ClientReseau();
            clientReseau.OnBallUpdate += ClientReseau_OnBallUpdate;
            clientReseau.OnRaquetteUpdate += ClientReseau_OnRaquetteUpdate;
            clientReseau.OnJoueurAssigne += ClientReseau_OnJoueurAssigne;

            // Demander si mode réseau
            DemanderModeReseau();

            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 10; 
            gameLoop.Tick += GameLoop_Tick;
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

        private async void DemanderModeReseau()
        {
            var result = MessageBox.Show(
                "Voulez-vous jouer en mode réseau ?",
                "Mode de jeu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                string adresse = PromptAdresseServeur();
                if (!string.IsNullOrEmpty(adresse))
                {
                    bool connecte = await clientReseau.ConnecterAsync(adresse, 5000);
                    if (connecte)
                    {
                        modeReseau = true;
                        MessageBox.Show("Connecté au serveur!", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Impossible de se connecter au serveur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string PromptAdresseServeur()
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Adresse du serveur",
                StartPosition = FormStartPosition.CenterScreen
            };
            
            Label textLabel = new Label() { Left = 20, Top = 20, Text = "Adresse IP du serveur:", Width = 350 };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 350, Text = "127.0.0.1" };
            Button confirmation = new Button() { Text = "OK", Left = 250, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            
            confirmation.Click += (sender, e) => { prompt.Close(); };
            
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
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
                ball.Update(terrain);
            }

            // Contrôler MA raquette
            if (modeReseau && monJoueurId != -1)
            {
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
                    await clientReseau.EnvoyerMessageAsync(message);
                    dernierEnvoi = DateTime.Now;
                }
            }
            else if (!modeReseau)
            {
                // Mode local : contrôler la raquette 1 simplement
                float vitesse = 7f;
                if (InputManager.Instance.EstPresse(Keys.Up))
                    raquetteJoueur1.PosY -= vitesse;
                if (InputManager.Instance.EstPresse(Keys.Down))
                    raquetteJoueur1.PosY += vitesse;
                
                raquetteJoueur1.PosY = Math.Clamp(raquetteJoueur1.PosY, 0, terrain.Height - raquetteJoueur1.Height);
            }

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