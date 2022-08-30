using System.Threading.Tasks;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Tests.Mocks.Components
{
    public class CheckDisposeStream : StreamGenerator
    {
        public bool DisposeCalled { get; private set; } = false; // if AsyncDispose Gets called, this one will trigger too.
        public bool DisposeAsyncCalled { get; private set; } = false;


        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            DisposeAsyncCalled = true;

            return base.DisposeAsync();
        }

        public CheckDisposeStream(long size) : base(size, 0)
        {
        }

        public CheckDisposeStream() : base(0, 0)
        {
        }
    }
}