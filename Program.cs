using System;

namespace Conway
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new GameofLife())
                game.Run();
        }
    }
}
