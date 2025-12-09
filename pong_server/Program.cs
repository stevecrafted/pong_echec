using System;
using System.Threading.Tasks;

namespace PongServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Serveur Pong Réseau ===");
            Console.WriteLine();

            int port = 5555;
            
            if (args.Length > 0 && int.TryParse(args[0], out int customPort))
            {
                port = customPort;
            }

            var server = new GameServer(port);
            
            try
            {
                await server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur serveur: {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}