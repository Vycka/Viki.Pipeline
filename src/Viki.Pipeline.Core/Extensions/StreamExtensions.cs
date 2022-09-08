using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Viki.Pipeline.Core.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<string> ReadAllText(this Stream stream, CancellationToken cancellationToken = default)
        {
            using StreamReader reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}