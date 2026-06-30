using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class PictureEntityMap : IEntityTypeConfiguration<PictureEntity>
    {
        public void Configure(EntityTypeBuilder<PictureEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name)
                .IsRequired();

            builder.Property(w => w.Path)
                .IsRequired();

            builder.ToTable("adm_Picture");
        }
    }
}
