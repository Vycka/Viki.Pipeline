using System;
using System.IO;
using System.Threading.Tasks;
using Dasync.Collections;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class CombinedAsyncOnlyStreamTests
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
        public async Task AccessingDisposed()
        {
            CombinedAsyncOnlyStream sutA = new CombinedAsyncOnlyStream(AsyncEnumerable.Empty<Stream>());
            CombinedAsyncOnlyStream sutB = new CombinedAsyncOnlyStream(AsyncEnumerable.Empty<Stream>());

            sutA.Dispose();
            await sutB.DisposeAsync();

            Assert.DoesNotThrow(() => sutA.Dispose());
            Assert.DoesNotThrow(() => sutB.Dispose());

            Assert.DoesNotThrowAsync(async () => await sutA.DisposeAsync());
            Assert.DoesNotThrowAsync(async () => await sutB.DisposeAsync());

            byte[] buffer = new byte[1];

            Assert.Throws<ObjectDisposedException>(() => _ = sutA.Read(buffer, 0, 1));
            Assert.Throws<ObjectDisposedException>(() => _ = sutB.Read(buffer, 0, 1));

            Assert.ThrowsAsync<ObjectDisposedException>(async () => _ = await sutA.ReadAsync(buffer, 0, 1));
            Assert.ThrowsAsync<ObjectDisposedException>(async () => _ = await sutB.ReadAsync(buffer, 0, 1));
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