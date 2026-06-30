using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class DashboardService
    {
        private readonly IRestClient _restClient;
        private readonly ISessionManager _sessionManager;

        public DashboardService(IRestClient restClient, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _sessionManager = sessionManager;
        }


        public Task<RestReponse<List<DashboardCategoryModel>>> GetDashboardCategoryAsync(int id, int parentCategoryId)
        {
            RestRequest reqeust = new RestRequest($"Dashboard/GetDashboardCategory/{id}/{parentCategoryId}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<DashboardCategoryModel>>(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetDashboardDocumentsAsync(DataSourceRequest dataSourceRequest, int id)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>($"Dashboard/GetDocuments/{id}", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetQuickSearchDataAsync(QuickSearchRequestModel dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>($"Dashboard/GetQuickSearchData", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetDashboardSubCategoriesAsync(DataSourceRequest dataSourceRequest, int id)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>($"Dashboard/GetSubCategories/{id}", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestFileReponse> GetPdfDocument(int id, int documentOperationType)
        {
            RestRequest restRequest = new RestRequest($"Dashboard/GetPdfDocument/{id}/{documentOperationType}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteFileRequestAsync(restRequest);
        }

        public Task<RestFileReponse> GetDocumentToDownload(int id, int documentOperationType)
        {
            RestRequest restRequest = new RestRequest($"Dashboard/GetDocumentToDownload/{id}/{documentOperationType}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteFileRequestAsync(restRequest);
        }

        public Task<RestReponse<DashboardModel>> GetDashboardModelAsync(int id)
        {
            RestRequest restRequest = new RestRequest($"Dashboard/GetDashboardModel/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<DashboardModel>(restRequest);
        }

        public Task<RestReponse<ViewDocumentModel>> GetViewDocumentModel(int id, int documentOperationType)
        {
            RestRequest restRequest = new RestRequest($"Dashboard/GetViewDocumentModel/{id}/{documentOperationType}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<ViewDocumentModel>(restRequest);
        }

        public Task<RestFileReponse> TemporaryImages(int id)
        {
            RestRequest restRequest = new RestRequest($"Dashboard/TemporaryImages/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteFileRequestAsync(restRequest);
        }
    }
}
