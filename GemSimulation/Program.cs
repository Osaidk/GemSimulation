using System;
using System.Threading.Tasks;
using GemSimulation.Equipment;
using GemSimulation.Host;
using GemSimulation.Models;

namespace GemSimulation
{
    public class Program
    {
        public static async Task Main()
        {
            const int port = 5000;

            // Start Equipment (server)
            var equipment = new GemEquipment();
            var equipmentTask = equipment.StartAsync(port);

            // Small delay to ensure the server is listening
            await Task.Delay(500);

            // Start Host (client)
            var host = new GemHost();
            await host.ConnectAsync("127.0.0.1", port);

            Console.WriteLine("Type SECS messages in 'SxFy' format (e.g., S1F13, S1F15, S1F17, S2F41)");
            Console.WriteLine("Type 'exit' to close the Host.");

            string? input;

            while ((input = Console.ReadLine()) != null)
            {
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                var msg = ParseInputToSecsMessage(input);
                await host.SendSecsMessageAsync(msg);

                if (input.Equals("S1F15", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[Host] Equipment will go offline...");
                    break;
                }


            }
            // Send S1F13 (Establish Communication Request)
            Console.WriteLine("Host terminated.");
            await equipmentTask;
        }
    
            private static SecsMessage ParseInputToSecsMessage(string input)
        {
            try
            {
                if (input.StartsWith("S") && input.Contains("F"))
                {
                    var parts = input[1..].Split('F');
                    int stream = int.Parse(parts[0]);
                    int function = int.Parse(parts[1]);
                    return new SecsMessage(stream, function, $"Manual Command {input}", true);
                }
            }
            catch
            {
                // ignore parse errors
            }

            Console.WriteLine("Invalid format. Using S1F13 as default.");
            return new SecsMessage(1, 13, "Establish Communication Request", true);
        }
    }
}