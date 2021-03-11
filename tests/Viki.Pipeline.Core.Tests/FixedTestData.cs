﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;
using Viki.Pipeline.Core.Streams.Components;

namespace Viki.Pipeline.Core.Tests
{
    public class FixedTestData
    {
        // 100+ MB of test data in scenario.
        // Reading byte by byte was slow.
        // thus extra complexity below to use byte arrays.

        public static List<Tuple<byte, int>> Structure = new List<Tuple<byte, int>>()
        {
            new Tuple<byte, int>((byte)'E', 100645000),
            new Tuple<byte, int>((byte)'A', 200004560),
            new Tuple<byte, int>((byte)'D', 100000045),
            new Tuple<byte, int>((byte)'G', 200500000),
            new Tuple<byte, int>((byte)'B', 100060000),
            new Tuple<byte, int>((byte)'e', 300007000),
        };

        public static Stream CreateStream() => new CombinedStream(CreateStreams());

        public static Stream[] CreateStreams(params Stream[] additionalStrams) => Structure
            .Select(s => new StreamGenerator(s.Item2, s.Item1))
            .Concat(additionalStrams)
            .ToArray();

        public static async IAsyncEnumerable<Packet> CreatePackets()
        {
            byte[] trashArray = new byte[3];

            foreach (Tuple<byte, int> scenario in Structure)
            {
                int splitBytes = scenario.Item2 / 6;
                int modBytes = scenario.Item2 % 6;

                yield return await Packet.ReadFrom(new StreamGenerator(splitBytes * 3, scenario.Item1));
                yield return await Packet.ReadFrom(new StreamGenerator(splitBytes * 2, scenario.Item1));
                yield return await Packet.ReadFrom(new StreamGenerator(splitBytes * 1, scenario.Item1));
                yield return await Packet.ReadFrom(new StreamGenerator(modBytes, scenario.Item1));
                yield return new Packet(trashArray, 0, NullArrayPool.Instance);
            }
        }


        public static void AssertStream(Stream stream)
        {
            AssertStream(stream, Structure);
        }

        public static void AssertStream(Stream stream, List<Tuple<byte, int>> structure)
        {
            try
            {
                int totalBytesRead = 0;
                byte[] buffer = new byte[85000];

                int bytesRead = stream.Read(buffer, 0, 85000);
                totalBytesRead += bytesRead;

                int currentSymbolCount = 0;
                int currentScenario = 0;

                while (bytesRead > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        int currentByte = buffer[i];

                        if (currentByte == structure[currentScenario].Item1)
                        {
                            currentSymbolCount++;
                        }
                        else
                        {
                            Assert.AreEqual(structure[currentScenario].Item2, currentSymbolCount, $"Incorrect symbol [{(char)structure[currentScenario].Item1}] count in data");

                            currentScenario++;
                            currentSymbolCount = 1;
                        }
                    }

                    bytesRead = stream.Read(buffer, 0, 85000);
                    totalBytesRead += bytesRead;
                }


                Assert.AreEqual(structure[currentScenario].Item2, currentSymbolCount, "Incorrect symbol count in data");
                Assert.AreEqual(structure.Sum(s => s.Item2), totalBytesRead, "Incorrect total length");
            }
            finally
            {
                stream.Dispose();
            }

        }

        public static void DebugStream(Stream stream)
        {
            int totalBytesRead = 0;
            byte[] buffer = new byte[85000];

            int bytesRead = stream.Read(buffer, 0, 85000);
            totalBytesRead += bytesRead;

            int currentSymbolCount = 0;

            int countedByte = -1;
            while (bytesRead > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    int currentByte = buffer[i];

                    if (currentByte == countedByte)
                    {
                        currentSymbolCount++;
                    }
                    else
                    {
                        string byteName = countedByte == -1 ? "-1" : $"{(char)countedByte}";

                        TestContext.WriteLine($"{byteName}: {currentSymbolCount}");
                        countedByte = currentByte;
                        currentSymbolCount = 1;
                    }
                }

                bytesRead = stream.Read(buffer, 0, 85000);
                totalBytesRead += bytesRead;
            }

            TestContext.WriteLine($"Total: {totalBytesRead}");
        }
    }
}