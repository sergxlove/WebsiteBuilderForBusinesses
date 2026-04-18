using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Configurations;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres
{
    public class WebBuilderDbContext : DbContext
    {
        public WebBuilderDbContext(DbContextOptions<WebBuilderDbContext> options) 
            :base(options) { }

        public DbSet<UsersEntity> Users { get; set; }
        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<TokensUserEntity> TokensUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsersConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectConfiguration());
            modelBuilder.ApplyConfiguration(new TokensUserConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
