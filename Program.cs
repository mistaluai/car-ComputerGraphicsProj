using OpenTK.Graphics.OpenGL4;

namespace OpenTK_Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game(500, 500))
            {
                game.Run();
            }
        }
    }
}