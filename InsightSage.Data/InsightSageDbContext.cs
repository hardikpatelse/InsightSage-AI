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

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
