using CQSAirborne.Model.Core;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.ExternalLink;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class ExternalLinkService
    {
        private readonly IRestClient _restClient;
        private const string _controllerName = "ExternalLink";
        private readonly ISessionManager _sessionManager;

        public ExternalLinkService(IRestClient restClient, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _sessionManager = sessionManager;

        }

        public Task<RestReponse<DataSourceResult>> GetAllAsync(DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>($"{_controllerName}/Get", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }
        public async Task<RestReponse<List<AddEditExternalLinkModel>>> GetAllLinksAsync()
        {
            RestRequest reqeust = new RestRequest($"{_controllerName}/GetAllLinks", RestMethodType.Get);
            var response = await _restClient.ExecuteGetAsync<List<AddEditExternalLinkModel>>(reqeust);
            return response;
        }
        public Task<RestReponse<AddEditExternalLinkModel>> GetCreateModelAsync()
        {
            RestRequest reqeust = new RestRequest($"{_controllerName}/CreateModel", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditExternalLinkModel>(reqeust);
        }

        public Task<RestReponse<AddEditExternalLinkModel>> GetEditModelAsync(int id)
        {
            RestRequest reqeust = new RestRequest($"{_controllerName}/Get/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditExternalLinkModel>(reqeust);
        }


        public Task<RestReponse> InsertAsync(AddEditExternalLinkModel addEditCategoryModel)
        {
            RestRequest<AddEditExternalLinkModel> reqeust = new RestRequest<AddEditExternalLinkModel>($"{_controllerName}/Post", RestMethodType.Post, addEditCategoryModel, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse> UpdateAsync(AddEditExternalLinkModel addEditCategoryModel)
        {
            RestRequest<AddEditExternalLinkModel> reqeust = new RestRequest<AddEditExternalLinkModel>($"{_controllerName}/Put", RestMethodType.Post, addEditCategoryModel, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }
    }
}
