namespace Taskular
{
    using System.Threading.Tasks;


    public interface CompensationResult
    {
        Task Task { get; }
    }


    public interface CompensationResult<T>
    {
        Task<T> Task { get; }
    }
}