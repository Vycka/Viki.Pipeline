using System.Buffers;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Tests.Streams.Components
{
    [TestFixture]
    public class PacketTests
    {
        [Test]
        public void MultiDisposeCallHandled()
        {
            MockArrayPool mock = new MockArrayPool();

            Packet sut = new Packet(new byte[0], 0, mock);

            sut.Dispose();
            sut.Dispose();

            Assert.AreEqual(1, mock.ReturnCallCount);
        }

        private class MockArrayPool : ArrayPool<byte>
        {
            public int ReturnCallCount { get; private set; } = 0;
            public override byte[] Rent(int minimumLength)
            {
                throw new System.NotImplementedException();
            }

            public override void Return(byte[] array, bool clearArray = false)
            {
                ReturnCallCount++;
            }
        }
    }
}