using CQSAirborne.Model.Core;
using CQSAirborne.Model.Plant;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class PlantService
    {
        private readonly IRestClient _restClient;
        private readonly ISessionManager _sessionManager;

        public PlantService(IRestClient restClient, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _sessionManager = sessionManager;
        }

        public Task<RestReponse<DataSourceResult>> GetAllAsync(DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Plant/Get", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public async Task<RestReponse> InsertAsync(AddEditPlantModel addEditPlantModel)
        {
            RestRequest<AddEditPlantModel> reqeust = new RestRequest<AddEditPlantModel>("Plant/Post", RestMethodType.Post, addEditPlantModel, _sessionManager.GetToken());
            var response = await _restClient.ExecutePostAsync(reqeust);
            return response;
        }

        public async Task<RestReponse> UpdateAsync(AddEditPlantModel addEditPlantModel)
        {
            RestRequest<AddEditPlantModel> reqeust = new RestRequest<AddEditPlantModel>("Plant/Put", RestMethodType.Post, addEditPlantModel, _sessionManager.GetToken());
            var response = await _restClient.ExecutePostAsync(reqeust);
            return response;
        }

        public async Task<RestReponse<AddEditPlantModel>> GetByIdAsync(int id)
        {
            RestRequest request = new RestRequest($"Plant/Get/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return await _restClient.ExecuteGetAsync<AddEditPlantModel>(request);
        }

        public async Task<RestReponse<List<SelectListModel>>> GetPlantList()
        {
            RestRequest request = new RestRequest("Plant/GetSelectList", RestMethodType.Get, _sessionManager.GetToken());
            return await _restClient.ExecuteGetAsync<List<SelectListModel>>(request);
        }

        public Task<RestReponse> ChangeStatus(int id, bool status)
        {
            RestRequest reqeust = new RestRequest($"Plant/ChangeStatus/{id}/{status}", RestMethodType.Post, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }
    }
}
