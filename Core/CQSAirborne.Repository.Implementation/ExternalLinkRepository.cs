using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class ExternalLinkRepository : BaseRepository<ExternalLinkEntity>, IExternalLinkRepository
    {
        public ExternalLinkRepository(IDataContext dataContext)
            : base(dataContext)
        {
        }
    }
}
