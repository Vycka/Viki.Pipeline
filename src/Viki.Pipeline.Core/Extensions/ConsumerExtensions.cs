﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Extensions
{
    public static class ConsumerExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this IConsumer<T> consumer, int pollingDelayMilliseconds = 100)
        {
            while (consumer.Available)
            {
                IReadOnlyList<T> batch;
                while (consumer.TryLockBatch(out batch))
                {
                    for (int i = 0; i < batch.Count; i++)
                    {
                        yield return batch[i];
                    }

                    consumer.ReleaseBatch();
                }

                Thread.Sleep(pollingDelayMilliseconds);
            }
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IConsumer<T> consumer, int pollingDelayMilliseconds = 100)
        {
            while (consumer.Available)
            {
                IReadOnlyList<T> batch;
                while (consumer.TryLockBatch(out batch))
                {
                    for (int i = 0; i < batch.Count; i++)
                    {
                        yield return batch[i];
                    }

                    consumer.ReleaseBatch();
                }

                await Task.Delay(pollingDelayMilliseconds);
            }
        }

        public static Task ToEnumerableAsync<T>(this IConsumer<T> consumer, Action<IEnumerable<T>> consumeAction, int pollingDelayMilliseconds = 100, bool start = true)
        {
            Task task = new Task(() => consumeAction(consumer.ToEnumerable(pollingDelayMilliseconds)), TaskCreationOptions.LongRunning);

            if (start)
                task.Start();

            return task;
        }

        public static Stream ToReadOnlyStream(this IConsumer<Packet> consumer, int pollingDelayMilliseconds = 100)
        {
            return new ConsumerStreamAdapter(consumer, pollingDelayMilliseconds);
        }
    }
}