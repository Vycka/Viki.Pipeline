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

            CombinedStream sut = new CombinedStream(FixedTestData.CreateStreams(checkDisposeStream));

            FixedTestData.AssertStream(sut, FixedTestData.Structure);

            Assert.IsTrue(checkDisposeStream.DisposeCalled);
        }

        [Test]
        public void DisposableBagWorks()
        {
            CheckDisposeStream checkDispose = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream();
            ((IDisposablesBag)sut).AddDisposable(checkDispose);

            sut.Dispose();

            Assert.IsTrue(checkDispose.DisposeCalled);
        }

        [Test]
        public async Task DisposeAsyncWorks()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();
            CheckDisposeStream checkDisposeBag = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(checkDisposeStream);
            ((IDisposablesBag)sut).AddDisposable(checkDisposeBag);

            await sut.DisposeAsync();

            Assert.IsTrue(checkDisposeStream.DisposeCalled);
            Assert.IsTrue(checkDisposeBag.DisposeCalled);
        }

        [Test]
        public void EmptyStream()
        {
            byte[] buffer = new byte[16];

            CombinedStream sut = new CombinedStream();
            Assert.AreEqual(0, sut.Read(buffer, 0, buffer.Length));
        }

        private class CheckDisposeStream : UnbufferedReadOnlyStreamBase
        {
            public bool DisposeCalled { get; private set; } = false;

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            protected override void Dispose(bool disposing)
            {
                DisposeCalled = true;
            }
        }
    }
}