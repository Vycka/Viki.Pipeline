using System;
using System.Collections.Generic;
using System.IO;
using Viki.Pipeline.Core.Streams.Base;
using Viki.Pipeline.Core.Streams.Interfaces;

namespace Viki.Pipeline.Core.Streams
{
    // Took idea from https://stackoverflow.com/questions/3879152/how-do-i-concatenate-two-system-io-stream-instances-into-one
    // Made my version of it.
    public class CombinedStream : UnbufferedReadOnlyStreamBase, IDisposablesBag
    {
        private readonly Stack<IDisposable> _disposables;

        private readonly bool _disposeStreams;
        private readonly IEnumerable<Stream> _streams;

        private IEnumerator<Stream> _enumerator;
        private bool _streamAvailable;

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed)</param>
        /// <param name="disposeStreams">Dispose passed streams</param>
        public CombinedStream(IEnumerable<Stream> streams, bool disposeStreams = true)
        {
            _disposeStreams = disposeStreams;
            _streams = streams ?? throw new ArgumentNullException(nameof(streams));

            _disposables = new Stack<IDisposable>();
        }

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Array most not contain any null values. All streams in will be disposed.</param>
        public CombinedStream(params Stream[] streams)
            : this(streams, true)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureEnumeratorInitialized();

            int bytesRead = 0;

            while (bytesRead == 0 && IsEnumeratorStreamAvailable())
            {
                Stream currentStream = GetEnumeratorCurrent();
                bytesRead = currentStream.Read(buffer, offset, count);

                if (bytesRead == 0)
                {
                    HandleStreamDisposing(currentStream);
                    AdvanceEnumerator();
                }
            }

            return bytesRead;
        }
        
        // Called from Stream's base Dispose
        protected override void Dispose(bool disposing)
        {
            EnsureEnumeratorInitialized();

            while (IsEnumeratorStreamAvailable())
            {
                HandleStreamDisposing(GetEnumeratorCurrent());
                AdvanceEnumerator();
            }

            _enumerator.Dispose();

            // IDisposablesBag part
            while (_disposables.Count != 0)
            {
                _disposables.Pop()?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void HandleStreamDisposing(Stream stream)
        {
            if (_disposeStreams)
            {
                stream?.Dispose();
            }
        }

        private void EnsureEnumeratorInitialized()
        {
            if (_enumerator == null)
            {
                _enumerator = _streams.GetEnumerator();
                AdvanceEnumerator();
            }
        }

        private Stream GetEnumeratorCurrent()
        {
            return _enumerator.Current;
        }

        private bool AdvanceEnumerator()
        {
            _streamAvailable = _enumerator.MoveNext();
            return _streamAvailable;
        }

        private bool IsEnumeratorStreamAvailable()
        {
            return _streamAvailable;
        }

        /// <inheritdoc />
        void IDisposablesBag.AddDisposable(IDisposable disposable)
        {
            _disposables.Push(disposable);
        }
    }
}