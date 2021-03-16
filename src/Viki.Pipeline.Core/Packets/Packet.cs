using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Viki.Pipeline.Core.Packets
{
    public class Packet<T>: IDisposable
    {
        /// <summary>
        /// Carried data array. Keep in mind that length of array might be bigger than the usable data contained
        /// - use DataLength property to get size of usable data.
        /// </summary>
        public T[] Data { get; private set; }

        /// <summary>
        /// Usable data length in array.
        /// </summary>
        public readonly int DataLength;

        private readonly ArrayPool<T> _arrayPool;

        /// <summary>
        /// Creates array packet which will be returned to provided ArrayPool on dispose
        /// </summary>
        /// <param name="data">data array</param>
        /// <param name="dataLength">usable data length in data array</param>
        /// <param name="arrayPool">ArrayPool to return disposed array</param>
        public Packet(T[] data, int dataLength, ArrayPool<T> arrayPool)
        {
            DataLength = dataLength;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            _arrayPool = arrayPool ?? throw new ArgumentNullException(nameof(arrayPool));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Data != null)
            {
                _arrayPool.Return(Data);
                Data = null;
            }
        }
    }

    /// <summary>
    /// Packet initializers for common cases?
    /// </summary>
    public static class Packet
    {
        /// <summary>
        /// Copies array and creates a Packet instance pooled under Shared ArrayPool
        /// (expected performance breakeven at 1k+ records)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">data source</param>
        /// <param name="count">items count to copy from data source</param>
        /// <param name="offset">data source reading position offset</param>
        /// <returns></returns>
        public static Packet<T> CopyFrom<T>(T[] source, int count, int offset = 0)
        {
            ArrayPool<T> arrayPool = ArrayPool<T>.Shared;

            T[] pooledArray = arrayPool.Rent(count);

            Array.Copy(source, offset, pooledArray, 0, count);

            return new Packet<T>(pooledArray, count, arrayPool);
        }

        /// <summary>
        /// Copies collection and creates a Packet instance pooled under Shared ArrayPool
        /// (expected performance breakeven at 1k+ records)
        /// </summary>
        public static Packet<T> CopyFrom<T>(ICollection<T> source)
        {
            ArrayPool<T> arrayPool = ArrayPool<T>.Shared;

            T[] pooledArray = arrayPool.Rent(source.Count);

            source.CopyTo(pooledArray, 0);

            return new Packet<T>(pooledArray, source.Count, arrayPool);
        }
    }
}