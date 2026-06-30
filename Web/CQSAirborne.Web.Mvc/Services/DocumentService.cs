using CQSAirborne.Model.Core;
using CQSAirborne.Model.Document;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Implementation;
using CQSAirborne.Web.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using CQSAirborne.Web.Mvc.Utils;

namespace CQSAirborne.Web.Mvc.Services
{
    public class DocumentService
    {
        private IRestClient _restClient;
        public IConfiguration Configuration { get; }
        public ISessionManager _sessionManager;
        public DocumentService(IConfiguration configuration, IRestClient restClient, ISessionManager sessionManager)
        {
            Configuration = configuration;
            _restClient = restClient;
            _sessionManager = sessionManager;
        }

        public Task<RestReponse<DataSourceResult>> GetAllAsync(DataSourceRequest dataSourceRequest)
        {

            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Document/Get", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }
        public Task<RestReponse<DataSourceResult>> GetAllDocumentViewScreen(DataSourceRequest dataSourceRequest)
        {

            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Document/GetAllDocumentViewScreen", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }
        public Task<RestReponse<AddEditDocumentModel>> GetCreateModelAsync()
        {
            RestRequest reqeust = new RestRequest("Document/CreateModel", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditDocumentModel>(reqeust);
        }

        public Task<RestReponse<AddEditDocumentModel>> GetEditModelAsync(int id)
        {
            RestRequest reqeust = new RestRequest($"Document/Get/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditDocumentModel>(reqeust);
        }

        public Task<RestReponse<AddEditDocumentModel>> GetEditHistoryModelAsync(int id)
        {
            RestRequest reqeust = new RestRequest($"Document/GetDocumentHistoryById/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditDocumentModel>(reqeust);
        }

        public Task<RestReponse<UploadResponse>> UploadAsync(IFormFile file, UploadType uploadType)
        {
            RestRequest<IFormFile> reqeust = new RestRequest<IFormFile>($"Document/Upload/{(int)uploadType}", RestMethodType.Post, file, _sessionManager.GetToken());
            return _restClient.ExecutePostFileAsync<UploadResponse>(reqeust);
        }

        public Task<RestReponse> InsertAsync(AddEditDocumentModel addEditCategoryModel)
        {
            RestRequest<AddEditDocumentModel> reqeust = new RestRequest<AddEditDocumentModel>("Document/Post", RestMethodType.Post, addEditCategoryModel, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse> UpdateAsync(AddEditDocumentModel addEditCategoryModel)
        {
            RestRequest<AddEditDocumentModel> reqeust = new RestRequest<AddEditDocumentModel>("Document/Put", RestMethodType.Post, addEditCategoryModel, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse<List<SelectListModel>>> GetAllDocumentType()
        {
            RestRequest reqeust = new RestRequest($"Document/GetAllDocumentType", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<SelectListModel>>(reqeust);
        }

        public Task<RestReponse<List<PlantSelectListModel>>> GetAllDocumentPlants()
        {
            RestRequest reqeust = new RestRequest($"Document/GetAllDocumentPlants", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<PlantSelectListModel>>(reqeust);
        }

        public Task<RestReponse<List<PlantSelectListModel>>> GetDocumentPlants(int documentId)
        {
            RestRequest reqeust = new RestRequest($"Document/GetDocumentPlants/{documentId}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<PlantSelectListModel>>(reqeust);
        }
        public Task<RestReponse<List<PlantSelectListModel>>> GetDocumentHistoryPlantsNew(int documentId)
        {
            RestRequest reqeust = new RestRequest($"Document/GetDocumentHistoryPlantsNew/{documentId}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<PlantSelectListModel>>(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetDocumentHistoryAll(int id, DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>($"Document/GetHistoryByDocumentId/{id}", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse> ChangeStatus(int id, bool status, bool isNotify = false)
        {
            RestRequest reqeust = new RestRequest($"Document/ChangeStatus/{id}/{status}/{isNotify}", RestMethodType.Post, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }
        public Task<RestReponse> EmailDocument(SendEmailModel sem)
        {
            string apiPath = Configuration.GetValue<string>("HangFireAPIPath");
            var xo = new CQSRestClient(apiPath);
            IServiceCollection serviceProvider = new ServiceCollection();
            var descriptor = serviceProvider.FirstOrDefault(d => d.ServiceType == typeof(IRestClient));
            serviceProvider.Remove(descriptor);
            serviceProvider.AddScoped<IRestClient, CQSRestClient>((s) => new CQSRestClient(apiPath));
            _restClient = new CQSRestClient(apiPath);
            RestRequest<SendEmailModel> reqeust = new RestRequest<SendEmailModel>($"HangFireApi/EmailDocument", RestMethodType.Post, sem, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetAllForExport(DataSourceRequest dataSourceRequest)
        {

            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Document/GetAllForExport", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }
        public Task<RestReponse<DataSourceResult>> GetAllForExportWithSP(DataSourceRequest dataSourceRequest)
        {

            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Document/GetAllForExportWithSP", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }
        public Task<RestReponse> DeleteAsync(int id)
        {
            RestRequest reqeust = new RestRequest($"Document/Delete/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }

        public Task<RestReponse<List<SelectListModel>>> GetAllPrefixDocNumber()
        {
            RestRequest reqeust = new RestRequest($"Document/GetAllPrefixDocNumber", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<SelectListModel>>(reqeust);
        }
        public Task<RestReponse<List<SelectListModel>>> GetAllDocumentList()
        {
            RestRequest reqeust = new RestRequest($"Document/GetAllDocumentList", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<SelectListModel>>(reqeust);
        }

        public Task<RestReponse> InsertDocumentEmailDataAsync(DocumentEmailDataModel model)
        {
            RestRequest<DocumentEmailDataModel> reqeust = new RestRequest<DocumentEmailDataModel>("Document/InsertDocumentEmailData", RestMethodType.Post, model, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }
        public Task<RestReponse<DataSourceResult>> GetPendingEmailDocuments(DataSourceRequest dataSourceRequest)
        {

            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Document/GetPendingEmailDocuments", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetPendingEmailDocumentsBySP(DataSourceRequest dataSourceRequest)
        {

            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Document/GetPendingEmailDocumentsBySP", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse> SendPendingEmail(string ids, string title)
        {
            RestRequest reqeust = new RestRequest($"Document/SendPendingEmail/{ids}/{title}", RestMethodType.Post, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }

        public Task<RestReponse<List<EmailPreviewModel>>> PreviewEmail(EmailPreviewRequest request)
        {
            RestRequest<EmailPreviewRequest> req = new RestRequest<EmailPreviewRequest>(
                "Document/preview-email",
                RestMethodType.Post,
                request,
                _sessionManager.GetToken()
            );

            return _restClient.ExecutePostAsync<EmailPreviewRequest, List<EmailPreviewModel>>(req);
        }
    }
}
