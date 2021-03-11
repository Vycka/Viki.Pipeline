using System.IO;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Pipes;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class ConsumerStreamAdapterTests
    {
        [Test]
        public void HappyFlow()
        {
            BatchingPipe<Packet> packetsPipe = new BatchingPipe<Packet>();

            _ = packetsPipe.ProduceCompleteAsync(FixedTestData.CreatePackets());

            Stream sut = packetsPipe.ToReadOnlyStream();

            FixedTestData.AssertStream(sut);
        }
    }
}