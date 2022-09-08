using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Streams.Mocks;

namespace Viki.Pipeline.Core.Tests.Streams.Mocks
{
    [TestFixture]
    public class NumbersGeneratorStreamTests
    {
        [Test]
        public async Task HappyFlow()
        {

            for (int i = 0; i < 30; i++)
            {
                await TestContext.Out.WriteLineAsync(
                    i + ": [" + await new NumbersGeneratorStream(i,42).ReadAllText() + "]"
                );
            }
        }
    }
}