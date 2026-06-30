using CQSAirborne.Domain;
using CQSAirborne.Model.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface ICategoryRepository : IBaseRepository<CategoryEntity>
    {
        IQueryable<CategoryEntity> GetPrimaryCategories();
        IQueryable<CategoryEntity> GetAllCategory(int id);
        IQueryable<CategoryEntity> GetByParentCategory(int id);
        IQueryable<CategoryEntity> GetSubCategories(int id);
        IQueryable<ViewQuickSearch> GetQuickSearches();
        bool IsCategryNameUnique(string categoryName, int id);
        bool IsCategryDisplayOrderUnique(int displayOrder, int id, int? primaryCategoryId);
        Task<List<DocumentChartModel>> GetPrimaryCategorywiseDocumentCount(long? employeeId, int? plantId);
        Task<List<DocumentChartModel>> GetSecondaryCategorywiseDocumentCount(long? employeeId, int? plantId);
        Task<List<DocumentChartModel>> GetDocumentCountByCategoryId(int categoryId, long? employeeId, int? plantId);
        Task<List<DocumentListModel>> GetAllPrefixDocNumber();
        Task<List<DocumentListModel>> GetAllDocumentList();
        Task<List<DocumentListModel>> GetFilteredDocumentsForExportAsync(int statusId, string docTypePrefix, long? employeeId);
        Task<List<DocumentListModel>> GetAllDocumentViewScreenAsync(int employeeId);
    }
}
