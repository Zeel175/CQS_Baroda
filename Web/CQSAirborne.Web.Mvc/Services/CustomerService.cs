using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Customer;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class CustomerService
    {
        private readonly IRestClient _restClient;
        private readonly ISessionManager _sessionManager;

        public CustomerService(IRestClient restClient, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _sessionManager = sessionManager;
        }


        public Task<RestReponse> InsertAsync(AddEditCustomerModel model)
        {
            RestRequest<AddEditCustomerModel> reqeust = new RestRequest<AddEditCustomerModel>("Customer/CreateEditCustomer", RestMethodType.Post, model, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse<AddEditCustomerModel>> GetByIdAsync(long id)
        {
            RestRequest reqeust = new RestRequest($"Customer/GetCustomerById/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditCustomerModel>(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetAllAsync(DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Customer/Get", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse> CreateEditCustomerDocument(CustomerDocumentMappingModel model)
        {
            RestRequest<CustomerDocumentMappingModel> reqeust = new RestRequest<CustomerDocumentMappingModel>("Customer/CreateEditCustomerDocument", RestMethodType.Post, model, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }
        
        public Task<RestReponse> DeleteCustomerDocument(long id)
        {
            RestRequest reqeust = new RestRequest($"Customer/DeleteCustomerDocument/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }
        public Task<RestReponse> DeleteCustomer(long id)
        {
            RestRequest reqeust = new RestRequest($"Customer/DeleteCustomer/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }
    }
}
