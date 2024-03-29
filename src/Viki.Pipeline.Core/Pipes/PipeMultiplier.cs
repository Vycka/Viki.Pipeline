﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Viki.Pipeline.Core.Interfaces;

namespace Viki.Pipeline.Core.Pipes
{
    public class PipeMultiplier<T> : IProducer<T>, IEnumerable<IConsumer<T>>
    {
        private readonly IPipe<T>[] _pipes;

        public int Count => _pipes.Length;

        public PipeMultiplier(int consumerCount)
        {
            if (consumerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(consumerCount));

            _pipes = new BatchingPipe<T>[consumerCount];
            for (int i = 0; i < consumerCount; i++)
            {
                _pipes[i] = new BatchingPipe<T>();
            }
        }

        public PipeMultiplier(params IPipe<T>[] producers)
        {
            if (producers?.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(producers));

            _pipes = producers.ToArray();
        }

        /// <inheritdoc />
        public void Produce(T item)
        {
            for (int i = 0; i < _pipes.Length; i++)
            {
                _pipes[i].Produce(item);
            }
        }

        /// <inheritdoc />
        public void Produce(IEnumerable<T> items)
        {
            T[] itemsArray = items.ToArray();

            for (int i = 0; i < _pipes.Length; i++)
            {
                _pipes[i].Produce(itemsArray);
            }
        }

        /// <inheritdoc />
        public void ProducingCompleted()
        {
            for (int i = 0; i < _pipes.Length; i++)
            {
                _pipes[i].ProducingCompleted();
            }
        }

        public IConsumer<T> this[int index] => _pipes[index];

        /// <inheritdoc />
        public IEnumerator<IConsumer<T>> GetEnumerator()
        {
            return _pipes.Cast<IConsumer<T>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _pipes.GetEnumerator();
        }

        /// <inheritdoc />
        public long BufferedItems => _pipes.Sum(p => p.BufferedItems);
    }
}