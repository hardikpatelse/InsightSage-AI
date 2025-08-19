namespace InsightSage.Shared.Models.Responses.Common
{
    public class EntitiesResponse<T> : BaseResponse
    {
        public T Result { get; set; }
    }
}
