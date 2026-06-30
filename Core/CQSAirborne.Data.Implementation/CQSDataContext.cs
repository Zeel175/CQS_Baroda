using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CQSAirborne.Data.Contract;
using CQSAirborne.Data.Implementation.Configuration;
using Microsoft.EntityFrameworkCore;

namespace CQSAirborne.Data.Implementation
{
    public class CQSDataContext : DbContext, IDataContext
    {
        public CQSDataContext(DbContextOptions<CQSDataContext> options)
            : base(options)
        {

        }

        public void Delete<T>(T entity)
            where T : class
        {
            Set<T>().Remove(entity);
            Entry<T>(entity).State = EntityState.Deleted;
        }

        public IQueryable<T> GetAll<T>()
            where T : class
        {
            return Set<T>().AsQueryable();
        }

        public T GetById<T>(object id)
            where T : class
        {
            return Find<T>(id);
        }

        //public Task<T> GetByIdAsync<T>(object id)
        //    where T : class
        //{
        //    return FindAsync<T>(id);
        //}
        public async Task<T> GetByIdAsync<T>(object id) where T : class
        {
            return await Set<T>().FindAsync(id);
        }

        public IQueryable<T> GetNoTracking<T>()
            where T : class
        {
            return Set<T>().AsNoTracking();
        }

        public void Insert<T>(T entity)
            where T : class
        {
            Set<T>().Add(entity);
            Entry(entity).State = EntityState.Added;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.ApplyConfiguration(new UserEntityMap());
            builder.ApplyConfiguration(new PictureEntityMap());
            builder.ApplyConfiguration(new PlantEntityMap());
            builder.ApplyConfiguration(new CodeMaintainEntityMap());
            builder.ApplyConfiguration(new GroupCodeEntityMap());
            builder.ApplyConfiguration(new CategoryEntityMap());
            builder.ApplyConfiguration(new DocumentEntityMap());
            builder.ApplyConfiguration(new ExternalLinkEntityMap());
            builder.ApplyConfiguration(new DocumentPlantMapEntityMap());
            builder.ApplyConfiguration(new DocumentTagsMapEntityMap());
            builder.ApplyConfiguration(new ViewQuickSearchMap());
            builder.ApplyConfiguration(new ErrorLogEntityMap());
            builder.ApplyConfiguration(new DocumentHistoryEntityMap());
            builder.ApplyConfiguration(new DocumentPlantMapHistoryEntityMap());
            builder.ApplyConfiguration(new EmployeeEntityMap());
            builder.ApplyConfiguration(new CustomerEntityMap());
            builder.ApplyConfiguration(new CustomerDocumentMappingEntityMap());
            builder.ApplyConfiguration(new DocumentEmailDataEntityMap());
            builder.ApplyConfiguration(new EmailHistoryEntityMap());
            builder.ApplyConfiguration(new RoleEntityMap());
            builder.ApplyConfiguration(new RolePermissionEntityMap());
            builder.ApplyConfiguration(new PermissionEntityMap());


            builder.ApplyConfiguration(new CPRMasterEntityMap());
            builder.ApplyConfiguration(new CPRMasterApproverDetailEntityMap());
        }

        public void Modify<T>(T entity)
            where T : class
        {
            Entry(entity).State = EntityState.Modified;
        }

        public Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate)
            where T : class
        {
            return Set<T>().FirstOrDefaultAsync(predicate);
        }

        public Task<List<T>> ConvertToListAsync<T>(IQueryable<T> query) where T : class
        {
            return query.ToListAsync();
        }
    }
}
