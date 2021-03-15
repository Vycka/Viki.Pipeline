﻿using System.Collections.Generic;
using Viki.Pipeline.Core.Interfaces;

namespace Viki.Pipeline.Core.Pipes
{
    public class BatchingPipe<T> : IPipe<T>
    {
        private bool _completed;

        private List<T> _writeOnlyList, _readOnlyList;

        /// <summary>
        /// Ordered in-memory transport layer.
        /// </summary>
        /// <param name="initialCapacity">Initial WoS and RoS buffers capacity. Total allocated amount of buffer will be double as two lists are allocated.</param>
        public BatchingPipe(int? initialCapacity = null)
        {
            if (initialCapacity.HasValue)
            {
                _writeOnlyList = new List<T>(initialCapacity.Value);
                _readOnlyList = new List<T>(initialCapacity.Value);
            }
            else
            {
                _writeOnlyList = new List<T>();
                _readOnlyList = new List<T>();
            }
        }

        public bool Available => !_completed || _readOnlyList.Count != 0 || _writeOnlyList.Count != 0;

        public bool TryLockBatch(out ICollection<T> batch)
        {
            bool result = false;

            if (_writeOnlyList.Count != 0)
            {
                if (_readOnlyList.Count != 0)
                {
                    batch = _readOnlyList;
                    result = true;
                }
                else
                {
                    Flip();
                    batch = null;
                }
            }
            else if (_completed) // WoS == 0, Completed == TRUE
            {
                if (_readOnlyList.Count != 0)
                {
                    batch = _readOnlyList;
                    result = true;
                }
                else
                {
                    batch = null;
                }
            }
            else
            {
                batch = null;
            }

            return result;
        }

        public void ReleaseBatch()
        {
            _readOnlyList.Clear();

            Flip();
        }

        private void Flip()
        {
            var tmp = _readOnlyList;
            _readOnlyList = _writeOnlyList;
            _writeOnlyList = tmp;
        }

        public void Produce(T item)
        {
            _writeOnlyList.Add(item);
        }

        public void Produce(IEnumerable<T> item)
        {
            _writeOnlyList.AddRange(item);
        }

        public void ProducingCompleted()
        {
            _completed = true;
        }

        public long BufferedItems => _writeOnlyList.Count + _readOnlyList.Count;
    }
}