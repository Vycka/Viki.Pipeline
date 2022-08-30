using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Streams
{
    /// <summary>
    /// CombinedSyncOnlyStream reads all provided streams only through Read() (no async)
    /// </summary>
    public class CombinedStream : UnbufferedReadOnlyStreamBase
    {
        private readonly bool _disposeStreams;

        private readonly IEnumerable<Stream> _streams;

        private IEnumerator<Stream> _enumerator;
        private bool _streamAvailable;

        private bool _disposed = false;

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Array most not contain any null values. All streams in will be disposed.</param>
        public CombinedStream(params Stream[] streams)
            : this(streams, true)
        {
        }

        // TODO: Extend disposing logic where disposehandler would get passed.. allowing to replace IDisposablesBag and additionally.. synchronize disposes if needed
        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed)</param>
        /// <param name="disposeStreams">Dispose passed streams</param>
        public CombinedStream(IEnumerable<Stream> streams, bool disposeStreams = true)
        {
            _disposeStreams = disposeStreams;
            _streams = streams ?? throw new ArgumentNullException(nameof(streams));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureEnumeratorInitialized();

            int bytesRead = 0;

            while (bytesRead == 0 && IsEnumeratorStreamAvailable())
            {
                Stream currentStream = GetEnumeratorCurrent();
                try
                {
                    bytesRead = currentStream.Read(buffer, offset, count);
                }
                catch (ObjectDisposedException)
                {
                }

                if (bytesRead == 0)
                {
                    HandleStreamDisposing(currentStream);
                    AdvanceEnumerator();
                }
            }

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureEnumeratorInitialized();

            int bytesRead = 0;

            while (bytesRead == 0 && IsEnumeratorStreamAvailable())
            {
                Stream currentStream = GetEnumeratorCurrent();
                try
                {
                    bytesRead = await currentStream.ReadAsync(buffer, offset, count, cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                }

                if (bytesRead == 0)
                {
                    await HandleStreamDisposingAsync(currentStream);
                    AdvanceEnumerator();
                }
            }

            return bytesRead;
        }

        // Called from Stream's base DisposeAsync()->Close()->Dispose()->[Dispose(bool disposing)]
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                EnsureEnumeratorInitialized();

                while (IsEnumeratorStreamAvailable())
                {
                    HandleStreamDisposing(GetEnumeratorCurrent());
                    AdvanceEnumerator();
                }

                _enumerator.Dispose();

                base.Dispose(disposing);
            }
        }

        public override ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;

                EnsureEnumeratorInitialized();

                while (IsEnumeratorStreamAvailable())
                {
                    HandleStreamDisposingAsync(GetEnumeratorCurrent());
                    AdvanceEnumerator();
                }

                _enumerator.Dispose();

                return base.DisposeAsync();
            }

            return default;
        }

        private void HandleStreamDisposing(Stream stream)
        {
            if (_disposeStreams)
            {
                try
                {
                    stream?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }
        private ValueTask HandleStreamDisposingAsync(Stream stream)
        {
            ValueTask result = default;
            if (_disposeStreams && stream != null)
            {
                try
                {
                    result = stream.DisposeAsync();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            return result;
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

        public void AddDisposable(Stream disposable)
        {
            throw new NotImplementedException();
        }
    }
}
