using System.IO;

namespace Viki.Pipeline.Core.Tests
{
    public class StreamReader
    {
        public static byte[] ReadAll(Stream stream)
        {
            using MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }
    }
}