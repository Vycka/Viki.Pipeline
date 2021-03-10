using System;
using System.Buffers;

namespace Viki.Pipeline.Core.Streams.Components
{
    public class Packet : IDisposable
    {
        public readonly int DataLength;
        public readonly byte[] Data;

        private readonly ArrayPool<byte> _arrayPool;

        public Packet(byte[] data, int dataLength, ArrayPool<byte> arrayPool)
        {
            DataLength = dataLength;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            _arrayPool = arrayPool ?? throw new ArgumentNullException(nameof(arrayPool));
        }

        public void Dispose()
        {
            _arrayPool.Return(Data);
        }
    }
}