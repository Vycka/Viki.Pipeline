using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Pipes;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class DuplicateStreamTests
    {
        [Test]
        public async Task HappyFlow()
        {
            Stream inputStream = FixedTestData.CreateStream();
            IPipe<Packet> duplicateBuffer = new BatchingPipe<Packet>();

            DuplicateStream sut = new DuplicateStream(inputStream, duplicateBuffer.ToWriteOnlyStream());

            // As sut is read for assertion, in the process it should start to write data to duplication destination
            Task assertSutStream = Task.Run(() => FixedTestData.AssertStream(sut));
            
            // Thus with next task we should be able to assert both streams at the same time.
            Task assertDuplicatedStream = Task.Run(() => FixedTestData.AssertStream(duplicateBuffer.ToReadOnlyStream()));

            await assertSutStream;
            await assertDuplicatedStream;
        }
    }
}