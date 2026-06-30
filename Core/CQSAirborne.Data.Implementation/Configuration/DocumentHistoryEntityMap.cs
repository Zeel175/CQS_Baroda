using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class DocumentHistoryEntityMap : IEntityTypeConfiguration<DocumentHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentHistoryEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Category)
                .WithMany(w => w.DocumentHistorys)
                .HasForeignKey(w => w.CategoryId);

            builder.HasOne(w => w.DocumentType)
                .WithMany(w => w.DocumentTypeWiseDocumentHistories)
                .HasForeignKey(w => w.DocumentTypeId);

            builder.HasOne(w => w.CommonPicture)
                .WithMany(w => w.DocumentHistories)
                .HasForeignKey(w => w.CommonPictureId);

            builder.HasOne(w => w.CPRMasterEntity)
               .WithMany(w => w.DocumentHistoryEntities)
               .HasForeignKey(w => w.CPRMasterId);

            builder.ToTable("adm_DocumentsHistory");
        }
    }
}
