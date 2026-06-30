using CQSAirborne.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IDocumentPlantRepository : IBaseRepository<DocumentPlantMapEntity>
    {
        IQueryable<DocumentPlantMapEntity> GetByDocumentId(int documentId);
        IQueryable<PlantEntity> GetDocumentAllPlants();
        IQueryable<PlantEntity> GetPlantsForDocumentHistory(int id);
        IQueryable<DocumentPlantMapHistoryEntity> GetPlantMapHistoryById(int id);
        IQueryable<DocumentPlantMapHistoryEntity> GetPlantMapHistoryByIdNew(int id);
        IQueryable<DocumentPlantMapHistoryEntity> GetPlantMapHistoryByIdNewWithIncludes(int id);
        IQueryable<DocumentPlantMapEntity> GetActiveByPredicate(Expression<Func<DocumentPlantMapEntity, bool>> expression);
        DocumentPlantMapEntity GetByIdWithIncludes(int id);
    }
}
