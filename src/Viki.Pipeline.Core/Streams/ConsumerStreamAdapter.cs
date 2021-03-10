using System.Collections.Generic;
using System.Linq;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Streams
{
    public class ConsumerStreamAdapter : CombinedStream
    {
        public ConsumerStreamAdapter(IConsumer<Packet> consumer, int pollingDelayMilliseconds = 100)
            : base(PacketConsumerToStreams(consumer, pollingDelayMilliseconds))
             
        {
        }

        private static IEnumerable<PacketStream> PacketConsumerToStreams(IConsumer<Packet> consumer, int pollingDelayMilliseconds)
        {
            return consumer
                .ToEnumerable(pollingDelayMilliseconds)
                .Select(p => new PacketStream(p));
        }
    }
}