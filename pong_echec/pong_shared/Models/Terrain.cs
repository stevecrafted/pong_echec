namespace pong_shared.Models
{
    public class Terrain
    {
        public int Width { get; }
        public int Height { get; }

        public Terrain(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
