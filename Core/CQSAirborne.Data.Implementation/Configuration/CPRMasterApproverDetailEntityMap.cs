using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class CPRMasterApproverDetailEntityMap : IEntityTypeConfiguration<CPRMasterApproverDetailEntity>
    {
        public void Configure(EntityTypeBuilder<CPRMasterApproverDetailEntity> builder)
        {
            builder.ToTable("adm_CPRMasterApproverDetails");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.ApproverRemarks)
                .HasColumnType("varchar(max)");

            builder.Property(e => e.IsMailSend)
                .HasDefaultValue(false);

            builder.HasOne(w => w.CprMaster)
              .WithMany(w => w.ApprovalsEntity)
              .HasForeignKey(w => w.CPRMasterId)
                .OnDelete(DeleteBehavior.Cascade);

            //builder.HasOne(e => e.User)
            //    .WithMany()
            //    .HasForeignKey(e => e.UserId);

            //builder.HasOne(e => e.CPRApprovalStatus)
            //    .WithMany()
            //    .HasForeignKey(e => e.ApprovalStatusId);
        }
    }
}
