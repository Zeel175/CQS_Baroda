using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IDocumentRepository : IBaseRepository<DocumentEntity>
    {
        bool IsDocumentNumberUnique(int id, string documentNumber);
        IQueryable<DocumentEntity> GetByCategory(int id);
        IQueryable<PlantEntity> GetCategoryDocumentPlantList(int categoryId);
        void AddDocumentHistory(DocumentHistoryEntity entity);
        void UpdateDocumentHistory(DocumentHistoryEntity entity);
        IQueryable<DocumentHistoryEntity> GetHistoryByDocumentId(int id);
        DocumentHistoryEntity GetHistoryById(int id);
        IQueryable<DocumentEntity> GetActiveById(int id);
        IQueryable<DocumentHistoryEntity> GetAllDocumentHistory();
        public DocumentEntity GetByIdWithIncludes(int id);
        DocumentHistoryEntity GetHistoryByIdWithInclude(int id);
        DocumentEntity GetByIdWithIncludesForUpdate(int id);
    }
}
