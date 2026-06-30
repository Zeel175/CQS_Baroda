using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class DocumentPlantRepository : BaseRepository<DocumentPlantMapEntity>, IDocumentPlantRepository
    {
        private readonly IDataContext _dataContext;
        public DocumentPlantRepository(IDataContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public IQueryable<DocumentPlantMapEntity> GetByDocumentId(int documentId)
        {
            return GetAllNoTracking().Where(w => w.DocumentId == documentId);
        }

        public IQueryable<PlantEntity> GetDocumentAllPlants()
        {
            return GetAllNoTracking()
                .Select(w => w.Plant)
                .Distinct()
                .OrderBy(w => w.Alias);
        }

        public IQueryable<PlantEntity> GetPlantsForDocumentHistory(int id)
        {
            var documentCode = _dataContext.GetNoTracking<DocumentEntity>().Where(w => w.Id == id)
                .Select(w => w.Code).DefaultIfEmpty().FirstOrDefault();
            if (!string.IsNullOrEmpty(documentCode))
            {
                var ddata = _dataContext.GetNoTracking<DocumentPlantMapHistoryEntity>()
                .Where(w => w.Document.Code == documentCode)
                .Select(w => w.Plant)
                .Distinct()
                .OrderBy(w => w.Alias);

                return ddata;
            }
            return null;
        }

        public IQueryable<DocumentPlantMapHistoryEntity> GetPlantMapHistoryById(int id)
        {
            var data = _dataContext.GetNoTracking<DocumentPlantMapHistoryEntity>().Where(w => w.Id == id);
            return data;
        }
        public IQueryable<DocumentPlantMapHistoryEntity> GetPlantMapHistoryByIdNew(int id)
        {
            var data = _dataContext.GetAll<DocumentPlantMapHistoryEntity>().ToList().Where(w => w.Id == id);
            return data.AsQueryable();
        }

        public IQueryable<DocumentPlantMapHistoryEntity> GetPlantMapHistoryByIdNewWithIncludes(int id)
        {
            var data = _dataContext.GetAll<DocumentPlantMapHistoryEntity>().Include(d => d.Document).ThenInclude(d => d.Category)
                .Include(d => d.Plant).Include(d => d.Picture).ToList().Where(w => w.Id == id);
            return data.AsQueryable();
        }

        public IQueryable<DocumentPlantMapEntity> GetActiveByPredicate(Expression<Func<DocumentPlantMapEntity, bool>> expression)
        {
            return GetAllNoTracking().Where(w => w.Document.IsActive).Where(expression);
        }

        public DocumentPlantMapEntity GetByIdWithIncludes(int id)
        {
            return GetAllNoTracking()
                //.Where(d => d.IsActive)   // optional
                .Where(d => d.Id == id)
                .Include(d => d.Document)
                .ThenInclude(d => d.Category)
                .Include(d => d.Picture)
                .FirstOrDefault();
        }
    }
}
