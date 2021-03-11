using System.IO;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Streams
{
    /// <summary>
    /// Wraps passed stream exposing it as unbuffered readonly stream.
    /// while wrapped stream is being read, a copy of its data is pushed into passed writable stream.
    /// If write stream blocks, it will block read of current stream too.
    /// </summary>
    public class DuplicateStream : UnbufferedReadOnlyStreamBase
    {
        private readonly Stream _source;
        private readonly Stream _destination;
        private readonly bool _disposeSource;
        private readonly bool _disposeDestination;

        public DuplicateStream(Stream source, Stream destination, bool disposeSource = true, bool disposeDestination = true)
        {
            _source = source;
            _destination = destination;
            _disposeSource = disposeSource;
            _disposeDestination = disposeDestination;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = _source.Read(buffer, offset, count);

            if (result > 0)
                _destination.Write(buffer, offset, result);

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposeDestination)
            {
                _destination.Dispose();
            }

            if (_disposeSource)
            {
                _source.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}