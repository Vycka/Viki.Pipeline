using System.Collections.Generic;

namespace Viki.Pipeline.Core.Interfaces
{
    /// <summary>
    /// IUnorderedConsumer is special IConsumer marker to indicate that items read are not guaranteed to be in the same order as in which they were produced.
    /// For ordered transfers, IPipe needs to be used.
    /// </summary>
    public interface IUnorderedConsumer<T>
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