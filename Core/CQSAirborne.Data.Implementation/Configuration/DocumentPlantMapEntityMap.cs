using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class DocumentPlantMapEntityMap : IEntityTypeConfiguration<DocumentPlantMapEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentPlantMapEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Document)
                .WithMany(w => w.DocumentPlantMaps)
                .HasForeignKey(w => w.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);   // ✅ IMPORTANT
            

            builder.HasOne(w => w.Plant)
                .WithMany(w => w.DocumentPlantMaps)
                .HasForeignKey(w => w.PlantId);

            builder.HasOne(w => w.Picture)
                .WithMany(w => w.DocumentPlants)
                .HasForeignKey(w => w.PictureId);


            builder.ToTable("adm_DocumentPlantMap");
        }
    }
}
