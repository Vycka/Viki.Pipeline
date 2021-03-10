using System;
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

            long expectPayloadCount = 500000000; // 500m

            IPipe<long> sut = new BatchingPipe<long>(1000000);

            Task producingTask = Task.Run(() =>
            {
                TestContext.WriteLine($"Producer started {stopwatch.Elapsed}");

                for (long i = 0; i < expectPayloadCount; i++)
                {
                    sut.Produce(i);
                }

                sut.ProducingCompleted();

                TestContext.WriteLine($"Producer completed {stopwatch.Elapsed}");
            });


            TestContext.WriteLine($"Consumer started {stopwatch.Elapsed}");

            int expectedNextValue = 0;
            foreach (long actualValue in sut.ToEnumerable())
            {
                // Tried using NUnit assert first - Assert.AreEqual(expectedNextValue++, actualValue)
                // But it introduced 99%+ of cpu overhead making this test run for minutes instead of seconds.
                if (expectedNextValue++ != actualValue) 
                {
                    Assert.Fail("Order was not preserved");
                }
            }

            Assert.IsTrue(producingTask.IsCompletedSuccessfully);
            Assert.AreEqual(expectPayloadCount, expectedNextValue);

            TestContext.WriteLine($"Consumer ended {stopwatch.Elapsed}");
        }
    }
}