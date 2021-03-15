namespace Viki.Pipeline.Core.Interfaces
{

    /// <summary>
    /// IPipe is a transport layer interface which also keeps order of passed objects. 
    /// </summary>
    public interface IPipe<T> : IUnorderedPipe<T>
    {
    }
}