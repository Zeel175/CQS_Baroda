using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class ExternalLinkEntityMap : IEntityTypeConfiguration<ExternalLinkEntity>
    {
        public void Configure(EntityTypeBuilder<ExternalLinkEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("adm_ExternalLink");
        }
    }
}
