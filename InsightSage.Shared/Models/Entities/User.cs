namespace InsightSage.Shared.Models.Entities
{
    public class User : BaseEntity
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? TenantId { get; set; }
    }
}
