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
        private readonly Stream _duplicateTo;

        public DuplicateStream(Stream source, Stream duplicateTo)
        {
            _source = source;
            _duplicateTo = duplicateTo;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = _source.Read(buffer, offset, count);

            if (result > 0)
                _duplicateTo.Write(buffer, offset, result);

            return result;
        }
    }
}