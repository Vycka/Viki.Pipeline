namespace Viki.Pipeline.Core.Interfaces
{
    /// <summary>
    /// Same as IPipe, but results might be unordered (sort of like udp)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnorderedPipe<T> : IPipe<T>
    {
    }
}