using System.Collections.Generic;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Streams
{
    public class ConsumerStreamAdapter : CombinedAsyncStream
    {
        public ConsumerStreamAdapter(IConsumer<Packet> consumer, int pollingDelayMilliseconds = 100)
            : base(PacketConsumerToStreams(consumer, pollingDelayMilliseconds))
             
        {
        }

        private static async IAsyncEnumerable<PacketStream> PacketConsumerToStreams(IConsumer<Packet> consumer, int pollingDelayMilliseconds)
        {
            await foreach (var packet in consumer.ToAsyncEnumerable(pollingDelayMilliseconds))
            {
                yield return new PacketStream(packet);
            }
        }
    }
}