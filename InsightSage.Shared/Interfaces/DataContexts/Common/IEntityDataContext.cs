namespace InsightSage.Shared.Interfaces.DataContexts.Common
{
    public interface IEntityDataContext<T> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(int id);

        Task<int> AddAsync(T data);

        Task<int> UpdateAsync(T data);

        Task<string> DeleteAsync(int id, int modifiedBy);
    }
}
