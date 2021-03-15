using System.Collections.Generic;

namespace Viki.Pipeline.Core.Interfaces
{
    /// <summary>
    /// Producer side of transport layer. Only one thread at the time can use implementation methods on instance of this interface.
    /// </summary>
    public interface IProducer<T>
    {
        /// <summary>
        /// Produce single item into collection
        /// </summary>
        /// <param name="item">item to produce</param>
        void Produce(T item);

        /// <summary>
        /// Produce enumerable list of items.
        /// Using this implementation with underlying implementation is ICollection can yield additional performance gains.
        /// </summary>
        /// <param name="items">collection of items to produce</param>
        void Produce(IEnumerable<T> items);

        /// <summary>
        /// Signals collection that no more items will be produced.
        /// </summary>
        void ProducingCompleted();
    }
}