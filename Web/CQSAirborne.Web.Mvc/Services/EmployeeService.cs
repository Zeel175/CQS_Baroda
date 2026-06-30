using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Employee;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class EmployeeService
    {
        private readonly IRestClient _restClient;
        private readonly ISessionManager _sessionManager;

        public EmployeeService(IRestClient restClient, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _sessionManager = sessionManager;
        }

        public Task<RestReponse<DataSourceResult>> GetAllAsync(DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Employee/Get", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }

        public Task<RestReponse> InsertAsync(AddEditEmployeeViewModel model)
        {
            RestRequest<AddEditEmployeeViewModel> reqeust = new RestRequest<AddEditEmployeeViewModel>("Employee/Post", RestMethodType.Post, model, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse> UpdateAsync(AddEditEmployeeViewModel model)
        {
            RestRequest<AddEditEmployeeViewModel> reqeust = new RestRequest<AddEditEmployeeViewModel>("Employee/Put", RestMethodType.Post, model, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse<AddEditEmployeeViewModel>> GetByIdAsync(long id)
        {
            RestRequest reqeust = new RestRequest($"Employee/Get/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditEmployeeViewModel>(reqeust);
        }

        public Task<RestReponse<List<EmployeeListViewModel>>> GetAllEmployees()
        {
            RestRequest reqeust = new RestRequest($"Employee/GetAllEmployees", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<EmployeeListViewModel>>(reqeust);
        }

        public Task<RestReponse<List<EmployeeListViewModel>>> GetAllEmployeesWithNoTracking()
        {
            RestRequest reqeust = new RestRequest($"Employee/GetAllEmployeesWithNoTracking", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<EmployeeListViewModel>>(reqeust);
        }
        //public Task<RestReponse> InsertBulkEmployee(List<AddEditEmployeeViewModel> employeeData)
        //{
        //    RestRequest<List<AddEditEmployeeViewModel>> reqeust = new RestRequest<List<AddEditEmployeeViewModel>>($"Employee/BulkUpdate/{true}", RestMethodType.Post, employeeData, _sessionManager.GetToken());
        //    return _restClient.ExecutePostAsync(reqeust);
        //}
        public Task<RestReponse<BulkUploadResponseModel>> InsertBulkEmployee(
      List<AddEditEmployeeViewModel> employeeData)
        {
            RestRequest<List<AddEditEmployeeViewModel>> request =
                new RestRequest<List<AddEditEmployeeViewModel>>(
                    $"Employee/BulkUpdate/{true}",
                    RestMethodType.Post,
                    employeeData,
                    _sessionManager.GetToken()
                );

            // ✅ Same Pattern as GetAllAsync
            return _restClient.ExecutePostAsync<List<AddEditEmployeeViewModel>, BulkUploadResponseModel>(request);
        }


        public Task<RestReponse> DeleteEmployee(long id)
        {
            RestRequest reqeust = new RestRequest($"Employee/DeleteEmployee/{id}", RestMethodType.Get, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }
        public Task<RestReponse> ChangeStatus(int id, bool status)
        {
            RestRequest reqeust = new RestRequest($"Employee/ChangeStatus/{id}/{status}", RestMethodType.Post, _sessionManager.GetToken());
            return _restClient.ExecuteAsync(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetEmployeesForViewPageAsync(DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> reqeust = new RestRequest<DataSourceRequest>("Employee/GetEmployeesForViewPage", RestMethodType.Post, dataSourceRequest, _sessionManager.GetToken());
            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(reqeust);
        }
    }
}
