using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQS.Log.Engine.Data
{
    public class LogContext : DbContext
    {
        public LogContext(DbContextOptions<LogContext> options)
            : base(options)
        {

        }

        public DbSet<LogModel> Log { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new LogModelMap());
        }
    }

    public class LogModel
    {
        public long Id { get; set; }
        public string ApplicationName { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class LogModelMap : IEntityTypeConfiguration<LogModel>
    {
        public void Configure(EntityTypeBuilder<LogModel> builder)
        {
            builder.HasKey(w => w.Id);

            builder.ToTable("tbl_ErrorLog");
        }
    }
}
