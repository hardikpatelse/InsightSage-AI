using InsightSage.Shared.Models.Responses.Common;

namespace InsightSage.Shared.Interfaces.Services.Common
{
    public interface IEntityService<T, U, V>
    {
        Task<T> GetAllAsync();

        Task<U> GetByIdAsync(int id);

        Task<AddUpdateEntityResponse> AddUpdateAsync(V data);

        Task<DeleteEntityResponse> DeleteAsync(int id, int modifiedBy);
    }
}
