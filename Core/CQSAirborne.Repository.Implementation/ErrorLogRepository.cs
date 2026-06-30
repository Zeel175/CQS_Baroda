using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class ErrorLogRepository : BaseRepository<ErrorLogEntity>, IErrorLogRepository
    {
        private readonly IDataContext _dataContext;
        public ErrorLogRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }

    }
}
