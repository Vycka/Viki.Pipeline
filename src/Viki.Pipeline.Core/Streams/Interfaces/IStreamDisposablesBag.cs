using System;
using System.IO;

namespace Viki.Pipeline.Core.Streams.Interfaces
{
    public interface IStreamDisposablesBag
    {
        /// <summary>
        /// Adds disposable object to list, from which everything will be disposed through sync/async dispose.. depending on how parent trigger dispose is triggered.
        /// </summary>
        /// <param name="disposable">object to dispose</param>
        void AddDisposable(Stream disposable);
    }
}