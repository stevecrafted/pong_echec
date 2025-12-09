using pong_echec.Game;
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
        Raquette raquetteJouerUn;
        
        // Client réseau
        ClientReseau clientReseau;
        bool modeReseau = false;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            terrain = new Terrain(1600, 900);
            ball = new Ball(400, 300);

            raquetteJouerUn = new Raquette(50, 200);
            raquetteJouerUn.VitesseX = 5;
            raquetteJouerUn.Move(1, 0);

            // Initialiser le client réseau
            clientReseau = new ClientReseau();
            clientReseau.OnBallUpdate += ClientReseau_OnBallUpdate;

            // Demander si mode réseau
            DemanderModeReseau();

            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 10; 
            gameLoop.Tick += GameLoop_Tick;
            gameLoop.Start();
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
                // Demander l'adresse du serveur
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

        private void ClientReseau_OnBallUpdate(BallData ballData)
        {
            // Mettre à jour la balle avec les données du serveur
            // Cette méthode sera appelée depuis un thread réseau, donc utiliser Invoke
            if (InvokeRequired)
            {
                Invoke(new Action(() => ClientReseau_OnBallUpdate(ballData)));
                return;
            }

            // Mettre à jour directement les propriétés privées via réflexion
            // ou modifier la classe Ball pour avoir des setters publics
            typeof(Ball).GetProperty("PosX")?.SetValue(ball, ballData.PosX);
            typeof(Ball).GetProperty("PosY")?.SetValue(ball, ballData.PosY);
            ball.SpeedX = ballData.SpeedX;
            ball.SpeedY = ballData.SpeedY;
        }
        
        private void GameLoop_Tick(object? sender, EventArgs e)
        {
            // En mode réseau, ne pas mettre à jour la balle localement
            // Le serveur envoie les mises à jour
            if (!modeReseau)
            {
                ball.Update(terrain);
            }

            raquetteJouerUn.UpdatePosition(terrain);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, terrain.Width, terrain.Height);
            
            ball.Draw(e.Graphics);
            raquetteJouerUn.Draw(e.Graphics);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            clientReseau.Deconnecter();
        }
    }
}