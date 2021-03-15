namespace Viki.Pipeline.Core.Interfaces
{
    public interface IUnorderedPipe<T> : IUnorderedConsumer<T>, IProducer<T>
    {
    }
}