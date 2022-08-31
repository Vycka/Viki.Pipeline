using System;
using System.Threading;
using System.Threading.Tasks;
using Viki.Pipeline.Core.Streams;

namespace Viki.Pipeline.Core.Tests.Mocks.Components
{
    public class CheckDisposeStream : StreamGenerator
    {
        private readonly TimeSpan _blockingOnDispose;
        public bool DisposeCalled { get; private set; } = false;
        public bool DisposeAsyncCalled { get; private set; } = false;


        public CheckDisposeStream() 
            : this(0, TimeSpan.Zero)
        {
        }

        public CheckDisposeStream(TimeSpan blockingOnDispose)
            : this(0, blockingOnDispose)
        {
        }

        public CheckDisposeStream(long size) 
            : this(size, TimeSpan.Zero)
        {
        }

        public CheckDisposeStream(long size, TimeSpan blockingOnDispose) : base(size, 0)
        {
            _blockingOnDispose = blockingOnDispose;
        }



        protected override void Dispose(bool disposing)
        {

            Thread.Sleep(_blockingOnDispose);

            DisposeCalled = true;

            base.Dispose(disposing);
        }


        public override ValueTask DisposeAsync()
        {
            return new ValueTask(Task.Run(async () =>
            {
                await Task.Delay(_blockingOnDispose);
                DisposeAsyncCalled = true;
            }));
        }

    }
}