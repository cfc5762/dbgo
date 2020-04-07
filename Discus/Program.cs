using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
            try
            {
                using (game = new Game1())
                    game.Run();
            }
            catch (Exception e)
            {
                FileStream error = File.Create(DateTime.Now+".txt");
                //BinaryWriter w = new BinaryWriter(error);
                BinaryFormatter w = new BinaryFormatter();
                w.Serialize(error, e.StackTrace + "/n" + e.Message);
                error.Close();
                
            }
        }
    }
}
