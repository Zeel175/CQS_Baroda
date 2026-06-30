using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class CustomerDocumentMappingEntityMap : IEntityTypeConfiguration<CustomerDocumentMappingEntity>
    {
        public void Configure(EntityTypeBuilder<CustomerDocumentMappingEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.Customer)
                .WithMany(w => w.CustomerDocumentMappings)
                .HasForeignKey(w => w.CustomerId);

            builder.HasOne(w => w.Picture)
                .WithMany(w => w.CustomerDocuments)
                .HasForeignKey(w => w.PictureId);

            builder.HasOne(w => w.Documents)
                .WithMany(w => w.CustomerDocuments)
                .HasForeignKey(w => w.DocumentId);

            builder.ToTable("adm_CustomerDocumentMapping");
        }
    }
}
