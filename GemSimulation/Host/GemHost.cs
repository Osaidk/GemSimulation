using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using GemSimulation.Models;


namespace GemSimulation.Host
{
    public class GemHost
    {
        private TcpClient _client = new();
        private NetworkStream? _stream = default!;

        public async Task ConnectAsync(string host, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            Console.WriteLine("Host connected to equipment.");
        }

        public async Task SendSecsMessageAsync(SecsMessage msg)
        {
            var data = msg.ToBytes();
            await _stream.WriteAsync(data);
            Console.WriteLine($"Host sent: {msg}");

            // Wait for reply
            var buffer = new byte[256];
            int bytes = await _stream.ReadAsync(buffer);
            var reply = SecsMessage.FromBytes(buffer[..bytes]);
            Console.WriteLine($"Host received: {reply}");
        }
    }
}