namespace Viki.Pipeline.Core.Streams.Mocks.Components
{
    public interface ISequenceGenerator
    {
        /// TODO: Seperator should be allowed be dynamic size.. but thats a whole other level given the Read() buffer size.
        /// <summary>
        /// Get separator which will be added after next element is completed 
        /// it will be only added if there is enough space on stream to hold separator and at least a single byte for the next value.
        /// </summary>
        byte NextSeparator();

        /// <summary>
        /// Target length for the next element to try to match
        /// However.. Actual element length might be smaller if there is not enough space on stream to complete it.
        /// </summary>
        long NextElementLength();

        /// <summary>
        /// Generate first element byte 
        /// </summary>
        byte NextElementFirstByte();

        /// <summary>
        /// Get another random byte for element (for eny position in single element except the first)
        /// </summary>
        byte NextElementByte();
    }
}