using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.QuickSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IDashboardService
    {
        List<DashboardCategoryModel> GetDashboardCategory(int id, int parentCategoryId);
        IQueryable<DocumentListModel> GetDocuments(int id);
        IQueryable<CategoryListModel> GetSubCategories(int id);
        DocumentDetailModel GetDocumentById(int id, DocumentOperationType documentOperationType);
        DocumentDetailModel GetPictureDetails(long id);
        DataSourceResult GetQuickSearchData(QuickSearchRequestModel dataSourceRequest, int userid);
        DashboardModel GetCreateModel(int id);
        ViewDocumentModel GetViewDocumentModel(int id, DocumentOperationType documentOperationType);
        List<ClickablePathModel> GetClickablePathForCategory(int id);
    }
}
