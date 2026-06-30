using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class CategoryService
    {
        private readonly IRestClient _restClient;
        private readonly ISessionManager _sessionManager;

        public CategoryService(IRestClient restClient, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _sessionManager = sessionManager;
        }

        public Task<RestReponse<DataSourceResult>> GetAllAsync(DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Category/Get", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse<List<SelectListModel>>> GetCategoryTypes()
        {
            RestRequest reqeust = new RestRequest("Category/CategoryTypes", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<SelectListModel>>(reqeust);
        }

        public Task<RestReponse<List<SelectListModel>>> GetPrimaryCategories()
        {
            RestRequest reqeust = new RestRequest("Category/PrimaryCategories", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<SelectListModel>>(reqeust);
        }

        public Task<RestReponse<List<CategorySelectListModel>>> GetAllCategory(int id)
        {
            RestRequest reqeust = new RestRequest($"Category/AllCategories/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<CategorySelectListModel>>(reqeust);
        }

        public Task<RestReponse<AddEditCategoryModel>> GetCreateModel()
        {
            RestRequest reqeust = new RestRequest("Category/CreateModel", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditCategoryModel>(reqeust);
        }

        public Task<RestReponse> InsertAsync(AddEditCategoryModel addEditCategoryModel)
        {
            RestRequest<AddEditCategoryModel> reqeust = new RestRequest<AddEditCategoryModel>("Category/Post", RestMethodType.Post, addEditCategoryModel, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse<AddEditCategoryModel>> GetEditModel(int id)
        {
            RestRequest reqeust = new RestRequest($"Category/Get/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditCategoryModel>(reqeust);
        }

        public Task<RestReponse> UpdateAsync(AddEditCategoryModel addEditCategoryModel)
        {
            RestRequest<AddEditCategoryModel> reqeust = new RestRequest<AddEditCategoryModel>("Category/Put", RestMethodType.Post, addEditCategoryModel, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse> ChangeStatus(int id, bool status)
        {
            RestRequest reqeust = new RestRequest($"Category/ChangeStatus/{id}/{status}", RestMethodType.Post, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }
        public Task<RestReponse<List<DocumentChartModel>>> GetCategorywiseDocumentCount(int? plantId)
        {
            RestRequest reqeust = new RestRequest($"Category/GetCategorywiseDocumentCount/{plantId}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<DocumentChartModel>>(reqeust);
        }

        public Task<RestReponse<List<DocumentChartModel>>> GetSecondaryCategorywiseDocumentCount(int? plantId)
        {
            RestRequest reqeust = new RestRequest($"Category/GetSecondaryCategorywiseDocumentCount/{plantId}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<DocumentChartModel>>(reqeust);
        }

        public Task<RestReponse<List<DocumentChartModel>>> GetCategorywiseDocumentCountByParentCategory(int categoryId, int? plantId)
        {
            RestRequest reqeust = new RestRequest($"Category/GetCategorywiseDocumentCountByParentCategory/{categoryId}/{plantId}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<DocumentChartModel>>(reqeust);
        }
    }
}
