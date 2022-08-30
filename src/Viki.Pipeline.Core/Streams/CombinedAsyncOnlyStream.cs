using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Streams.Base;
using Viki.Pipeline.Core.Streams.Interfaces;

namespace Viki.Pipeline.Core.Streams
{
    // Took idea from https://stackoverflow.com/questions/3879152/how-do-i-concatenate-two-system-io-stream-instances-into-one
    // Made my version of it.

    /// <summary>
    /// AsyncOnlyStream reads all provided streams only through ReadAsync()
    /// </summary>
    public class CombinedAsyncOnlyStream : UnbufferedReadOnlyStreamBase, IAsyncDisposablesBag
    {
        /// <summary>
        /// Indicates if stream is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        private readonly Stack<IAsyncDisposable> _disposables;

        private readonly bool _disposeStreams;
        private readonly IAsyncEnumerable<Stream> _streams;

        private IAsyncEnumerator<Stream> _enumerator;
        private bool _streamAvailable;
        private CancellationTokenSource _enumeratorCancellationTokenSource;

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed)</param>
        /// <param name="disposeStreams">Dispose passed streams</param>
        public CombinedAsyncOnlyStream(IAsyncEnumerable<Stream> streams, bool disposeStreams = true)
        {
            _disposeStreams = disposeStreams;
            _streams = streams ?? throw new ArgumentNullException(nameof(streams));

            _disposables = new Stack<IAsyncDisposable>();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            Task<int> readTask = ReadAsync(buffer, offset, count, CancellationToken.None);
            Task.WaitAll(readTask);

            return readTask.Result;
        }

        /// <inheritdoc />
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CombinedSyncOnlyStream));

            try
            {
                await EnsureEnumeratorInitialized(cancellationToken);

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
                        await AdvanceEnumerator();
                    }
                }

                return bytesRead;
            }
            catch (OperationCanceledException)
            {
                _enumeratorCancellationTokenSource.Cancel();
                throw;
            }

        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DisposeAsyncInner().GetAwaiter().GetResult();
                base.Dispose(disposing);
            }
        }

        /// <inheritdoc />
        public override async ValueTask DisposeAsync()
        {
            if (!IsDisposed)
            {
                await DisposeAsyncInner();
                await base.DisposeAsync();
            }
        }

        private async ValueTask DisposeAsyncInner()
        {
            IsDisposed = true;

            await EnsureEnumeratorInitialized(CancellationToken.None);

            while (IsEnumeratorStreamAvailable())
            {
                await HandleStreamDisposingAsync(GetEnumeratorCurrent());
                await AdvanceEnumerator();
            }

            await _enumerator.DisposeAsync();

            // IDisposablesBag part
            while (_disposables.Count != 0)
            {
                IAsyncDisposable disposable = _disposables.Pop();
                if (disposable != null)
                {
                    await disposable.DisposeAsync();
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

        private async Task EnsureEnumeratorInitialized(CancellationToken token)
        {
            if (_enumerator == null)
            {
                _enumeratorCancellationTokenSource = new CancellationTokenSource();
                token.Register(() => _enumeratorCancellationTokenSource.Cancel());

                _enumerator = _streams.GetAsyncEnumerator(_enumeratorCancellationTokenSource.Token);

                await AdvanceEnumerator();
            }
        }

        private Stream GetEnumeratorCurrent()
        {
            return _enumerator.Current;
        }

        private async Task<bool> AdvanceEnumerator()
        {
            _streamAvailable = await _enumerator.MoveNextAsync();
            return _streamAvailable;
        }

        private bool IsEnumeratorStreamAvailable()
        {
            return _streamAvailable;
        }

        /// <inheritdoc />
        void IAsyncDisposablesBag.AddDisposable(IAsyncDisposable disposable)
        {
            _disposables.Push(disposable);
        }
    }
}