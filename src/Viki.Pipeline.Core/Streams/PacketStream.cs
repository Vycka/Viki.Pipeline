using System.IO;
using Viki.Pipeline.Core.Packets;

namespace Viki.Pipeline.Core.Streams
{
    public class PacketStream : MemoryStream
    {
        private readonly Packet<byte> _packet;

        public PacketStream(Packet<byte> packet)
            : base(packet.Data, 0, packet.DataLength, false)
        {
            _packet = packet;
        }

        protected override void Dispose(bool disposing)
        {
            _packet.Dispose();

            base.Dispose(disposing);
        }
    }
}