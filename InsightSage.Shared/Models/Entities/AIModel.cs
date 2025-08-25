namespace InsightSage.Shared.Models.Entities
{
    public class AIModel : BaseEntity
    {
        private string _name = string.Empty;
        
        public required string Name 
        { 
            get => _name;
            set
            {
                _name = value;
                Slug = GenerateSlugFromName(value);
            }
        }
        
        public string Description { get; set; } = string.Empty;
        
        public string Slug { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether the AI model is currently active and available for use
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Generates a slug from the name by converting to lowercase and replacing spaces with underscores
        /// </summary>
        /// <param name="name">The name to convert to a slug</param>
        /// <returns>The generated slug</returns>
        private static string GenerateSlugFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.Trim()
                      .ToLowerInvariant()
                      .Replace(" ", "_")
                      .Replace("\t", "_")
                      .Replace("\n", "_")
                      .Replace("\r", "_");
        }
    }
}
