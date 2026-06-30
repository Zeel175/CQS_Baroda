using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class DocumentEntityMap : IEntityTypeConfiguration<DocumentEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Category)
                .WithMany(w => w.Documents)
                .HasForeignKey(w => w.CategoryId);

            builder.HasOne(w => w.DocumentType)
                .WithMany(w => w.DocumentTypeWiseDocuments)
                .HasForeignKey(w => w.DocumentTypeId);

            builder.HasOne(w => w.CommonPicture)
                .WithMany(w => w.Documents)
                .HasForeignKey(w => w.CommonPictureId);


            builder.HasOne(w => w.CPRMasterEntity)
                .WithMany(w => w.DocumentEntities)
                .HasForeignKey(w => w.CPRMasterId);

            builder.ToTable("adm_Documents");
        }
    }
}
