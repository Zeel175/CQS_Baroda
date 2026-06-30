using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using CQSAirborne.Model.Core;                  // DataSourceRequest
using CQSAirborne.Model.CPRMaster;            // CPRMasterModel, CPRMasterAttachmentDetailModel
using CQSAirborne.Services.API.Extensions;    // ToDataSourceResultAsync
using CQSAirborne.Services.API.Utils;         // IIdentityHelper
using CQSAirborne.Services.Contract;          // ICPRMasterService
using CQSAirborne.Domain;                     // GroupCodeEntity
using Microsoft.EntityFrameworkCore;          // for EF Core
using CQSAirborne.Repository.Contract;
using System;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/CPRMaster")]
    public class CPRMasterApiController : BaseController
    {
        private readonly ICPRMasterService _cprMasterService;
        private readonly ICPRMasterRepository _cPRMasterRepository;
        private readonly IIdentityHelper _identityHelper;

        public CPRMasterApiController(
            ICPRMasterService cprMasterService,
            IIdentityHelper identityHelper
            , ICPRMasterRepository cPRMasterRepository)
        {
            _cprMasterService = cprMasterService;
            _identityHelper = identityHelper;
            _cPRMasterRepository = cPRMasterRepository;
        }

        // POST: api/CPRMaster/CreateEditCPRMaster
        [HttpPost("CreateEditCPRMaster")]
        public async Task<IActionResult> CreateEditCPRMaster(CPRMasterModel model)
        {
            int userId = User.Identity.GetUserId();
            var isSuccess = await _cprMasterService.CreateEditAsync(model, userId);
            return isSuccess ? Ok() : (IActionResult)BadRequest();
        }

        // GET: api/CPRMaster/GetCPRMasterById/5
        [HttpGet("GetCPRMasterById/{id}")]
        public async Task<IActionResult> GetCPRMasterById(long id)
        {
            var data = await _cprMasterService.GetByIdAsync(id);
            return data != null ? Ok(data) : (IActionResult)BadRequest();
        }

        // POST: api/CPRMaster/Get  (server-side paging/filtering ready)
        [HttpPost("Get")]
        public async Task<IActionResult> Get(DataSourceRequest dataSourceRequest)
        {
            var data = await _cprMasterService
                .GetAll()
                .ToDataSourceResultAsync(dataSourceRequest);
            return Ok(data);
        }

        [HttpPost("GetAllBySP")]
        public async Task<IActionResult> GetAllBySP(DataSourceRequest dataSourceRequest)
        {
            try
            {
                int userId = User.Identity.GetUserId();
                var list =  _cPRMasterRepository.GetAllFromSPAsync(userId).Result.ToDataSourceResult(dataSourceRequest);
                return Ok(list);
            }
            catch (Exception ec)
            {
                return null;
            }
        }

        [HttpGet("GetCPRListBySP")]
        public async Task<IActionResult> GetCPRListBySP()
        {
            try
            {
                int userId = User.Identity.GetUserId();
                var list = await _cPRMasterRepository.GetAllFromSPAsync(userId);
                return Ok(list);
            }
            catch (Exception ec)
            {
                return null;
            }
        }


        // GET: api/CPRMaster/DeleteCPRMaster/5
        [HttpGet("DeleteCPRMaster/{id}")]
        public async Task<IActionResult> DeleteCPRMaster(long id)
        {
            int userId = User.Identity.GetUserId();
            var isSuccess = await _cprMasterService.DeleteAsync(id, userId);
            return isSuccess ? Ok() : (IActionResult)BadRequest();
        }

        // GET: api/CPRMaster/GetCPRMasterStatus
        // Returns statuses from adm_GroupCode (ModuleName = 'CPRStatusType')
        [HttpGet("GetCPRMasterStatus")]
        public async Task<IActionResult> GetCPRMasterStatus()
        {
            var list = await _cprMasterService.GetCPRMasterStatusAsync();

            return Ok(list);
        }

        [HttpGet("GetCPRMasterStage")]
        public async Task<IActionResult> GetCPRMasterStage()
        {
            var list = await _cprMasterService.GetCPRMasterStageAsync();

            return Ok(list);
        }


        // POST: api/CPRMaster/UpdateStageStatus
        [HttpPost("UpdateStageStatus")]
        public async Task<IActionResult> UpdateStageStatus(CPRStageStatusUpdateModel model)
        {
            int userId = User.Identity.GetUserId();
            var isSuccess = await _cprMasterService.UpdateStageStatusAsync(model, userId);
            return isSuccess ? Ok() : (IActionResult)BadRequest();
        }

        [HttpGet("GetCPRPrintByIdFromSp/{CPRId}")]
        public async Task<IActionResult> GetCPRPrintByIdFromSp(long CPRId)
        {
            try
            {
                var list = await _cPRMasterRepository.GetCprPrintByIdFromSpAsync(CPRId);
                return Ok(list);
            }
            catch (Exception ec)
            {
                return null;
            }
        }

        [HttpGet("GetAllNonStandardCategories")]
        public async Task<IActionResult> GetAllNonStandardCategories()
        {
            try
            {
                var list = await _cPRMasterRepository.GetAllNonStandardCategoriesAsync();
                return Ok(list);
            }
            catch (Exception ec)
            {
                return null;
            }
        }

        [HttpGet("GetCPRApprovalDetailsByCPRId/{CPRId}")]
        public async Task<IActionResult> GetCPRApprovalDetailsByCPRId(long CPRId)
        {
            var data = await _cPRMasterRepository.GetCPRApprovalDetailsByCPRIdAsync(CPRId);
            return data != null ? Ok(data) : (IActionResult)BadRequest();
        }
    }
}
