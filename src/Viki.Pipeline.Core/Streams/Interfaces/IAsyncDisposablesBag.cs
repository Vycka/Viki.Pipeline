using System;

namespace Viki.Pipeline.Core.Streams.Interfaces
{
    public interface IAsyncDisposablesBag
    {
        /// <summary>
        /// Adds disposable object to list, from which everything will be disposed once dispose is triggered on this object instance.
        /// </summary>
        /// <param name="disposable">object to dispose</param>
        void AddDisposable(IAsyncDisposable disposable);
    }
}