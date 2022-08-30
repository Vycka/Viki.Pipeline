using System;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Streams
{
    /// <summary>
    /// Generates a one-time readable byte-stream mock of specified size
    /// </summary>
    public class StreamGenerator : UnbufferedReadOnlyStreamBase
    {
        /// <summary>
        /// Indicates if stream is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        private readonly long _size;
        private readonly byte _fillValue;

        private long _position = 0;

        /// <summary>
        /// Create stream
        /// </summary>
        /// <param name="size">Size of data in bytes that stream will allow to read before reporting end of stream</param>
        /// <param name="fillValue">value to fill</param>
        public StreamGenerator(long size, byte fillValue)
        {
            _size = size;
            _fillValue = fillValue;
        }

        /// <summary>
        /// Create stream
        /// </summary>
        /// <param name="size">Length of stream</param>
        /// <param name="fillValue">value to fill</param>
        public StreamGenerator(long size, char fillValue) : this(size, Convert.ToByte(fillValue)) { }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(StreamGenerator));

            if (_position >= _size)
                return 0;
            if ((int)_position + count > _size)
                count = (int)(_size - _position);
            _position += count;

            System.Runtime.CompilerServices.Unsafe.InitBlock(ref buffer[offset], _fillValue, Convert.ToUInt32(count));

            return count;
        }
    }
}