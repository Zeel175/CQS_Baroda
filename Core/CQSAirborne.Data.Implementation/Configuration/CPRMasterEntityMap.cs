using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class CPRMasterEntityMap : IEntityTypeConfiguration<CPRMasterEntity>
    {
        public void Configure(EntityTypeBuilder<CPRMasterEntity> builder)
        {
            builder.ToTable("adm_CPRMaster");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.CPRNo)
                .HasMaxLength(200);

            builder.Property(e => e.DepartmentTitle)
                .HasMaxLength(300);

            builder.Property(e => e.Department)
                .HasMaxLength(300);

            builder.Property(e => e.Revision)
                .HasMaxLength(50);

            builder.Property(e => e.Program)
                .HasMaxLength(300);

            builder.Property(e => e.ActionRequested)
                .HasMaxLength(400);

            builder.Property(e => e.CPRRaisedDueTo)
                .HasMaxLength(400);

            // Relationships

            builder.HasMany(e => e.ApprovalsEntity)
     .WithOne(a => a.CprMaster)
     .HasForeignKey(a => a.CPRMasterId);

            //builder.HasOne(e => e.RequestedByEntity)
            //       .WithMany()
            //       .HasForeignKey(e => e.RequestedById);

            //builder.HasOne(e => e.DocumentEntity)
            //       .WithMany()
            //       .HasForeignKey(e => e.DocumentId);

            //builder.HasOne(e => e.CategoryEntity)
            //       .WithMany()
            //       .HasForeignKey(e => e.CategoryId);

        }
    }
}
