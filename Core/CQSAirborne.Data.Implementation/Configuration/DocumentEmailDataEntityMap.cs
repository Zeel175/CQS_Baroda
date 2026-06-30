using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class DocumentEmailDataEntityMap : IEntityTypeConfiguration<DocumentEmailDataEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentEmailDataEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("adm_DocumentEmailData");
        }
    }
}
