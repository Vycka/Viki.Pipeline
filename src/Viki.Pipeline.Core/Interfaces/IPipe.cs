namespace Viki.Pipeline.Core.Interfaces
{
    public interface IPipe<T> : IConsumer<T>, IProducer<T>
    {
    }
}