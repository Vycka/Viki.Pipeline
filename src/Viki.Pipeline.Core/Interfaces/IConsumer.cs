using System.Collections.Generic;

namespace Viki.Pipeline.Core.Interfaces
{
    public interface IConsumer<T>
    {
        bool TryLockBatch(out ICollection<T> batch);

        void ReleaseBatch();

        bool Available { get; }
    }
}