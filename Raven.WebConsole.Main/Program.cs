using System;
using System.IO;

namespace Raven.WebConsole.Main 
{
    internal class Program 
    {
        private static void Main(string[] args) 
        {
            var port = 9124;

            if (args.Length > 0) {
                port = int.Parse(args[0]);
            }

            var webDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\RavenDB.WebConsole"));
            using (var server = new WebServer(webDir, port, "RavenDB.WebConsole"))
            {
                server.Start();

                Console.WriteLine("Server Started on port {0}! Press Enter to Shutdown", port);
                Console.ReadLine();

                Console.WriteLine("Shutting down");
                server.Stop();
            }
        }
    }
}
