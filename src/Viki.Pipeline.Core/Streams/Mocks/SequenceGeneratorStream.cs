using System;
using Viki.Pipeline.Core.Streams.Base;
using Viki.Pipeline.Core.Streams.Mocks.Components;

namespace Viki.Pipeline.Core.Streams.Mocks
{
    public class SequenceGeneratorStream : UnbufferedReadOnlyStreamBase
    {
        private readonly long _size;
        private readonly ISequenceGenerator _generator;

        private long _position = 0;
        private long _latestDividerPosition = -1;

        private long _nextElementLength;

        /// <summary>
        /// Indicates if stream is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Initialize new instance with custom seed
        /// </summary>
        /// <param name="size">Size of data in bytes that stream will allow to read before reporting end of stream</param>
        /// <param name="generator">A generator which provides all the rng methods required</param>
        public SequenceGeneratorStream(long size, ISequenceGenerator generator)
        {
            _size = size;
            _generator = generator;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(SequenceGeneratorStream));

            long bytesLeftToReadTotal = _size - _position;
            if (bytesLeftToReadTotal == 0)
                return 0;

            int bytesToRead = (int)Math.Min(bytesLeftToReadTotal, count);
            for (int i = 0; i < bytesToRead; i++)
            {
                int bufferWritePosition = offset + i;

                long activeGlobalWritePosition = _position + i;
                long positionSinceLastDivider = activeGlobalWritePosition - _latestDividerPosition;

                // prevent numbers from beginning with "0" ar sometimes janky json tools don't like it (e.g 0009 can fail some random validator)
                if (positionSinceLastDivider == 1)
                {
                    byte nextFirstByte = _generator.NextElementFirstByte();
                    _nextElementLength = _generator.NextElementLength();

                    // this extra clause solves issues where randomness of length could not be equally spread between desired min/max length
                    if (_size - activeGlobalWritePosition - 1 <= _nextElementLength) 
                        _nextElementLength = _size - activeGlobalWritePosition - 2; 

                    buffer[bufferWritePosition] = nextFirstByte;
                }
                // add comma if number is big enough and next byte is not the end of stream.
                // (so not to end with comma in generated sequence of numbers... e.g."123,123,")
                else if (positionSinceLastDivider > _nextElementLength && activeGlobalWritePosition + 1 != _size)
                {
                    buffer[bufferWritePosition] = _generator.NextSeparator();
                    _latestDividerPosition = activeGlobalWritePosition;
                }
                // business as usual x]
                else
                {
                    buffer[bufferWritePosition] = _generator.NextElementByte();
                }
            }

            _position += bytesToRead;

            return bytesToRead;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }
    }
}