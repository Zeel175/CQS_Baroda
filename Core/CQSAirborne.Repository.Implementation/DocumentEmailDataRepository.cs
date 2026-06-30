using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class DocumentEmailDataRepository : BaseRepository<DocumentEmailDataEntity>, IDocumentEmailDataRepository
    {
        private readonly IDataContext _dataContext;
        public DocumentEmailDataRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }
    }
}
