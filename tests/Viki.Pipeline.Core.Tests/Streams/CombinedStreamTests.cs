using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Mocks;
using Viki.Pipeline.Core.Tests._TestData.Components;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class CombinedStreamTests
    {
        [Test]
        public void HappyFlowSync()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(FixedTestData.CreateStreams(checkDisposeStream));

            FixedTestData.AssertStream(sut, FixedTestData.Structure);

            Assert.IsTrue(checkDisposeStream.DisposeCalled);
            Assert.IsFalse(checkDisposeStream.DisposeAsyncCalled);
        }

        [Test]
        public async Task HappyFlowAsync()
        {
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(FixedTestData.CreateStreams(checkDisposeStream));

            await FixedTestData.AssertStreamAsync(sut, FixedTestData.Structure);

            Assert.IsTrue(checkDisposeStream.DisposeAsyncCalled);
        }

        [Test]
        public void NoStreamsSync()
        {
            byte[] buffer = new byte[16];

            CombinedStream sut = new CombinedStream();
            Assert.AreEqual(0, sut.Read(buffer, 0, buffer.Length));

            sut.Dispose();
        }

        [Test]
        public async Task NoStreamsAsync()
        {
            byte[] buffer = new byte[16];

            CombinedStream sut = new CombinedStream();
            Assert.AreEqual(0, await sut.ReadAsync(buffer, 0, buffer.Length));

            await sut.DisposeAsync();
        }


        [Test]
        public void EmptyStreamsSync()
        {
            byte[] buffer = new byte[16];

            CheckDisposeStream emptyStreamA = new CheckDisposeStream(0, TimeSpan.FromMilliseconds(300));
            CheckDisposeStream emptyStreamB = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(emptyStreamA, emptyStreamB);
            Assert.AreEqual(0, sut.Read(buffer, 0, buffer.Length));

            Assert.IsTrue(emptyStreamA.DisposeCalled);
            Assert.IsTrue(emptyStreamB.DisposeCalled);
            Assert.IsFalse(emptyStreamA.DisposeAsyncCalled);
            Assert.IsFalse(emptyStreamB.DisposeAsyncCalled);

            sut.Dispose();
        }

        [Test]
        public async Task EmptyStreamsAsync()
        {
            byte[] buffer = new byte[16];

            CheckDisposeStream emptyStreamA = new CheckDisposeStream(0, TimeSpan.FromMilliseconds(300));
            CheckDisposeStream emptyStreamB = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(emptyStreamA, emptyStreamB);
            Assert.AreEqual(0, await sut.ReadAsync(buffer, 0, buffer.Length));
            
            await sut.DisposeAsync();

            Assert.IsTrue(emptyStreamA.DisposeAsyncCalled);
            Assert.IsTrue(emptyStreamB.DisposeAsyncCalled);
        }

        [Test]
        public void DisposeUnreadsSync()
        {
            CheckDisposeStream streamA = new CheckDisposeStream(20);
            CheckDisposeStream streamB = new CheckDisposeStream(20, TimeSpan.FromMilliseconds(300));
            CombinedStream sut = new CombinedStream(streamA, streamB);

            sut.Dispose();
            
            Assert.IsTrue(streamA.DisposeCalled);
            Assert.IsTrue(streamB.DisposeCalled);
            Assert.IsFalse(streamA.DisposeAsyncCalled);
            Assert.IsFalse(streamB.DisposeAsyncCalled);
        }

        [Test]
        public async Task DisposeUnreadsAsync()
        {
            CheckDisposeStream streamA = new CheckDisposeStream(20);
            CheckDisposeStream streamB = new CheckDisposeStream(20, TimeSpan.FromMilliseconds(300));

            CombinedStream sut = new CombinedStream(streamA, streamB);

            await sut.DisposeAsync();

            Assert.IsFalse(streamA.DisposeCalled);
            Assert.IsFalse(streamB.DisposeCalled);
            Assert.IsTrue(streamA.DisposeAsyncCalled);
            Assert.IsTrue(streamB.DisposeAsyncCalled);
        }

        [Test]
        public void DisposePartiallyUnreadsSync()
        {
            byte[] buffer = new byte[30];

            CheckDisposeStream streamA = new CheckDisposeStream(20);
            CheckDisposeStream streamB = new CheckDisposeStream(20, TimeSpan.FromMilliseconds(300));

            CombinedStream sut = new CombinedStream(streamA, streamB);

            // We allow here for us to know how SUT behaves and write this weird but easy assert.
            // as otherwise, we would need to reinvent slimmer stream.CopyTo() extension
            Assert.AreEqual(20, sut.Read(buffer, 0, 30));
            Assert.AreEqual(10, sut.Read(buffer, 0, 10));

            Assert.IsTrue(streamA.DisposeCalled);
            Assert.IsFalse(streamB.DisposeCalled);

            sut.Dispose();

            Assert.IsFalse(streamA.DisposeAsyncCalled);
            Assert.IsTrue(streamB.DisposeCalled);

            Assert.IsFalse(streamB.DisposeAsyncCalled);
        }

        [Test]
        public async Task DisposePartiallyUnreadsAsync()
        {
            byte[] buffer = new byte[30];

            CheckDisposeStream streamA = new CheckDisposeStream(20);
            CheckDisposeStream streamB = new CheckDisposeStream(20, TimeSpan.FromMilliseconds(300));

            CombinedStream sut = new CombinedStream(streamA, streamB);

            // We allow here for us to know how SUT behaves and write this weird but easy assert.
            // as otherwise, we would need to reinvent slimmer stream.CopyToAsync() extension
            Assert.AreEqual(20, await sut.ReadAsync(buffer, 0, 30));
            Assert.AreEqual(10, await sut.ReadAsync(buffer, 0, 10));

            Assert.IsTrue(streamA.DisposeAsyncCalled);
            Assert.IsFalse(streamB.DisposeAsyncCalled);

            await sut.DisposeAsync();

            Assert.IsTrue(streamA.DisposeAsyncCalled);
            Assert.IsTrue(streamB.DisposeAsyncCalled);
        }

        [Test]
        public void DisposedStreamDoesntBreakSync()
        {
            StreamGenerator disposedStream = new StreamGenerator(0, 0);
            disposedStream.Dispose();
            StreamGenerator validStream = new StreamGenerator(1, 'A');

            CombinedStream sut = new CombinedStream(validStream, disposedStream);

            byte[] buffer = new byte[10];

            Assert.AreEqual(1, sut.Read(buffer, 0, 10));
            Assert.Throws<ObjectDisposedException>(() => _ = sut.Read(buffer, 0, 10));
            Assert.AreEqual('A', buffer[0]);
        }

        [Test]
        public async Task DisposedStreamDoesntBreakAsync()
        {
            StreamGenerator disposedStream = new StreamGenerator(0, 0);
            disposedStream.Dispose();
            StreamGenerator validStream = new StreamGenerator(1, 'A');

            CombinedStream sut = new CombinedStream(validStream, disposedStream);

            byte[] buffer = new byte[10];

            Assert.AreEqual(1, await sut.ReadAsync(buffer, 0, 10));
            Assert.ThrowsAsync<ObjectDisposedException>(async () => _ = await sut.ReadAsync(buffer, 0, 10));
            Assert.AreEqual('A', buffer[0]);
        }

        [Test]
        public async Task AccessingDisposed()
        {
            CombinedStream sutA = new CombinedStream();
            CombinedStream sutB = new CombinedStream();

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

        [Test]
        public void DisposeStreamsPropertyWorks()
        {
            throw new Exception("should cover this");
        }
    }
}