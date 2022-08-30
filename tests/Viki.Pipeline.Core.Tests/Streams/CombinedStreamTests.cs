using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Base;
using Viki.Pipeline.Core.Streams.Interfaces;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class CombinedStreamTests
    {
        [Test]
        public void HappyFlow()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedSyncOnlyStream sut = new CombinedSyncOnlyStream(FixedTestData.CreateStreams(checkDisposeStream));

            FixedTestData.AssertStream(sut, FixedTestData.Structure);

            Assert.IsTrue(checkDisposeStream.DisposeCalled);
        }
        [Test]
        public async Task DisposeAsyncOnReadWorks()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(checkDisposeStream);

            await sut.DisposeAsync();

            Assert.IsTrue(checkDisposeStream.DisposeAsyncCalled);
            Assert.IsFalse(checkDisposeStream.DisposeCalled);
        }

        [Test]
        public void DisposeOnReadWorks()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(checkDisposeStream);

            sut.Dispose();

            Assert.IsFalse(checkDisposeStream.DisposeAsyncCalled);
            Assert.IsTrue(checkDisposeStream.DisposeCalled);
        }


        [Test]
        public void DisposableBagWorks()
        {
            CheckDisposeStream checkDispose = new CheckDisposeStream();

            CombinedSyncOnlyStream sut = new CombinedSyncOnlyStream();
            ((IDisposablesBag)sut).AddDisposable(checkDispose);

            sut.Dispose();

            Assert.IsTrue(checkDispose.DisposeCalled);
        }

        [Test]
        public async Task DisposeAsyncWorks()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();
            CheckDisposeStream checkDisposeBag = new CheckDisposeStream();

            CombinedSyncOnlyStream sut = new CombinedSyncOnlyStream(checkDisposeStream);
            ((IDisposablesBag)sut).AddDisposable(checkDisposeBag);

            await sut.DisposeAsync();

            Assert.IsTrue(checkDisposeStream.DisposeCalled);
            Assert.IsTrue(checkDisposeBag.DisposeCalled);
        }

        [Test]
        public void EmptyStream()
        {
            byte[] buffer = new byte[16];

            CombinedSyncOnlyStream sut = new CombinedSyncOnlyStream();
            Assert.AreEqual(0, sut.Read(buffer, 0, buffer.Length));
        }

        private class CheckDisposeStream : UnbufferedReadOnlyStreamBase
        {
            public bool DisposeCalled { get; private set; } = false;
            public bool DisposeAsyncCalled { get; private set; } = false;

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            protected override void Dispose(bool disposing)
            {
                DisposeCalled = true;
            }

            public override ValueTask DisposeAsync()
            {
                DisposeAsyncCalled = true;
                return default;
            }
        }
    }
}