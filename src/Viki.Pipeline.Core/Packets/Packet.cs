using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Viki.Pipeline.Core.Packets
{
    public class Packet<T>: IDisposable
    {
        public readonly int DataLength;
        public T[] Data { get; private set; } 

        private readonly ArrayPool<T> _arrayPool;

        public Packet(T[] data, int dataLength, ArrayPool<T> arrayPool)
        {
            DataLength = dataLength;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            _arrayPool = arrayPool ?? throw new ArgumentNullException(nameof(arrayPool));
        }

        public void Dispose()
        {
            if (Data != null)
            {
                _arrayPool.Return(Data);
                Data = null;
            }
        }
    }

    public static class Packet
    {
        public static Task<Packet<byte>> ReadFrom(Stream stream)
        {
            return ReadFrom(stream, NullArrayPool.Instance);
        }

        public static async Task<Packet<byte>> ReadFrom(Stream stream, ArrayPool<byte> arrayPool)
        {
            MemoryStream localCopy = new MemoryStream();

            await stream.CopyToAsync(localCopy);

            byte[] localCopyBytes = localCopy.ToArray();
            return new Packet<byte>(localCopyBytes, localCopyBytes.Length, arrayPool);
        }
    }
}