namespace Viki.Pipeline.Core.Interfaces
{
    /// <summary>
    /// Minimal monitoring interface to monitor only critical metrics.
    /// </summary>
    public interface IPipeMetrics
    {
        /// <summary>
        /// Get approximate value of items in the buffer.
        /// </summary>
        long BufferedItems { get; }
    }
}