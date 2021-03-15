﻿namespace Viki.Pipeline.Core.Interfaces
{
    /// <summary>
    /// IUnorderedPipe is a transport layer interface which alone doesn't guarantee same order of passed objects. this can be used for messaging type of tasks where order is not relevant.
    /// For ordered transfers, IPipe needs to be used.
    /// </summary>
    public interface IUnorderedPipe<T> : IConsumer<T>, IProducer<T>, IPipeMetrics
    {
    }

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