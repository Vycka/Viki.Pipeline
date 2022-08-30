using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class StreamGeneratorTests
    {
        [Test]
        public void HappyFlow()
        {
            StreamGenerator sut = new StreamGenerator(12345, 'A');

            byte[] actual = StreamReader.ReadAll(sut);

            Assert.AreEqual(12345, actual.Length);
            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i] != 'A')
                    Assert.Fail("Incorrect data in the stream");
            }
        }

        [Test]
        public async Task DisposedThrowsOnRead()
        {
            byte[] buffer = new byte[10];

            StreamGenerator sutA = new StreamGenerator(0, 0);
            sutA.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _ = sutA.Read(buffer, 0, 10));
            Assert.ThrowsAsync<ObjectDisposedException>(async () => _ = await sutA.ReadAsync(buffer, 0, 10));

            StreamGenerator sutB = new StreamGenerator(0, 0);
            await sutA.DisposeAsync();
            Assert.Throws<ObjectDisposedException>(() => _ = sutA.Read(buffer, 0, 10));
            Assert.ThrowsAsync<ObjectDisposedException>(async () => _ = await sutA.ReadAsync(buffer, 0, 10));
        }
    }
}