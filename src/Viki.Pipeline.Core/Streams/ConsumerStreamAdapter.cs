using System.Collections.Generic;
using System.Linq;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Pipes.Interfaces;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Streams
{
    public class ConsumerStreamAdapter : CombinedStream
    {
        public ConsumerStreamAdapter(IConsumer<Packet> consumer)
            : base(PacketConsumerToStreams(consumer))
             
        {
        }

        private static IEnumerable<PacketStream> PacketConsumerToStreams(IConsumer<Packet> consumer)
        {
            return consumer
                .ToEnumerable()
                .Select(p => new PacketStream(p));
        }
    }
}