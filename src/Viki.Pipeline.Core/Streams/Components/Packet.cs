﻿using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;

namespace Viki.Pipeline.Core.Streams.Components
{
    public class Packet : IDisposable
    {
        public readonly int DataLength;
        public byte[] Data { get; private set; } 

        private readonly ArrayPool<byte> _arrayPool;

        public Packet(byte[] data, int dataLength, ArrayPool<byte> arrayPool)
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

        public static async Task<Packet> ReadFrom(Stream stream)
        {
            MemoryStream localCopy = new MemoryStream();

            await stream.CopyToAsync(localCopy);

            byte[] localCopyBytes = localCopy.ToArray();
            return new Packet(localCopyBytes, localCopyBytes.Length, NullArrayPool.Instance);
        }
    }
}