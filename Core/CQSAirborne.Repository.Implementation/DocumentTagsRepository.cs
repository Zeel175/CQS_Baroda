using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class DocumentTagsRepository : BaseRepository<DocumentTagsEntity>, IDocumentTagsRepository
    {
        private readonly IDataContext _dataContext;
        public DocumentTagsRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public IQueryable<DocumentTagsEntity> GetByDocumentId(int documentId)
        {
            return GetAllNoTracking().Where(w => w.DocumentId == documentId);
        }
    }
}
