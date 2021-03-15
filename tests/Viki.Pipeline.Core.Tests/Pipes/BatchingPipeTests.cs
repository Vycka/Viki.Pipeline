using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Pipes;

namespace Viki.Pipeline.Core.Tests.Pipes
{
    [TestFixture]
    public class BatchingPipeTests
    {

        [Test(Description = "Move 500m of values and check if order is preserved, (child thread produces, test thread consumes)")]
        public void HappyFlow()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Could do more but since reader thread is slower. this test alone can eat up to ~4GB+ of RAM.
            
            long expectPayloadCount = 500000000; // 5B

            TestContext.WriteLine($"Pipe allocation {stopwatch.Elapsed}");
            IPipe<long> sut = new BatchingPipe<long>(100000000);
            TestContext.WriteLine($"Pipe allocation end {stopwatch.Elapsed}");
            
            Task producingTask = Task.Run(async () =>
            {
                TestContext.WriteLine($"Producer started {stopwatch.Elapsed}");

                for (long i = 0; i < expectPayloadCount; i++)
                {
                    // Since producing thread in sterile environment will be faster, we need to throttle it a bit.
                    // Without it and with scenarios like 5 billion of items, this test can hit 40GB+ of used RAM quite fast.
                    // TODO: Create test which batch reads so consumer will be faster than producer.

                    // with 100m limit this should keep memory usage below 1GB.
                    if (sut.BufferedItems >= 100000000) 
                        await Task.Delay(1);

                    sut.Produce(i);
                }

                sut.ProducingCompleted();

                TestContext.WriteLine($"Producer completed {stopwatch.Elapsed}");
            });


            TestContext.WriteLine($"Consumer started {stopwatch.Elapsed}");

            long expectedNextValue = 0;
            foreach (long actualValue in sut.ToEnumerable())
            {
                // Tried using NUnit assert first - Assert.AreEqual(expectedNextValue++, actualValue)
                // But it introduced 99%+ of cpu overhead making this test run for minutes instead of seconds.
                if (expectedNextValue++ != actualValue) 
                {
                    Assert.Fail($"Order was not preserved e:{expectedNextValue-1} a:{actualValue}");
                }
            }

            Assert.IsTrue(producingTask.IsCompletedSuccessfully);

            Assert.IsTrue(producingTask.IsCompletedSuccessfully);
            Assert.AreEqual(expectPayloadCount, expectedNextValue);

            TestContext.WriteLine($"Consumer ended {stopwatch.Elapsed}");
        }
    }
}