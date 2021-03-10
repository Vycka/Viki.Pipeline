using Viki.Pipeline.Core.Interfaces;
using Viki.Pipeline.Core.Pipes;

namespace Viki.Pipeline.Core.Factories
{
    public class BatchingPipeFactory<T> : IPipeFactory<T>
    {
        public IPipe<T> Create()
        {
            BatchingPipe<T> pipe = new BatchingPipe<T>();

            PipeCreatedEvent?.Invoke(this, pipe);

            return pipe;
        }

        public event PipeCreatedEventDelegate<T> PipeCreatedEvent;
    }
}