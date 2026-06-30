using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class CodeMaintainEntityMap : IEntityTypeConfiguration<CodeMaintainEntity>
    {
        public void Configure(EntityTypeBuilder<CodeMaintainEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("adm_CodeMaintain");
        }
    }
}
