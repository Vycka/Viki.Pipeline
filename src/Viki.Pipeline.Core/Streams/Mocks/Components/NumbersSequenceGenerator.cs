using System;

namespace Viki.Pipeline.Core.Streams.Mocks.Components
{
    public class NumbersSequenceGenerator : ISequenceGenerator
    {
        private readonly Random _rng;
        private readonly byte _separator;
        private readonly int _minLength, _maxLength;

        public NumbersSequenceGenerator(int seed = 42, int minLength = 1, int maxLength = 7, char separator = ',')
        {
            _rng = new Random(seed);
            _minLength = minLength;
            _maxLength = maxLength + 1; // Random().Next() behaves in [min, max)
            _separator = (byte)separator;
        }

        /// <inheritdoc />
        public byte NextSeparator() => _separator;

        /// <inheritdoc />
        public long NextElementLength() => _rng.Next(_minLength, _maxLength);

        /// <inheritdoc />
        public byte NextElementFirstByte() => (byte)_rng.Next('1', '9' + 1);

        /// <inheritdoc />
        public byte NextElementByte() => (byte)_rng.Next('0', '9' + 1);
    }
}