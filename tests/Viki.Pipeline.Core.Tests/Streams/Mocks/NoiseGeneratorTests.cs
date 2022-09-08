using System.Collections;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams.Mocks;

namespace Viki.Pipeline.Core.Tests.Streams.Mocks
{
    [TestFixture]
    public class NoiseGeneratorTests
    {
        [Test]
        public void HappyFlow()
        {
            NoiseStreamGenerator sutA1 = new NoiseStreamGenerator(1234567, 42);
            NoiseStreamGenerator sutA2 = new NoiseStreamGenerator(1234567, 42);
            NoiseStreamGenerator sutB1 = new NoiseStreamGenerator(123);
            NoiseStreamGenerator sutC1 = new NoiseStreamGenerator(123);

            byte[] actualA1 = StreamReader.ReadAll(sutA1);
            byte[] actualA2 = StreamReader.ReadAll(sutA2);
            byte[] actualB1 = StreamReader.ReadAll(sutB1);
            byte[] actualC1 = StreamReader.ReadAll(sutC1);

            bool areEqual = StructuralComparisons.StructuralEqualityComparer.Equals(actualA1, actualA2);
            Assert.IsTrue(areEqual, "Noise with same seed was different");

            bool defaultSeedRandomized = !StructuralComparisons.StructuralEqualityComparer.Equals(actualB1, actualC1);
            Assert.IsTrue(areEqual, "seed-less constructors returned same results both times");

            Assert.AreEqual(1234567, actualA1.Length);
            Assert.AreEqual(123, actualB1.Length);
            Assert.AreEqual(123, actualC1.Length);
        }
    }
}