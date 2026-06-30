using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class DocumentPlantMapHistoryEntityMap : IEntityTypeConfiguration<DocumentPlantMapHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentPlantMapHistoryEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Document)
                .WithMany(w => w.DocumentPlantMaps)
                .HasForeignKey(w => w.DocumentHistoryId);

            builder.HasOne(w => w.Plant)
                .WithMany(w => w.DocumentPlantMapHistories)
                .HasForeignKey(w => w.PlantId);

            builder.HasOne(w => w.Picture)
                .WithMany(w => w.DocumentPlantHistories)
                .HasForeignKey(w => w.PictureId);

            builder.ToTable("adm_DocumentPlantMapHistory");
        }
    }
}
