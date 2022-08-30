using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class CombinedAsyncStreamTests
    {
        [Test]
        public void HappyFlow()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedAsyncOnlyStream sut = new CombinedAsyncOnlyStream(FixedTestData.CreateStreamsAsyncEnumerable(checkDisposeStream));

            FixedTestData.AssertStream(sut, FixedTestData.Structure);

            Assert.IsTrue(checkDisposeStream.DisposeCalled);
        }

        [Test]
        public void SyncDisposeTriggersAsyncDispose()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedAsyncOnlyStream sut = new CombinedAsyncOnlyStream(FixedTestData.CreateStreamsAsyncEnumerable(checkDisposeStream));

            sut.Dispose();
            
            Assert.IsTrue(checkDisposeStream.DisposeCalled);
        }

        [Test]
        public async Task MultipleDisposeCallsDontBreak()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedAsyncOnlyStream sut = new CombinedAsyncOnlyStream(FixedTestData.CreateStreamsAsyncEnumerable(checkDisposeStream));

            await sut.DisposeAsync();
            sut.Dispose();
            sut.Dispose();
            await sut.DisposeAsync();

            Assert.IsTrue(checkDisposeStream.DisposeCalled);
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