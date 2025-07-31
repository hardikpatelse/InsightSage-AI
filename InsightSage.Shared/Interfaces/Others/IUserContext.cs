namespace InsightSage.Shared.Interfaces.Others
{
    public interface IUserContext
    {
        string? UserId { get; }
        string? Email { get; }
        string? Name { get; }
        string? TenantId { get; }
    }
}
