using System.Collections.Generic;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Packets;

namespace Viki.Pipeline.Core.Streams
{
    // TODO: CombinedAsyncStream is a temporary solution, as making stream out of IProducer directly would cut away to IEnumerables related layer
    // Especially when CombinedAsyncStream introduces some questionable behaviour when dealing with Async enumerables.
    public class ConsumerStreamAdapter : CombinedStream
    {
        public ConsumerStreamAdapter(IConsumer<Packet<byte>> consumer, int pollingDelayMilliseconds = 100)
            : base(PacketConsumerToStreams(consumer, pollingDelayMilliseconds))
             
        {
        }

        private static IEnumerable<PacketStream> PacketConsumerToStreams(IConsumer<Packet<byte>> consumer, int pollingDelayMilliseconds)
        {
            foreach (var packet in consumer.ToEnumerable(pollingDelayMilliseconds))
            {
                yield return new PacketStream(packet);
            }
        }
    }
}