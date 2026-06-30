using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class GroupCodeEntityMap : IEntityTypeConfiguration<GroupCodeEntity>
    {
        public void Configure(EntityTypeBuilder<GroupCodeEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("adm_GroupCode");
        }
    }
}
