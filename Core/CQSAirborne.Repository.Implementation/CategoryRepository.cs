using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Model.Document;
using CQSAirborne.Repository.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class CategoryRepository : BaseRepository<CategoryEntity>, ICategoryRepository
    {
        private readonly IDataContext _dataContext;
        private readonly IStoredProcedureContext _storedProcedureContext;
        public CategoryRepository(IDataContext dataContext, IStoredProcedureContext storedProcedureContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
            _storedProcedureContext = storedProcedureContext;
        }
        public async Task<List<DocumentChartModel>> GetPrimaryCategorywiseDocumentCount(long? employeeId, int? plantId)
        {
            return await _storedProcedureContext.GetDocumentChart(employeeId, plantId);
        }
        public async Task<List<DocumentChartModel>> GetSecondaryCategorywiseDocumentCount(long? employeeId, int? plantId)
        {
            return await _storedProcedureContext.GetSecondaryDocumentChart(employeeId, plantId);
        }
        public async Task<List<DocumentChartModel>> GetDocumentCountByCategoryId(int categoryId, long? employeeId, int? plantId)
        {
            return await _storedProcedureContext.GetDocumentChartByCategoryId(categoryId, employeeId, plantId);
        }
        public async Task<List<DocumentListModel>> GetAllPrefixDocNumber()
        {
            return await _storedProcedureContext.GetAllPrefixDocNumber();
        }
        public async Task<List<DocumentListModel>> GetAllDocumentList()
        {
            return await _storedProcedureContext.GetAllDocumentList();
        }
        public async Task<List<DocumentListModel>> GetFilteredDocumentsForExportAsync(int statusId, string docTypePrefix, long? employeeId)
        {
            return await _storedProcedureContext.GetFilteredDocumentsForExportAsync(statusId, docTypePrefix,employeeId);
        }

        public async Task<List<DocumentListModel>> GetAllDocumentViewScreenAsync(int employeeId)
        {
            return await _storedProcedureContext.GetAllDocumentViewScreenAsync(employeeId);
        }
        public IQueryable<CategoryEntity> GetPrimaryCategories()
        {
            return GetAllNoTracking()
                .Where(w => w.PrimaryCategoryId == null && w.IsActive);
        }

        public IQueryable<CategoryEntity> GetAllCategory(int id)
        {
            return GetAllNoTracking()
                .Where(w => w.Id != id && w.IsActive);
        }

        public IQueryable<CategoryEntity> GetByParentCategory(int id)
        {
            if (id > 0)
                return GetAllNoTracking().Where(w => w.Id == id && w.IsActive)
                    .OrderBy(w => w.DisplayOrder);
            return GetAllNoTracking()
                .Where(w => w.PrimaryCategoryId == null && w.IsActive)
                .OrderBy(w => w.DisplayOrder);
        }

        public IQueryable<CategoryEntity> GetSubCategories(int id)
        {
            return GetAllNoTracking()
                .Where(w => w.PrimaryCategoryId == id && w.IsActive)
                .OrderBy(w => w.DisplayOrder);
        }

        public IQueryable<ViewQuickSearch> GetQuickSearches()
        {
            return _dataContext.GetNoTracking<ViewQuickSearch>().Where(w => w.IsActive
            && w.DbName != "DocumentCode"
            );
        }

        public bool IsCategryNameUnique(string categoryName, int id)
        {
            return GetAllNoTracking()
                .Where(w => w.Id != id && w.Name == categoryName).Count() == 0;
        }

        public bool IsCategryDisplayOrderUnique(int displayOrder, int id, int? primaryCategoryId)
        {
            return GetAllNoTracking()
                .Where(w => w.Id != id &&
                w.PrimaryCategoryId == primaryCategoryId
                && w.DisplayOrder == displayOrder
                ).Count() == 0;
        }
    }
}
