using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Pipes;

namespace Viki.Pipeline.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (T item in source)
            {
                yield return await Task.FromResult(item);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public static IEnumerable<IEnumerable<T>> ToBatchesAsync<T>(this IEnumerable<T> source, int batchSize)
        {
            using IEnumerator<T> enumerator = source.GetEnumerator();

            int index = 0;
            while (enumerator.MoveNext())
            {
                BatchingPipe<T> batch = new BatchingPipe<T>();
                Task task = Task.Run(() => ProduceBatch(enumerator, batch, batchSize));

                yield return batch.ToEnumerable();

                Task.WaitAll(task);

                if (!task.IsCompletedSuccessfully)
                    throw new Exception("Failure while reading source enumerable", task.Exception);
            }
        }
        private static void ProduceBatch<T>(IEnumerator<T> enumerator, IProducer<T> pipe, int size)
        {
            try
            {
                int count = 0;
                do
                {
                    pipe.Produce(enumerator.Current);

                    count++;
                    if (count >= size)
                        break;
                } while (enumerator.MoveNext());
            }
            finally
            {
                pipe.ProducingCompleted();
            }
        }
    }
}