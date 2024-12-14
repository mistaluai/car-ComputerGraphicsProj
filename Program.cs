using OpenTK.Graphics.OpenGL4;

namespace OpenTK_Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game(1280, 720))
            {
                game.Run();
            }
        }
    }
}