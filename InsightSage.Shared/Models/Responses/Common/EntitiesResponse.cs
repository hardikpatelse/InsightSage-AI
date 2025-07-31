namespace InsightSage.Shared.Models.Responses.Common
{
    public class EntitiesResponse<T> : BaseResponse
        where T : class
    {
        public T Result { get; set; }
    }
}
