using System.Threading.Tasks;
using NUnit.Framework;
using Viki.Pipeline.Core.Extensions;
using Viki.Pipeline.Core.Streams.Mocks;
using Viki.Pipeline.Core.Streams.Mocks.Components;

namespace Viki.Pipeline.Core.Tests.Streams.Mocks
{
    [TestFixture]
    public class SequenceGeneratorStreamTests
    {
        [Test]
        public async Task HappyFlowNumbers()
        {
            //await TestContext.Out.WriteLineAsync(await new SequenceGeneratorStream(6, new NumbersSequenceGenerator()).ReadAllText());

            for (int i = 0; i < 60; i++)
            {
                await TestContext.Out.WriteLineAsync(
                    i + ": [" + await new SequenceGeneratorStream(i, new NumbersSequenceGenerator()).ReadAllText() + "]"
                );
            }
        }

        [Test]
        public async Task HappyFlowCharacters()
        {

            for (int i = 0; i < 60; i++)
            {
                await TestContext.Out.WriteLineAsync(
                    i + ": [" + await new SequenceGeneratorStream(i, new CharactersSequenceGenerator()).ReadAllText() + "]"
                );
            }
        }
    }
}