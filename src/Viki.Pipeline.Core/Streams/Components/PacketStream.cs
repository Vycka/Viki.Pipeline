using System.IO;

namespace Viki.Pipeline.Core.Streams.Components
{
    public class PacketStream : MemoryStream
    {
        private readonly Packet _packet;

        public PacketStream(Packet packet)
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