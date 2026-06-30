using CQSAirborne.Model.Core;
using CQSAirborne.Model.CPRMaster;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class CPRMasterService
    {
        private readonly IRestClient _restClient;
        private readonly ISessionManager _sessionManager;

        public CPRMasterService(IRestClient restClient, ISessionManager sessionManager)
        {
            _restClient = restClient;
            _sessionManager = sessionManager;
        }

        // Create / Edit (POST: api/CPRMaster/CreateEditCPRMaster)
        public Task<RestReponse> InsertAsync(CPRMasterModel model)
        {
            var request = new RestRequest<CPRMasterModel>(
                "CPRMaster/CreateEditCPRMaster",
                RestMethodType.Post,
                model,
                _sessionManager.GetToken());

            return _restClient.ExecutePostAsync(request);
        }

        // Get by Id (GET: api/CPRMaster/GetCPRMasterById/{id})
        public Task<RestReponse<CPRMasterModel>> GetByIdAsync(long id)
        {
            var request = new RestRequest(
                $"CPRMaster/GetCPRMasterById/{id}",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteGetAsync<CPRMasterModel>(request);
        }

        // List with server-side paging/filtering (POST: api/CPRMaster/Get)
        public Task<RestReponse<DataSourceResult>> GetAllAsync(DataSourceRequest dataSourceRequest)
        {
            var request = new RestRequest<DataSourceRequest>(
                "CPRMaster/Get",
                RestMethodType.Post,
                dataSourceRequest,
                _sessionManager.GetToken());

            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(request);
        }
        public Task<RestReponse<DataSourceResult>> GetAllBySPAsync(DataSourceRequest dataSourceRequest)
        {
            var request = new RestRequest<DataSourceRequest>(
                "CPRMaster/GetAllBySP",
                RestMethodType.Post,
                dataSourceRequest,
                _sessionManager.GetToken());

            return _restClient.ExecutePostAsync<DataSourceRequest, DataSourceResult>(request);
        }

        // Delete CPR (soft delete) (GET: api/CPRMaster/DeleteCPRMaster/{id})
        public Task<RestReponse> DeleteAsync(long id)
        {
            var request = new RestRequest(
                $"CPRMaster/DeleteCPRMaster/{id}",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteAsync(request);
        }

        public Task<RestReponse<List<SelectListModel>>> GetCPRMasterStatus()
        {
            var request = new RestRequest(
                $"CPRMaster/GetCPRMasterStatus",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteGetAsync<List<SelectListModel>>(request);
        }

        public Task<RestReponse<List<SelectListModel>>> GetCPRMasterStage()
        {
            var request = new RestRequest(
                $"CPRMaster/GetCPRMasterStage",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteGetAsync<List<SelectListModel>>(request);
        }

        // Update Status (POST: api/CPRMaster/UpdateStageStatus)
        public Task<RestReponse> UpdateStageStatus(CPRStageStatusUpdateModel model)
        {
            var request = new RestRequest<CPRStageStatusUpdateModel>(
                "CPRMaster/UpdateStageStatus",
                RestMethodType.Post,
                model,
                _sessionManager.GetToken());

            return _restClient.ExecutePostAsync(request);
        }

        public Task<RestReponse<CPRPrintModel>> GetCPRPrintByIdFromSp(long CPRId)
        {
            var request = new RestRequest(
                $"CPRMaster/GetCPRPrintByIdFromSp/{CPRId}",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteGetAsync<CPRPrintModel>(request);
        }

        public Task<RestReponse<List<NonStandardCategoryModel>>> GetAllNonStandardCategories()
        {
            var request = new RestRequest(
                $"CPRMaster/GetAllNonStandardCategories",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteGetAsync<List<NonStandardCategoryModel>>(request);
        }

        public Task<RestReponse<List<CPRApprovalDetailsModel>>> GetCPRApprovalDetailsByCPRId(long CPRId)
        {
            var request = new RestRequest(
                $"CPRMaster/GetCPRApprovalDetailsByCPRId/{CPRId}",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteGetAsync<List<CPRApprovalDetailsModel>>(request);
        }

        public Task<RestReponse<List<CPRMasterModel>>> GetCPRListBySP()
        {
            var request = new RestRequest(
                $"CPRMaster/GetCPRListBySP",
                RestMethodType.Get,
                _sessionManager.GetToken());

            return _restClient.ExecuteGetAsync<List<CPRMasterModel>>(request);
        }
    }
}
