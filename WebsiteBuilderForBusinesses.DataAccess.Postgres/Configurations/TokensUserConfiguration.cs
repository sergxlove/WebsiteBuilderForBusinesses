using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Configurations
{
    public class TokensUserConfiguration : IEntityTypeConfiguration<TokensUserEntity>
    {
        public void Configure(EntityTypeBuilder<TokensUserEntity> builder)
        {
            builder.ToTable("tokensUser");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.UserId)
                .IsRequired();
            builder.Property(t => t.Created)
                .IsRequired();
            builder.Property(t => t.Ended)
                .IsRequired();
            builder.Property(t => t.Email)
                .IsRequired();
            builder.Property(t => t.Role)
                .IsRequired();
        }
    }
}
