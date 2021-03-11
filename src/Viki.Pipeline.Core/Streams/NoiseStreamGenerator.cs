using System;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Streams
{
    /// <summary>
    /// Generates stream with pseudo-random noise data
    /// </summary>
    public class NoiseStreamGenerator : UnbufferedReadOnlyStreamBase
    {
        private static readonly Random SeedGenerator;

        private readonly Random _noiseGenerator;
        private readonly long _size;
        private long _position = 0;


        static NoiseStreamGenerator()
        {
            SeedGenerator = new Random();
        }

        /// <summary>
        /// Initialize new instance with random seed
        /// </summary>
        /// <param name="size">Size of data in bytes that stream will allow to read before reporting end of stream</param>
        public NoiseStreamGenerator(long size)
            : this(size, SeedGenerator.Next())
        {
        }

        /// <summary>
        /// Initialize new instance with custom seed
        /// </summary>
        /// <param name="size">Size of data in bytes that stream will allow to read before reporting end of stream</param>
        /// <param name="seed">seed to use in System.Random when noise data is generated</param>
        public NoiseStreamGenerator(long size, int seed)
        {
            _size = size;
            _noiseGenerator = new Random(seed);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long bytesLeftToReadTotal = _size - _position;

            if (bytesLeftToReadTotal == 0)
                return 0;

            int bytesToRead = (int)Math.Min(bytesLeftToReadTotal, count);
            for (int i = 0; i < bytesToRead; i++)
            {
                buffer[i + offset] = (byte)_noiseGenerator.Next();
            }

            _position += bytesToRead;

            return bytesToRead;
        }
    }
}