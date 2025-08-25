using InsightSage.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightSage.Data
{
    public class InsightSageDbContext : DbContext
    {
        protected InsightSageDbContext()
        {
        }
        public InsightSageDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AIModel> AIModels { get; set; }
        public DbSet<ChatMode> ChatModes { get; set; }
        public DbSet<AIPlatform> AIPlatforms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                // Configure primary key with identity
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1); // Start at 1, increment by 1
                
                // Configure other properties
                entity.Property(e => e.Email)
                    .HasMaxLength(256)
                    .IsRequired(false);
                    
                entity.Property(e => e.Name)
                    .HasMaxLength(256)
                    .IsRequired(false);
                    
                entity.Property(e => e.UserId)
                    .HasMaxLength(256)
                    .IsRequired(false);
                    
                entity.Property(e => e.TenantId)
                    .HasMaxLength(256)
                    .IsRequired(false);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
                    
                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
                
                // Create unique constraint on Email
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email_Unique")
                    .HasFilter("[Email] IS NOT NULL"); // Only enforce uniqueness for non-null emails
                    
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Users_UserId");
                    
                entity.HasIndex(e => e.TenantId)
                    .HasDatabaseName("IX_Users_TenantId");
            });

            // Configure ChatSession entity
            modelBuilder.Entity<ChatSession>(entity =>
            {
                // Configure primary key with identity
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1);

                // Configure properties
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.UniqueIdentity)
                    .IsRequired();

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.AIModelId)
                    .IsRequired(false); // Optional - session can exist without AI model

                entity.Property(e => e.ChatModeId)
                    .IsRequired(false); // Optional - session can exist without chat mode

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure relationships
                entity.HasOne(e => e.AIModel)
                    .WithMany()
                    .HasForeignKey(e => e.AIModelId)
                    .OnDelete(DeleteBehavior.SetNull) // Don't cascade delete sessions when AI model is deleted
                    .HasConstraintName("FK_ChatSessions_AIModel");

                entity.HasOne(e => e.ChatMode)
                    .WithMany()
                    .HasForeignKey(e => e.ChatModeId)
                    .OnDelete(DeleteBehavior.SetNull) // Don't cascade delete sessions when chat mode is deleted
                    .HasConstraintName("FK_ChatSessions_ChatMode");

                // Create unique constraint on UniqueIdentity
                entity.HasIndex(e => e.UniqueIdentity)
                    .IsUnique()
                    .HasDatabaseName("IX_ChatSessions_UniqueIdentity");

                // Create index on UserId for better performance
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_ChatSessions_UserId");

                // Create composite index for user and name searches
                entity.HasIndex(e => new { e.UserId, e.Name })
                    .HasDatabaseName("IX_ChatSessions_UserId_Name");

                // Create indexes on foreign keys for better join performance
                entity.HasIndex(e => e.AIModelId)
                    .HasDatabaseName("IX_ChatSessions_AIModelId");

                entity.HasIndex(e => e.ChatModeId)
                    .HasDatabaseName("IX_ChatSessions_ChatModeId");

                // Create composite index for filtering by user and AI model
                entity.HasIndex(e => new { e.UserId, e.AIModelId })
                    .HasDatabaseName("IX_ChatSessions_UserId_AIModelId");

                // Create composite index for filtering by user and chat mode
                entity.HasIndex(e => new { e.UserId, e.ChatModeId })
                    .HasDatabaseName("IX_ChatSessions_UserId_ChatModeId");
            });

            // Configure ChatMessage entity
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                // Configure primary key with identity
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1);

                // Configure properties
                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Content)
                    .IsRequired();

                entity.Property(e => e.ChatSessionId)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship with ChatSession
                entity.HasOne<ChatSession>()
                    .WithMany()
                    .HasForeignKey(e => e.ChatSessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create index on ChatSessionId for better performance
                entity.HasIndex(e => e.ChatSessionId)
                    .HasDatabaseName("IX_ChatMessages_ChatSessionId");
            });

            // Configure AIModel entity
            modelBuilder.Entity<AIModel>(entity =>
            {
                // Configure primary key with identity
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1);

                // Configure properties
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(e => e.Slug)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Create unique constraint on Name
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_AIModels_Name_Unique");

                // Create unique constraint on Slug for URL-safe lookups
                entity.HasIndex(e => e.Slug)
                    .IsUnique()
                    .HasDatabaseName("IX_AIModels_Slug_Unique");

                // Create index on IsActive for filtering active models
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_AIModels_IsActive");
            });

            // Configure ChatMode entity
            modelBuilder.Entity<ChatMode>(entity =>
            {
                // Configure primary key with identity
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1);

                // Configure properties
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(e => e.Slug)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(e => e.IconName)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Create unique constraint on Name
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_ChatModes_Name_Unique");

                // Create unique constraint on Slug for URL-safe lookups
                entity.HasIndex(e => e.Slug)
                    .IsUnique()
                    .HasDatabaseName("IX_ChatModes_Slug_Unique");

                // Create index on IsActive for filtering active modes
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_ChatModes_IsActive");
            });

            // Configure AIPlatform entity
            modelBuilder.Entity<AIPlatform>(entity =>
            {
                // Configure primary key with identity
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1);

                // Configure properties
                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                entity.Property(e => e.Slug)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(e => e.ApiBaseUrl)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.WebsiteUrl)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.DocumentationUrl)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.AuthHeaderName)
                    .HasMaxLength(100)
                    .IsRequired()
                    .HasDefaultValue("Authorization");

                entity.Property(e => e.AuthHeaderFormat)
                    .HasMaxLength(200)
                    .IsRequired()
                    .HasDefaultValue("Bearer {api_key}");

                entity.Property(e => e.ConfigurationJson)
                    .HasColumnType("ntext")
                    .IsRequired(false);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Create unique constraint on Name
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_AIPlatforms_Name_Unique");

                // Create unique constraint on Slug for URL-safe lookups
                entity.HasIndex(e => e.Slug)
                    .IsUnique()
                    .HasDatabaseName("IX_AIPlatforms_Slug_Unique");

                // Create index on IsActive for filtering active platforms
                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_AIPlatforms_IsActive");

                // Create index on DisplayOrder for sorting
                entity.HasIndex(e => e.DisplayOrder)
                    .HasDatabaseName("IX_AIPlatforms_DisplayOrder");
            });
        }
    }
}
