using System.ComponentModel.DataAnnotations;

namespace InsightSage.Shared.Models.Entities
{
    public class ChatSession : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public Guid UniqueIdentity { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Foreign key reference to the AI model used for this chat session
        /// </summary>
        public int? AIModelId { get; set; }

        /// <summary>
        /// Foreign key reference to the chat mode used for this chat session
        /// </summary>
        public int? ChatModeId { get; set; }

        /// <summary>
        /// Navigation property to the associated AI model
        /// </summary>
        public virtual AIModel? AIModel { get; set; }

        /// <summary>
        /// Navigation property to the associated chat mode
        /// </summary>
        public virtual ChatMode? ChatMode { get; set; }
    }
}
