namespace InsightSage.Shared.Interfaces.Others
{
    public interface IUserContext
    {
        Guid UserId { get; }
        string? Email { get; }
        string? Name { get; }
        string? TenantId { get; }
        string? Role { get; }
    }
}
