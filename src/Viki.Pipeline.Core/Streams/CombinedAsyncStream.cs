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
    public class CombinedAsyncStream : UnbufferedReadOnlyStreamBase, IDisposablesBag
    {
        private readonly Stack<IDisposable> _disposables;

        private readonly bool _disposeStreams;
        private readonly IAsyncEnumerable<Stream> _streams;

        private IAsyncEnumerator<Stream> _enumerator;
        private bool _streamAvailable;

        /// <summary>
        /// Create new instance of CombinedStream
        /// </summary>
        /// <param name="streams">Streams to be read from. Enumerable most not contain any nulls. (Enumerable will be iterated only as needed)</param>
        /// <param name="disposeStreams">Dispose passed streams</param>
        public CombinedAsyncStream(IAsyncEnumerable<Stream> streams, bool disposeStreams = true)
        {
            _disposeStreams = disposeStreams;
            _streams = streams ?? throw new ArgumentNullException(nameof(streams));

            _disposables = new Stack<IDisposable>();
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            Task<int> readTask = ReadAsync(buffer, offset, count, CancellationToken.None);
            Task.WaitAll(readTask);

            return readTask.Result;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await EnsureEnumeratorInitialized(cancellationToken).ConfigureAwait(false);

            int bytesRead = 0;

            while (bytesRead == 0 && IsEnumeratorStreamAvailable())
            {
                Stream currentStream = GetEnumeratorCurrent();
                bytesRead = await currentStream.ReadAsync(buffer, offset, count, cancellationToken);

                if (bytesRead == 0)
                {
                    HandleStreamDisposing(currentStream);
                    await AdvanceEnumerator();
                }
            }

            return bytesRead;
        }

        public override async ValueTask DisposeAsync()
        {
            await EnsureEnumeratorInitialized(CancellationToken.None);

            while (IsEnumeratorStreamAvailable())
            {
                HandleStreamDisposing(GetEnumeratorCurrent());
                await AdvanceEnumerator();
            }

            await _enumerator.DisposeAsync();

            // IDisposablesBag part
            while (_disposables.Count != 0)
            {
                _disposables.Pop()?.Dispose();
            }
        }


        private void HandleStreamDisposing(Stream stream)
        {
            if (_disposeStreams)
            {
                stream?.Dispose();
            }
        }

        private async Task EnsureEnumeratorInitialized(CancellationToken token)
        {
            if (_enumerator == null)
            {
                _enumerator = _streams.GetAsyncEnumerator(token);
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
        void IDisposablesBag.AddDisposable(IDisposable disposable)
        {
            _disposables.Push(disposable);
        }
    }
}