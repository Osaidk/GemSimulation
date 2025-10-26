using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GemSimulation.Models;

namespace GemSimulation.Equipment
{
    public class GemEquipment
    {
        private TcpListener? _listener;
        private EquipmentState _state = EquipmentState.Offline;
        private bool _running = true;
        public async Task StartAsync(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine($"Equipment listening on port {port}...");

            using var client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Host connected to equipment.");
            using var stream = client.GetStream();

            _state = EquipmentState.Local;

            while (_running)
            {
                var buffer = new byte[256];
                int bytes = await stream.ReadAsync(buffer);
                if (bytes == 0) break;

                var msg = SecsMessage.FromBytes(buffer[..bytes]);
                Console.WriteLine($"Equipment received: {msg}");

                var reply = ProcessMessage(msg);

                if (reply != null)
                {
                    var replyBytes = reply.ToBytes();
                    await stream.WriteAsync(replyBytes);
                    Console.WriteLine($"→ [Equipment] Sent: {reply}");
                }
                
                if (_state == EquipmentState.Offline)
                {
                    Console.WriteLine("[Equipment] Going offline. Shutting down...");
                    _running = false;
                }
            }

            _listener.Stop();
            
        }
    
        
        private SecsMessage? ProcessMessage(SecsMessage msg)
        {
            switch (msg.Stream, msg.Function)
            {
                case (1, 13):
                    _state = EquipmentState.Remote;
                    return new SecsMessage(1, 14, "Establish Communication Acknowledge");
                case (1, 15):
                    _state = EquipmentState.Offline;
                    return new SecsMessage(1, 16, "Offline Acknowledge");
                case (1, 17):
                    _state = EquipmentState.Remote;
                    return new SecsMessage(1, 18, "Online Acknowledge");
                case (2, 41):
                    return new SecsMessage(2, 42, "Command Accepted");

                default:
                return new SecsMessage(msg.Stream, msg.Function + 1, "Unknown Message Reply");
            }
        }
    }
}