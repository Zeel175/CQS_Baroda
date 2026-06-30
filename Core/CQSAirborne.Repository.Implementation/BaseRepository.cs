using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public abstract class BaseRepository<T> : IBaseRepository<T>
        where T : BaseEntity
    {
        private readonly IDataContext _dataContext;
        public BaseRepository(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public virtual void Delete(T entity)
        {
            _dataContext.Delete(entity);
        }

        public virtual IQueryable<T> GetAll()
        {
            return _dataContext.GetAll<T>();
        }

        public virtual IQueryable<T> GetAllNoTracking()
        {
            return _dataContext.GetNoTracking<T>();
        }

        public virtual T GetById(object id)
        {
            return _dataContext.GetById<T>(id);
        }

        public virtual void Insert(T entity)
        {
            _dataContext.Insert(entity);
        }

        public virtual void Update(T entity)
        {
            _dataContext.Modify(entity);
        }

        public virtual Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return _dataContext.GetAsync<T>(predicate);
        }

        public virtual Task<List<TModel>> ConvertToListAsync<TModel>(IQueryable<TModel> query)
            where TModel : class
        {
            return _dataContext.ConvertToListAsync(query);
        }

        public Task<T> GetByIdAsync(object id)
        {
            return _dataContext.GetByIdAsync<T>(id);
        }
    }
}
