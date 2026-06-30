using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class DocumentTagsMapEntityMap : IEntityTypeConfiguration<DocumentTagsEntity>
    {
        public void Configure(EntityTypeBuilder<DocumentTagsEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Document)
                .WithMany(w => w.DocumentTags)
                .HasForeignKey(w => w.DocumentId);

            builder.ToTable("adm_DocumentTags");
        }
    }
}
