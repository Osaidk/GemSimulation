using System;
using System.Text;

namespace GemSimulation.Models
{
    public class SecsMessage
    {
        public int Stream { get; }
        public int Function { get; }
        public string Name { get; }
        public bool ReplyExpected { get; }

        public SecsMessage(int stream, int function, string name, bool replyExpected = false) // constructor 
        {
            Stream = stream;
            Function = function;
            Name = name;
            ReplyExpected = replyExpected;

        }

        public byte[] ToBytes()
        {
            var text = $"{Stream}:{Function}:{Name}";
            return Encoding.ASCII.GetBytes(text);
        }

        public static SecsMessage FromBytes(byte[] data)
        {
            var text = Encoding.ASCII.GetString(data);
            var parts = text.Split(':');
            return new SecsMessage(int.Parse(parts[0]), int.Parse(parts[1]), parts[2]);
        }

        public override string ToString() => $"S{Stream}F{Function} - {Name}";

    }
}