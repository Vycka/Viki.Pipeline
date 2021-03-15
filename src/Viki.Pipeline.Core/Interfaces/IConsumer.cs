using System.Collections.Generic;

namespace Viki.Pipeline.Core.Interfaces
{
    /// <summary>
    /// Consumer side of transport layer. Only one thread at the time can use implementation methods on instance of this interface.
    /// </summary>
    public interface IConsumer<T>
    {
        /// <summary>
        /// Attempt to lock batch of data for reading.
        /// </summary>
        /// <param name="batch">reference to where pointer to batch will be written. BEWARE: batch collection is only borrowed, data from it needs to be copied else-were before using ReleaseBatch(). After which implementation might reuse it.</param>
        /// <returns></returns>
        bool TryLockBatch(out ICollection<T> batch);

        /// <summary>
        /// Signal that batch received trough TryLockBatch() was read and implementation can use it again to fill new data.
        /// </summary>
        void ReleaseBatch();

        /// <summary>
        /// Check if more data is expected to be read. (this does not mean that TryLockBatch() will succeed on the next call)
        /// This becomes false only when on producing side ProducingCompleted() is called and here on IConsumer side all buffered data was received through TryLockBatch()
        /// </summary>
        bool Available { get; }
    }
}