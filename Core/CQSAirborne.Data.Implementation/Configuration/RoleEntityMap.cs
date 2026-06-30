using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CQSAirborne.Model;
using CQSAirborne.Domain;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class RoleEntityMap : IEntityTypeConfiguration<RoleEntity>
    {
        public void Configure(EntityTypeBuilder<RoleEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("adm_Roles");
        }
    }
}
