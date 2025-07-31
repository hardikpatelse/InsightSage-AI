using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses.Common;

namespace InsightSage.Shared.Models.Responses
{
    public class UserResponse<T> : EntitiesResponse<T> where T : class
    {
    }
}
