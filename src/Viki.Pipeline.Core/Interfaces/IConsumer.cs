using System.Collections.Generic;

namespace Viki.Pipeline.Core.Interfaces
{
    /// <summary>
    /// Consumer side of transport layer. Only one thread at the time can use implementation methods on instance of this interface.
    /// </summary>
    public interface IConsumer<T> : IUnorderedConsumer<T>
    {
    }
}