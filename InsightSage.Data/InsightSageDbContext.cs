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

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

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
        }
    }
}
