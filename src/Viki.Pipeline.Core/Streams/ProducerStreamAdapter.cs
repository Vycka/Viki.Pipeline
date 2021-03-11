using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Streams.Base;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Streams
{
    public class ProducerStreamAdapter : UnbufferedWriteOnlyStreamBase
    {
        private readonly IProducer<Packet> _producer;

        private static ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;

        public ProducerStreamAdapter(IProducer<Packet> producer)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] localBuffer = ArrayPool.Rent(count);

            Array.Copy(buffer, offset, localBuffer, 0, count);

            Packet packet = new Packet(localBuffer, count, NullArrayPool.Instance);

            _producer.Produce(packet);
        }


        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // IProducer sync write operation should be cheaper than spawning a task, thus we suppress it.
            Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        public override void Close()
        {
            _producer.ProducingCompleted();

            base.Close();
        }
        
        protected override void Dispose(bool disposing)
        {
            _producer.ProducingCompleted();

            base.Dispose(disposing);
        }
    }
}