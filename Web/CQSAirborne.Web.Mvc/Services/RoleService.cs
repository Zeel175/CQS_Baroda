using CQSAirborne.Web.Infrastructure.Contracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Role;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;

namespace CQSAirborne.Web.Mvc.Services
{
    public class RoleService 
    {
        private readonly string BASEURL;
        private readonly IRestClient _restClient;
        private readonly IConfiguration _configuration;
        private readonly ISessionManager _sessionManager;

        public RoleService(IRestClient restClient, IConfiguration configuration, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _configuration = configuration;
            //BASEURL = _configuration.GetSection("BaseApiPath").Value;
            _sessionManager = sessionManager;
        }

        public Task<RestReponse> CreateEditAsync(AddEditRoleModel model)
        {
            RestRequest<AddEditRoleModel> reqeust = new RestRequest<AddEditRoleModel>($"Role/CreateEditRole", RestMethodType.Post, model,  _sessionManager.GetToken());
            return _restClient.ExecutePostAsync(reqeust);
        }

        public Task<RestReponse<DataSourceResult>> GetAllRoleAsync(DataSourceRequest dataSourceRequest)
        {
            RestRequest<DataSourceRequest> restRequest =
                new RestRequest<DataSourceRequest>($"Role/GetAllRole", RestMethodType.Post, dataSourceRequest,  _sessionManager.GetToken());

            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(restRequest);
        }

        public Task<RestReponse<AddEditRoleModel>> GetEditModelAsync(int id)
        {
            RestRequest reqeust = new RestRequest($"Role/GetByIdAsync/{id}", RestMethodType.Get,  _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<AddEditRoleModel>(reqeust);
        }

        public Task<RestReponse<List<StructureRoleSelectListModel>>> GetRoleList()
        {
            RestRequest reqeust = new RestRequest($"Role/GetRoleSelectList", RestMethodType.Get,  _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<StructureRoleSelectListModel>>(reqeust);
        }

        public Task<RestReponse<List<AssignPermissionViewModel>>> GetAccessPermission(PermissionScreenType permissionScreenType, string id, int userId)
        {
            RestRequest reqeust = new RestRequest($"Role/GetAccessPermission/{(int)permissionScreenType}/{userId}/{id}", RestMethodType.Get,  _sessionManager.GetToken());
            return _restClient.ExecuteGetAsync<List<AssignPermissionViewModel>>(reqeust);
        }

        //public Task<RestReponse<LoginResult>> DeleteRole(int Id)
        //{
        //    RestRequest reqeust = new RestRequest($"{BASEURL}Role/DeleteRole/{Id}", RestMethodType.Get,  _sessionManager.GetToken());
        //    return _restClient.ExecuteGetAsync<LoginResult>(reqeust);
        //}
    }
}
