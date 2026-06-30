using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class EmailHistoryEntityMap : IEntityTypeConfiguration<EmailHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<EmailHistoryEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("adm_EmailHistory");
        }
    }
}
