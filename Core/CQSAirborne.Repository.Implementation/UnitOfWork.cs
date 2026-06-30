using CQSAirborne.Data.Contract;
using CQSAirborne.Repository.Contract;
using System.Threading;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDataContext _dataContext;

        public UnitOfWork(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public int Commit()
        {
            return _dataContext.SaveChanges();
        }

        public Task<int> CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
