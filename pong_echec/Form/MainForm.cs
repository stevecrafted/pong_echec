using pong_echec.Game;

namespace pong_echec
{
    public partial class MainForm : Form
    {
        Terrain terrain;
        Ball ball;
        System.Windows.Forms.Timer gameLoop;
        Raquette raquetteJouerUn;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            terrain = new Terrain(1600, 900); // taille zone
            ball = new Ball(400, 300);

            raquetteJouerUn = new Raquette(50, 200);
            raquetteJouerUn.VitesseX = 5;

            raquetteJouerUn.Move(1, 0);

            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 10; 
            gameLoop.Tick += GameLoop_Tick;
            gameLoop.Start();
        }

        private void GameLoop_Tick(object? sender, EventArgs e)
        {
            ball.Update(terrain);

            // Truc temporelle pour faire bouger la raquette
            raquetteJouerUn.UpdatePosition(terrain);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // fond
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, terrain.Width, terrain.Height);
            
            // dessiner balle, raquette
            ball.Draw(e.Graphics);
            raquetteJouerUn.Draw(e.Graphics);
        }

    }
}
