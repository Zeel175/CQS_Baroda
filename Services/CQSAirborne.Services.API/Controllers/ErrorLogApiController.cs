using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.ErrorLog;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Services.API.Controllers
{
    [Route("api/ErrorLog")]
    [ApiController]
    public class ErrorLogApiController : BaseController
    {
        private readonly IErrorLogService _errorLogService;
        private readonly IIdentityHelper _identityHelper;

        public ErrorLogApiController(IErrorLogService errorLogService
            , IIdentityHelper identityHelper)
        {
            _errorLogService = errorLogService;
            _identityHelper = identityHelper;
        }

        [HttpPost("[action]")]
        public IActionResult Post(AddEditErrorLogModel addEditErrorLogModel)
        {
            _errorLogService.Insert(addEditErrorLogModel);
            return Ok();
        }
    }
}