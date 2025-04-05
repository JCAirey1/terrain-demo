using System;

namespace TConsole
{
    class Program
    {
        public static TerrainDemo.Logger MyLogger { get; } = new TerrainDemo.Logger();

        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            MyLogger.LogInfo("test");
            Console.WriteLine("Done");
        }
    }
}
