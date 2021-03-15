using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Packets;
using Viki.Pipeline.Core.Streams;

namespace Viki.Pipeline.Core.Extensions
{
    public static class ProducerExtensions
    {
        public static Task ProduceCompleteAsync<T>(this IProducer<T> producer, IEnumerable<T> items, bool start = true)
        {
            return ProduceCompleteAsync(producer, items, CancellationToken.None, start);
        }

        public static async Task ProduceCompleteAsync<T>(this IProducer<T> producer, IAsyncEnumerable<T> items)
        {
            try
            {
                await foreach (T item in items)
                {
                    producer.Produce(item);
                }
            }
            finally
            {
                producer.ProducingCompleted();
            }
        }

        public static Task ProduceCompleteAsync<T>(this IProducer<T> producer, IEnumerable<T> items, CancellationToken token, bool start = true)
        {
            Task task = new Task(() => ProduceAndComplete(producer, items, token), token, TaskCreationOptions.LongRunning);

            if (start)
                task.Start();

            return task;
        }

        private static void ProduceAndComplete<T>(IProducer<T> producer, IEnumerable<T> items, CancellationToken token)
        {
            try
            {
                foreach (T item in items)
                {
                    if (token.IsCancellationRequested)
                        break;

                    producer.Produce(item);
                }
            }
            finally
            {
                producer.ProducingCompleted();
            }
        }

        public static Stream ToWriteOnlyStream(this IProducer<Packet<byte>> producer, bool closeProducer = true)
        {
            return new ProducerStreamAdapter(producer, closeProducer);
        }
    }
}