using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface ICategoryService
    {
        IQueryable<CategoryListModel> GetAll();

        List<SelectListModel> GetCategoryTypes();

        List<SelectListModel> GetPrimaryCategories();
        List<CategorySelectListModel> GetAllCategories(int id);
        bool IsCategryNameUnique(string categoryName, int id);
        Task<AddEditCategoryModel> GetCreateModelAsync();
        Task<bool> CreateAsync(AddEditCategoryModel addEditCategoryModel, int userId);
        bool IsCategryDisplayOrderUnique(int displayOrder, int id, int? primaryCategory);
        Task<AddEditCategoryModel> GetByIdAsync(int id);
        Task<bool> UpdateAsync(AddEditCategoryModel addEditCategoryModel, int userId);
        bool ChangeStatus(int id, bool status);
        Task<List<DocumentChartModel>> GetCategorywiseDocumentCount(long? employeeId, int? plantId);
        Task<List<DocumentChartModel>> GetSecondaryCategorywiseDocumentCount(long? employeeId, int? plantId);
        Task<List<DocumentChartModel>> GetCategorywiseDocumentCountByParentCategory(int categoryId, long? employeeId, int? plantId);
        bool CategoryIsRestricted(int Id);
    }
}
