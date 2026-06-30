using CQSAirborne.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CQSAirborne.Data.Implementation.Configuration
{
    public class UserEntityMap : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.UserName)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(w => w.Password)
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(w => w.PasswordHash)
                .HasMaxLength(128)
                .IsRequired();

            //builder.Property(w => w.Id).UseSqlServerIdentityColumn();
            builder.Property(w => w.Id)
              .ValueGeneratedOnAdd();


            builder.ToTable("adm_User");
        }
    }
}
