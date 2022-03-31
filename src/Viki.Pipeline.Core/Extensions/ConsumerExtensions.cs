using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Packets;
using Viki.Pipeline.Core.Streams;

namespace Viki.Pipeline.Core.Extensions
{
    public static class ConsumerExtensions
    {
        public static IEnumerable<Packet<T>> ToPacketsEnumerable<T>(this IConsumer<T> consumer, int pollingDelayMilliseconds = 100)
        {
            while (consumer.Available)
            {
                ICollection<T> batch;
                while (consumer.TryLockBatch(out batch))
                {
                    Packet<T> packet = Packet.CopyFrom(batch);

                    yield return packet;

                    consumer.ReleaseBatch();
                }

                Thread.Sleep(pollingDelayMilliseconds);
            }
        }

        public static async IAsyncEnumerable<Packet<T>> ToPacketsAsyncEnumerable<T>(this IConsumer<T> consumer, int pollingDelayMilliseconds = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (consumer.Available)
            {
                ICollection<T> batch;
                while (consumer.TryLockBatch(out batch))
                {
                    try
                    {
                        Packet<T> packet = Packet.CopyFrom(batch);

                        yield return packet;

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    finally
                    {
                        consumer.ReleaseBatch();
                    }
                }

                await Task.Delay(pollingDelayMilliseconds, cancellationToken);
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this IConsumer<T> consumer, int pollingDelayMilliseconds = 100)
        {
            while (consumer.Available)
            {
                ICollection<T> batch;
                while (consumer.TryLockBatch(out batch))
                {
                    foreach (T item in batch)
                    {
                        yield return item;
                    }

                    consumer.ReleaseBatch();
                }

                Thread.Sleep(pollingDelayMilliseconds);
            }
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IConsumer<T> consumer, int pollingDelayMilliseconds = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (consumer.Available)
            {
                ICollection<T> batch;
                while (consumer.TryLockBatch(out batch))
                {
                    try
                    {
                        foreach (T item in batch)
                        {
                            yield return item;
                        }
                        
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    finally
                    {
                        consumer.ReleaseBatch();
                    }

                }

                await Task.Delay(pollingDelayMilliseconds, cancellationToken);
            }
        }

        public static Task ToEnumerableAsync<T>(this IConsumer<T> consumer, Action<IEnumerable<T>> consumeAction, int pollingDelayMilliseconds = 100, bool start = true)
        {
            Task task = new Task(() => consumeAction(consumer.ToEnumerable(pollingDelayMilliseconds)), TaskCreationOptions.LongRunning);

            if (start)
                task.Start();

            return task;
        }

        public static Stream ToReadOnlyStream(this IConsumer<Packet<byte>> consumer, int pollingDelayMilliseconds = 100)
        {
            return new ConsumerStreamAdapter(consumer, pollingDelayMilliseconds);
        }
    }
}