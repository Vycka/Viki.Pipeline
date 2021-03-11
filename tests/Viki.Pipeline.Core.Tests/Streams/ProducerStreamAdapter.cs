using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Pipes;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class ProducerStreamAdapter
    {
        [Test]
        public void HappyFlow()
        {
            Stream testData = new CombinedStream(FixedTestData.CreateStreams());

            BatchingPipe<Packet> packetsPipe = new BatchingPipe<Packet>();
            Stream sut = packetsPipe.ToWriteOnlyStream();

            Task.Run(async () =>
            {
                await testData.CopyToAsync(sut);
                await sut.DisposeAsync(); // Closing/Disposing adapter triggers completion of producer by default.
            });
            

            FixedTestData.AssertStream(packetsPipe.ToReadOnlyStream());
            //FixedTestData.DebugStream(packetsPipe.ToReadOnlyStream());
        }
    }
}