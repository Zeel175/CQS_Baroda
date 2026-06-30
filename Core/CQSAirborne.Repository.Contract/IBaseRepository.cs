using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IBaseRepository<T>
        where T : BaseEntity
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetAllNoTracking();
        T GetById(object id);
        Task<T> GetByIdAsync(object id);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<List<TModel>> ConvertToListAsync<TModel>(IQueryable<TModel> query) where TModel : class;
    }
}
