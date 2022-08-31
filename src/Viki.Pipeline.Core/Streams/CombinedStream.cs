using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Streams
{
    /// <summary>
    /// CombinedStream exposes streams through both sync/async at the same time.
    /// As result, no IDisposableBags functionality here. it will be replaced with something else more cooler later.
    /// // TODO: Extend disposing logic where dispose-handler would get passed.. allowing to replace IDisposablesBag and "disposeStreams" and additionally.. synchronize disposes if needed
    /// </summary>
    public class CombinedStream : UnbufferedReadOnlyStreamBase
    {
        private readonly bool _disposeStreams;

        private readonly IEnumerable<Stream> _streams;

        private IEnumerator<Stream> _enumerator;
        private bool _streamAvailable;

        /// <summary>
        /// Indicates if stream is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Array most not contain any null values. All streams in will be disposed.</param>
        public CombinedStream(params Stream[] streams)
            : this(streams, true)
        {
        }

        
        /// <summary>
        /// Create new instance of CombinedStream.
        /// !!! disposeStreams functionallity is temporary and planned to be refactored/broken/redesigned. feature won't disappear. but instead it will be presented in different-more-flexible form.
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed)</param>
        /// <param name="disposeStreams">Dispose passed streams</param>
        public CombinedStream(IEnumerable<Stream> streams, bool disposeStreams = true)
        {
            _disposeStreams = disposeStreams;
            _streams = streams ?? throw new ArgumentNullException(nameof(streams));
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CombinedStream));

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
        /// <inheritdoc />
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CombinedStream));

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
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                EnsureEnumeratorInitialized();

                while (IsEnumeratorStreamAvailable())
                {
                    HandleStreamDisposing(GetEnumeratorCurrent());
                    AdvanceEnumerator();
                }

                _enumerator.Dispose();

                base.Dispose(disposing);
            }
            else
            {
                throw new ObjectDisposedException(nameof(CombinedStream));
            }
        }

        /// <inheritdoc />
        public override ValueTask DisposeAsync()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                List<Task> disposeTasks = new List<Task>();

                EnsureEnumeratorInitialized();

                while (IsEnumeratorStreamAvailable())
                {
                    ValueTask disposeTask = HandleStreamDisposingAsync(GetEnumeratorCurrent());
                    disposeTasks.Add(disposeTask.AsTask());

                    AdvanceEnumerator();
                }

                _enumerator.Dispose();

                //return base.DisposeAsync();
                //// kinda still have to call base so it will support properly all the legacy who relies on Close() and so on..
                return new ValueTask(Task.WhenAll(disposeTasks.ToArray()));
            }
            else
            {
                throw new ObjectDisposedException(nameof(CombinedStream));
            }

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
    }
}
