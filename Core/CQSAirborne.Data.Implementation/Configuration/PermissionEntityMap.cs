using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CQSAirborne.Model;
using System;
using System.Collections.Generic;
using System.Text;
using CQSAirborne.Domain;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class PermissionEntityMap : IEntityTypeConfiguration<PermissionEntity>
    {
        public void Configure(EntityTypeBuilder<PermissionEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.PermissionTypeMasters)
            .WithMany(w => w.Permissions)
            .HasForeignKey(w => w.PermissionTypeId);

            builder.ToTable("adm_Permission");
        }
    }
}
