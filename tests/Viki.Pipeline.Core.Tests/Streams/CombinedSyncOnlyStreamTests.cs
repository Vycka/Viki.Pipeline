using System;
using System.IO;
using System.Threading.Tasks;
using Dasync.Collections;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Interfaces;
using Viki.Pipeline.Core.Tests.Mocks.Components;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class CombinedSyncOnlyStreamTests
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

        [Test]
        public async Task AccessingDisposed()
        {
            CombinedSyncOnlyStream sutA = new CombinedSyncOnlyStream();
            CombinedSyncOnlyStream sutB = new CombinedSyncOnlyStream();

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
    }
}