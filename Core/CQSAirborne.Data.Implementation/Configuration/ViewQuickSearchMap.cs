using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class ViewQuickSearchMap : IEntityTypeConfiguration<ViewQuickSearch>
    {
        public void Configure(EntityTypeBuilder<ViewQuickSearch> builder)
        {
            builder.HasKey(new string[]{ "Id", "DbName" });
            builder.ToTable("vw_QuickSearch");
        }
    }
}
