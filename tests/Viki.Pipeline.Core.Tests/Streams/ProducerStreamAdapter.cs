using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Packets;
using Viki.Pipeline.Core.Pipes;
using Viki.Pipeline.Core.Streams;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class ProducerStreamAdapter
    {
        [Test]
        public void HappyFlow()
        {
            Stream testData = new CombinedSyncOnlyStream(FixedTestData.CreateStreams());

            BatchingPipe<Packet<byte>> packetsPipe = new BatchingPipe<Packet<byte>>();
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