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
    public class RolePermissionEntityMap : IEntityTypeConfiguration<RolePermissionEntity>
    {
        public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.HasOne(w => w.RoleMaster)
            .WithMany(w => w.RolePermissions)
            .HasForeignKey(w => w.RoleId);

            builder.HasOne(w => w.PermissionMaster)
           .WithMany(w => w.RolePermissions)
           .HasForeignKey(w => w.PermissionId);

            builder.ToTable("adm_RolePermission");
        }
    }
}
