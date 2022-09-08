using System;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Streams.Mocks
{
    /// <summary>
    /// Write-only stream which writes everything into nothingness (dev/null).
    /// It also advances the Position property to allow keeping track of the total amount if bytes written.
    /// </summary>
    public class NullOutputStream : UnbufferedWriteOnlyStreamBase
    {
        /// <summary>
        /// Indicates if stream is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        private long _bytesWritten = 0; 

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(StreamGenerator));

            _bytesWritten += count;
        }

        /// <inheritdoc />
        public override long Position
        {
            get => _bytesWritten;
            set => throw new NotImplementedException();
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