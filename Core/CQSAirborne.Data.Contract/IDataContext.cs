using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CQSAirborne.Data.Contract
{
    public interface IDataContext
    {
        IQueryable<T> GetAll<T>() where T : class;
        IQueryable<T> GetNoTracking<T>() where T : class;
        T GetById<T>(object id) where T : class;
        Task<T> GetByIdAsync<T>(object id) where T : class;
        Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        void Insert<T>(T entity) where T : class;
        void Modify<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;


        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<List<T>> ConvertToListAsync<T>(IQueryable<T> query) where T : class;
    }
}
