using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Configurations
{
    public class UsersConfiguration : IEntityTypeConfiguration<UsersEntity>
    {
        public void Configure(EntityTypeBuilder<UsersEntity> builder)
        {
            builder.ToTable("users");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Login)
                .IsRequired();
            builder.Property(a => a.HashPassword)
                .IsRequired();
            builder.Property(a => a.Role)
                .IsRequired();
        }
    }
}
