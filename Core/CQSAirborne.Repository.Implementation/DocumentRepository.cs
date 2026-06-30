using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class DocumentRepository : BaseRepository<DocumentEntity>, IDocumentRepository
    {
        private readonly IDataContext _dataContext;
        public DocumentRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public void AddDocumentHistory(DocumentHistoryEntity entity)
        {
            _dataContext.Insert(entity);
        }

        public void UpdateDocumentHistory(DocumentHistoryEntity entity)
        {
            _dataContext.Modify(entity);
        }

        public IQueryable<DocumentEntity> GetByCategory(int id)
        {
            return GetAllActiveNoTracking()
                .Where(w => w.CategoryId == id);
        }

        public IQueryable<PlantEntity> GetCategoryDocumentPlantList(int categoryId)
        {
            return GetAllNoTracking().Where(w => w.CategoryId == categoryId)
                .SelectMany(w => w.DocumentPlantMaps)
                .Select(w => w.Plant)
                .Distinct();
        }

        public IQueryable<DocumentHistoryEntity> GetHistoryByDocumentId(int id)
        {
            var documentCode = GetAllNoTracking().Where(w => w.Id == id)
                .Select(w => w.Code).DefaultIfEmpty().FirstOrDefault();
            if (string.IsNullOrEmpty(documentCode))
                return null;

            return _dataContext.GetNoTracking<DocumentHistoryEntity>()
                .Where(w => w.Code == documentCode);
        }

        public bool IsDocumentNumberUnique(int id, string documentNumber)
        {
            return !GetAllNoTracking()
                .Any(w => w.Id != id && w.DocumentNumber == documentNumber);
        }

        public DocumentHistoryEntity GetHistoryById(int id)
        {
            return _dataContext.GetById<DocumentHistoryEntity>(id);
        }

        public IQueryable<DocumentEntity> GetAllActiveNoTracking()
        {
            return GetAllNoTracking().Where(w => w.IsActive);
        }

        public IQueryable<DocumentEntity> GetActiveById(int id)
        {
            return GetAllActiveNoTracking().Where(w => w.Id == id);
        }

        public IQueryable<DocumentHistoryEntity> GetAllDocumentHistory()
        {
            //var documentCode = GetAllNoTracking().Where(w => w.Id == id)
            //    .Select(w => w.Code).DefaultIfEmpty().FirstOrDefault();
            //if (string.IsNullOrEmpty(documentCode))
            //    return null;

            return _dataContext.GetNoTracking<DocumentHistoryEntity>();
                //.Where(w => w.Code == documentCode);
        }

        public DocumentEntity GetByIdWithIncludes(int id)
        {
            return GetAllNoTracking()
                //.Where(d => d.IsActive)   // optional
                .Where(d => d.Id == id)
                .Include(d => d.DocumentType)
                .Include(d => d.Category)
                .Include(d => d.CommonPicture)
                .Include(d => d.DocumentTags)
                .Include(d => d.DocumentPlantMaps)
                    .ThenInclude(m => m.Plant)
                .Include(d => d.DocumentPlantMaps)
                    .ThenInclude(m => m.Picture)
                .FirstOrDefault();
        }

        public DocumentEntity GetByIdWithIncludesForUpdate(int id)
        {
            return GetAllNoTracking()
                //.Where(d => d.IsActive)   // optional
                .Where(d => d.Id == id)
                .Include(d => d.DocumentType)
                .Include(d => d.Category)
                .Include(d => d.CommonPicture)
                .Include(d => d.DocumentTags)
                .Include(d => d.DocumentPlantMaps)
                .FirstOrDefault();
        }
        public DocumentHistoryEntity GetHistoryByIdWithInclude(int id)
        {
            return _dataContext.GetNoTracking<DocumentHistoryEntity>()
                .Where(d => d.Id == id)
                .Include(d => d.DocumentType)
                .Include(d => d.Category)
                .Include(d => d.CommonPicture)
                .Include(d => d.DocumentPlantMaps)
                    .ThenInclude(m => m.Plant)
                .Include(d => d.DocumentPlantMaps)
                    .ThenInclude(m => m.Picture)
                .FirstOrDefault();
        }
    }
}
