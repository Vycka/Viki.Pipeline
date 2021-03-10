using Viki.Pipeline.Core.Pipes;

namespace Viki.Pipeline.Core.Interfaces
{
    public interface IPipeFactory<T> : IPipeProvider<T>
    {
        IPipe<T> Create();
    }

    public interface IPipeProvider<T>
    {
        event PipeCreatedEventDelegate<T> PipeCreatedEvent;
    }

    public delegate void PipeCreatedEventDelegate<T>(IPipeFactory<T> sender, BatchingPipe<T> pipe);
}