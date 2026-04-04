using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<ProjectEntity>
    {
        public void Configure(EntityTypeBuilder<ProjectEntity> builder)
        {
            builder.ToTable("projects");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name)
                .IsRequired();
            builder.Property(a => a.DateOpen)
                .IsRequired();
        }
    }
}
