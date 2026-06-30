using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class CategoryEntityMap : IEntityTypeConfiguration<CategoryEntity>
    {
        public void Configure(EntityTypeBuilder<CategoryEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.PrimaryCategory)
                .WithMany(w => w.SecondaryCategories)
                .HasForeignKey(w => w.PrimaryCategoryId);

            builder.HasOne(w => w.CategoryType)
                .WithMany(w => w.Categories)
                .HasForeignKey(w => w.CategoryTypeId);

            builder.Property(w => w.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.ToTable("adm_Category");
        }
    }
}
