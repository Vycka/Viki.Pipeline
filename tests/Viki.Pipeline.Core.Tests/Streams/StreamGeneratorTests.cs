using System.Security.Cryptography;
using NUnit.Framework;
using Viki.Pipeline.Core.Streams;

namespace Viki.Pipeline.Core.Tests.Streams
{
    [TestFixture]
    public class StreamGeneratorTests
    {
        [Test]
        public void HappyFlow()
        {
            StreamGenerator sut = new StreamGenerator(12345, 'A');

            byte[] actual = StreamReader.ReadAll(sut);

            Assert.AreEqual(12345, actual.Length);
            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i] != 'A')
                    Assert.Fail("Incorrect data in the stream");
            }
        }
    }
}