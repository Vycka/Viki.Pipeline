using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dasync.Collections;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Packets;
using Viki.Pipeline.Core.Pipes;

namespace Viki.Pipeline.Core.Tests.Pipes
{
    [TestFixture]
    public class BatchingPipeTests
    {
        private Stopwatch _stopwatch;

        [SetUp]
        public void Setup()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }


        [Test(Description = "Move 500m of values and check if order is preserved, (child thread produces, test thread consumes)")]
        public void HappyFlow()
        {

            // 500m on i9-9900 takes about 5-8secs.. bottle-necked by consumer thread. (Release build)
            long expectPayloadCount = 500000000;

            TestContext.WriteLine($"Pipe allocation {_stopwatch.Elapsed}");
            IPipe<long> sut = new BatchingPipe<long>(100000000);
            TestContext.WriteLine($"Pipe allocation end {_stopwatch.Elapsed}");
            
            // Limited buffer prevents this test eating more than ~1GB
            Task producingTask = RunProducingTask(sut, expectPayloadCount, 100000000);
            

            TestContext.WriteLine($"Consumer stating {_stopwatch.Elapsed}");
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
            Assert.AreEqual(expectPayloadCount, expectedNextValue);

            TestContext.WriteLine($"Consumer completed {_stopwatch.Elapsed}");
        }


        [Test(Description = "Same situation as default HappyFlow, with exception that many threads get to consume response to keep up with producer")]
        public async Task HappyFlowHeavyPooledBatches()
        {
            // 5B on i9-9900 takes about 25secs.. bottle-necked by producer thread. (Release build)
            long expectPayloadCount = 5000000000;

            TestContext.WriteLine($"Pipe allocation {_stopwatch.Elapsed}");
            IPipe<long> sut = new BatchingPipe<long>(100000000);
            TestContext.WriteLine($"Pipe allocation complete {_stopwatch.Elapsed}");

            // with unlimited buffer, test run was sitting between 3-16GB of ram usage.
            Task producingTask = RunProducingTask(sut, expectPayloadCount, Int32.MaxValue);
            
            

            TestContext.WriteLine($"Consumer stating {_stopwatch.Elapsed}");
            ConcurrentBag<BatchSummary> results = new ConcurrentBag<BatchSummary>();
            await sut
                .ToPacketsAsyncEnumerable()
                .ParallelForEachAsync((packet, index) =>
                {
                    try
                    {
                        results.Add(BatchSummary.Validate(packet, index));
                        return Task.CompletedTask;
                    }
                    finally
                    {
                        packet.Dispose();
                    }
                }
            );
            TestContext.WriteLine($"Consumer completed {_stopwatch.Elapsed}");

            Assert.IsTrue(producingTask.IsCompletedSuccessfully);

            BatchSummary[] resultsArray = results
                .Where(p => !p.Empty)
                .OrderBy(r => r.Index)
                .ToArray();

            Assert.Positive(resultsArray.Length);
            Assert.IsTrue(resultsArray.All(r => r.Valid));

            for (int i = 1; i < resultsArray.Length; i++)
            {
                if (resultsArray[i].First - 1 != resultsArray[i - 1].Last)
                {
                    Assert.Fail("Bad ordering detected");
                }
            }

            Assert.AreEqual(expectPayloadCount-1, resultsArray[^1].Last);
        }

        private Task RunProducingTask(IProducer<long> producer, long payloadCount, int bufferLimit)
        {
            return Task.Run(async () =>
            {
                TestContext.WriteLine($"Producer starting {_stopwatch.Elapsed}");

                for (long i = 0; i < payloadCount; i++)
                {
                    // By default producing thread in sterile environment will be faster, we need to throttle it a bit.
                    // Without the throttle in scenarios consisting of 5+ billion of items - this test can hit 40GB+ of used RAM quite fast.
                    // with 100m limit this should keep memory usage below 1GB.
                    if (producer.BufferedItems >= bufferLimit)
                        await Task.Delay(1);

                    producer.Produce(i);
                }

                producer.ProducingCompleted();

                TestContext.WriteLine($"Producer completed {_stopwatch.Elapsed}");
            });
        }

        private class BatchSummary
        {
            public long Index, First, Last;
            public bool Valid;
            public bool Empty;

            public static BatchSummary Validate(Packet<long> packet, long index)
            {
                BatchSummary result = new BatchSummary();

                result.Index = index;

                if (packet.DataLength != 0)
                {
                    result.First = packet.Data[0];
                    result.Last = packet.Data[packet.DataLength-1];
                    result.Valid = true;
                    

                    long current = result.First;
                    for (int i = 1; i < packet.DataLength; i++)
                    {
                        if (++current != packet.Data[i])
                        {
                            result.Valid = false;
                            break;
                        }
                    }
                }
                else
                {
                    result.Empty = true;
                    result.Valid = true;
                }

                return result;
            }
        }
    }
}