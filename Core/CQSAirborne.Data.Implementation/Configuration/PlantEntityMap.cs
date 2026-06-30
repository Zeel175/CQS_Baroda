using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class PlantEntityMap : IEntityTypeConfiguration<PlantEntity>
    {
        public void Configure(EntityTypeBuilder<PlantEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Logo)
                .WithMany(w => w.Plants)
                .HasForeignKey(w => w.LogoId);

            builder.ToTable("adm_Plant");
        }
    }
}
