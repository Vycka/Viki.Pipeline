using System.IO;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Packets;
using Viki.Pipeline.Core.Pipes;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class ConsumerStreamAdapterTests
    {
        [Test]
        public void HappyFlow()
        {
            BatchingPipe<Packet<byte>> packetsPipe = new BatchingPipe<Packet<byte>>();

            _ = packetsPipe.ProduceCompleteAsync(FixedTestData.CreatePackets());

            Stream sut = packetsPipe.ToReadOnlyStream();

            FixedTestData.AssertStream(sut);
        }
    }
}