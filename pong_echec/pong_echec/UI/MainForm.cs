using pong_shared.Models;
using pong_echec.Input;
using pong_echec.Reseau;
using pong_shared;
using pong_echec.Game;
using System.Collections.Generic;
using System.Windows.Forms;

namespace pong_echec.UI
{
    public partial class MainForm : Form
    {
        ConfigurationJeu configurationJeu;
        Terrain terrain;
        Ball ball;
        System.Windows.Forms.Timer gameLoop;
        GestionnairePieces gestionnairePieces;
        Raquette raquetteJoueur1;
        Raquette raquetteJoueur2;
        ClientReseau clientReseau;
        bool modeReseau = false;
        int monJoueurId = -1;
        private DateTime dernierEnvoi = DateTime.Now;
        private const int INTERVALLE_ENVOI_MS = 16;

        // UI for Ready state
        private Button readyButton;
        private Label statusLabel;
        private GameStateType currentGameState = GameStateType.WaitingForPlayers;

        public MainForm()
        {
            int nombrePiece = 4;
            configurationJeu = ConfigurationJeu.ObtenirConfiguration(nombrePiece);

            InitializeComponent(configurationJeu);
            this.DoubleBuffered = true;
            this.KeyPreview = true;

            terrain = new Terrain(configurationJeu.TerrainWidth, configurationJeu.TerrainHeight);
            ball = new Ball(configurationJeu.BallStartX, configurationJeu.BallStartY);
            gestionnairePieces = new GestionnairePieces(terrain);

            raquetteJoueur1 = new Raquette(configurationJeu.Raquette1X, configurationJeu.Raquette1Y) { Couleur = Brushes.Blue, Width = configurationJeu.RaquetteWidth };
            raquetteJoueur2 = new Raquette(configurationJeu.Raquette2X, configurationJeu.Raquette2Y) { Couleur = Brushes.Red, Width = configurationJeu.RaquetteWidth };

            clientReseau = new ClientReseau();
            clientReseau.OnBallUpdate += ClientReseau_OnBallUpdate;
            clientReseau.OnRaquetteUpdate += ClientReseau_OnRaquetteUpdate;
            clientReseau.OnJoueurAssigne += ClientReseau_OnJoueurAssigne;
            clientReseau.OnPiecesUpdate += ClientReseau_OnPiecesUpdate;
            clientReseau.OnGameStateUpdate += ClientReseau_OnGameStateUpdate;

            InitialiserUIReady();
            InitialiserModeReseauAsync();

            gameLoop = new System.Windows.Forms.Timer { Interval = 10 };
            gameLoop.Tick += GameLoop_Tick;
            gameLoop.Start();
        }

        private void InitialiserUIReady()
        {
            statusLabel = new Label
            {
                Text = "En attente des joueurs...",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            readyButton = new Button
            {
                Text = "Prêt",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Size = new Size(120, 50),
                Location = new Point((this.ClientSize.Width - 120) / 2, (this.ClientSize.Height - 50) / 2)
            };
            readyButton.Click += ReadyButton_Click;

            this.Controls.Add(statusLabel);
            this.Controls.Add(readyButton);
        }

        private async void ReadyButton_Click(object sender, EventArgs e)
        {
            await clientReseau.EnvoyerMessageAsync(MessageReseau.CreerPlayerReady());
            readyButton.Enabled = false;
            readyButton.Text = "En attente...";
        }

        private void ClientReseau_OnGameStateUpdate(GameState gameState)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientReseau_OnGameStateUpdate(gameState)));
                return;
            }

            currentGameState = gameState.StateType;
            switch (currentGameState)
            {
                case GameStateType.WaitingForPlayers:
                    statusLabel.Text = "En attente d'un autre joueur...";
                    readyButton.Visible = true;
                    break;
                case GameStateType.InProgress:
                    statusLabel.Visible = false;
                    readyButton.Visible = false;
                    break;
                case GameStateType.GameOver:
                    statusLabel.Text = "Partie terminée!";
                    statusLabel.Visible = true;
                    break;
            }
        }

        private void ClientReseau_OnPiecesUpdate(List<PieceData> piecesData)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientReseau_OnPiecesUpdate(piecesData)));
                return;
            }
            gestionnairePieces.UpdatePieces(piecesData);
        }

        private async void InitialiserModeReseauAsync()
        {
            modeReseau = await PremiereEntree.DemanderModeReseau(clientReseau);
        }

        private void ClientReseau_OnJoueurAssigne(int joueurId)
        {
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

            if (raquetteData.JoueurId == monJoueurId)
                return;

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

            if (modeReseau && monJoueurId != -1 && currentGameState == GameStateType.InProgress)
            {
                Raquette maRaquette = monJoueurId == 1 ? raquetteJoueur1 : raquetteJoueur2;
                float ancienneX = maRaquette.PosX;
                float ancienneY = maRaquette.PosY;

                float vitesse = 7f;
                if (InputManager.Instance.EstPresse(Keys.Up)) maRaquette.PosY -= vitesse;
                if (InputManager.Instance.EstPresse(Keys.Down)) maRaquette.PosY += vitesse;
                if (InputManager.Instance.EstPresse(Keys.Left)) maRaquette.PosX -= vitesse;
                if (InputManager.Instance.EstPresse(Keys.Right)) maRaquette.PosX += vitesse;

                maRaquette.PosX = Math.Clamp(maRaquette.PosX, 0, terrain.Width - maRaquette.Width);
                maRaquette.PosY = Math.Clamp(maRaquette.PosY, 0, terrain.Height - maRaquette.Height);

                if ((maRaquette.PosX != ancienneX || maRaquette.PosY != ancienneY) &&
                    (DateTime.Now - dernierEnvoi).TotalMilliseconds >= INTERVALLE_ENVOI_MS)
                {
                    var message = MessageReseau.CreerUpdateRaquette(monJoueurId, maRaquette.PosX, maRaquette.PosY);
                    await clientReseau.EnvoyerMessageAsync(message);
                    dernierEnvoi = DateTime.Now;
                }
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.FillRectangle(Brushes.Black, 0, 0, terrain.Width, terrain.Height);
            
            if (currentGameState == GameStateType.InProgress)
            {
                gestionnairePieces.Draw(e.Graphics);
                ball.Draw(e.Graphics);
                raquetteJoueur1.Draw(e.Graphics);
                raquetteJoueur2.Draw(e.Graphics);
            }

            if (modeReseau)
            {
                int piecesJ1 = gestionnairePieces.CompterPiecesVivantes(1);
                int piecesJ2 = gestionnairePieces.CompterPiecesVivantes(2);
                string infoPieces = $"Pièces J1: {piecesJ1} | Pièces J2: {piecesJ2}";
                e.Graphics.DrawString(infoPieces, new Font("Arial", 12), Brushes.Yellow, 10, 35);

                string debug = $"Joueur {monJoueurId} | ";
                debug += monJoueurId == 1 ? "Raquette BLEUE" : "Raquette ROUGE";
                e.Graphics.DrawString(debug, new Font("Arial", 14, FontStyle.Bold), Brushes.White, 10, 10);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            clientReseau.Deconnecter();
        }
    }
}