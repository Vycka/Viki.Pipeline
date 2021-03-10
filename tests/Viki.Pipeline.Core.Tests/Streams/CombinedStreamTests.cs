using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Base;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class CombinedStreamTests
    {
        [Test]
        public void HappyFlow()
        {
            // 100+ MB of test data in scenario.
            // Reading byte by byte was slow.
            // thus extra complexity below to use byte arrays.

            List<Tuple<byte, int>> scenarioData = new List<Tuple<byte, int>>()
            {
                new Tuple<byte, int>((byte)'E', 100645000),
                new Tuple<byte, int>((byte)'A', 200004560),
                new Tuple<byte, int>((byte)'D', 100000045),
                new Tuple<byte, int>((byte)'G', 200500000),
                new Tuple<byte, int>((byte)'B', 100060000),
                new Tuple<byte, int>((byte)'e', 300007000),
            };

            Stream[] scenarioStreams = scenarioData.Select(s => new StreamGenerator(s.Item2, s.Item1)).Cast<Stream>().ToArray();
            CheckDisposeStream checkDisposeStream = new CheckDisposeStream();

            CombinedStream sut = new CombinedStream(scenarioStreams.Concat(new [] { checkDisposeStream }));

            int totalBytesRead = 0;
            byte[] buffer = new byte[85000];

            int bytesRead = sut.Read(buffer, 0, 85000);
            totalBytesRead += bytesRead;

            int currentSymbolCount = 0;
            int currentScenario = 0;

            while (bytesRead > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    int currentByte = buffer[i];
                    
                    if (currentByte == scenarioData[currentScenario].Item1)
                    {
                        currentSymbolCount++;
                    }
                    else
                    {
                        Assert.AreEqual(scenarioData[currentScenario].Item2, currentSymbolCount);

                        currentScenario++;
                        currentSymbolCount = 1;
                    }
                }

                bytesRead = sut.Read(buffer, 0, 85000);
                totalBytesRead += bytesRead;
            }


            Assert.AreEqual(scenarioData[currentScenario].Item2, currentSymbolCount);
            Assert.AreEqual(0, bytesRead);
            Assert.AreEqual(scenarioData.Sum(s => s.Item2), totalBytesRead);
            Assert.IsTrue(checkDisposeStream.DisposeCalled);
        }

        private class CheckDisposeStream : UnbufferedReadOnlyStreamBase
        {
            public bool DisposeCalled { get; private set; } = false;

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            protected override void Dispose(bool disposing)
            {
                DisposeCalled = true;
            }
        }
    }
}