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
            builder.HasData(
                new UsersEntity
                {
                    Id = Guid.NewGuid(),
                    Login = "admin@mail.ru",
                    HashPassword = "$2a$11$cXzJITgtUiw/4cWi1y.XH.xHG01Bwyj53m3w2HOU4nWIrOk24AgXG", //admin123
                    Role = "admin"
                },
                new UsersEntity
                {
                    Id = Guid.NewGuid(),
                    Login = "user@mail.ru",
                    HashPassword = "$2a$11$zD.sI1v4tUcoNEHXYH/vduhMU8kiFMXJh5yURxIkE5S2emBiS156i", //user123
                    Role = "user"
                }
                );
        }
    }
}
