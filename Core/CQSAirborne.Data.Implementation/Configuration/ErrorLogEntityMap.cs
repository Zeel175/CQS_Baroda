using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class ErrorLogEntityMap : IEntityTypeConfiguration<ErrorLogEntity>
    {
        public void Configure(EntityTypeBuilder<ErrorLogEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("tbl_ErrorLog");
        }
    }
}
