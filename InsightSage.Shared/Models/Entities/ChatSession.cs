using System.ComponentModel.DataAnnotations;

namespace InsightSage.Shared.Models.Entities
{
    public class ChatSession : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public Guid UniqueIdentity { get; set; }

        public Guid UserId { get; set; }
    }
}
