using pong_echec.Game;

namespace pong_echec
{
    public partial class MainForm : Form
    {
        Terrain terrain;
        Ball ball;
        System.Windows.Forms.Timer gameLoop;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            terrain = new Terrain(800, 450); // taille zone
            ball = new Ball(400, 300);

            gameLoop = new System.Windows.Forms.Timer();
            gameLoop.Interval = 10; // ~60 FPS
            gameLoop.Tick += GameLoop_Tick;
            gameLoop.Start();
        }

        private void GameLoop_Tick(object? sender, EventArgs e)
        {
            ball.Update(terrain);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // fond
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, terrain.Width, terrain.Height);
            // dessiner balle
            ball.Draw(e.Graphics);
        }

    }
}
