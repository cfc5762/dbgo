using System;

namespace Discus
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static Game1 game;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (game = new Game1())
                game.Run();
        }
    }
}
