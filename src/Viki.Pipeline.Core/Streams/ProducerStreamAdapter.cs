﻿using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Packets;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Streams
{
    public class ProducerStreamAdapter : UnbufferedWriteOnlyStreamBase
    {
        private readonly IProducer<Packet<byte>> _producer;
        private readonly bool _closeProducer;

        private static ArrayPool<byte> ArrayPool = ArrayPool<byte>.Shared;

        public ProducerStreamAdapter(IProducer<Packet<byte>> producer, bool closeProducer = true)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _closeProducer = closeProducer;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] localBuffer = ArrayPool.Rent(count);

            Array.Copy(buffer, offset, localBuffer, 0, count);

            Packet<byte> packet = new Packet<byte>(localBuffer, count, ArrayPool);

            _producer.Produce(packet);
        }


        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // IProducer sync write operation should be cheaper than spawning a task, thus we suppress it.
            Write(buffer, offset, count);
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (_closeProducer)
                _producer.ProducingCompleted();

            base.Dispose(disposing);
        }
    }
}