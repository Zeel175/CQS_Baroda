using CQSAirborne.Model.Core;
using CQSAirborne.Model.ExternalLink;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/ExternalLink")]
    public class ExternalLinkApiController : BaseController
    {
        private readonly IExternalLinkService _externalLinkService;
        private readonly IIdentityHelper _identityHelper;

        public ExternalLinkApiController(IExternalLinkService externalLinkService
            , IIdentityHelper identityHelper)
        {
            _externalLinkService = externalLinkService;
            _identityHelper = identityHelper;
        }

        [HttpPost("Get")]
        public IActionResult Get(DataSourceRequest dataSourceRequest)
        {
            var data = _externalLinkService.GetAll().ToDataSourceResult(dataSourceRequest);
            return Ok(data);
        }
        [AllowAnonymous]
        [HttpGet("GetAllLinks")]
        public IActionResult GetAllLinks()
        {
            var data = _externalLinkService.GetAll();
            return Ok(data);
        }
        [HttpGet("CreateModel")]
        public IActionResult CreateModel()
        {
            var data = _externalLinkService.GetCreateModel();
            return Ok(data);
        }

        [HttpPost("[action]")]
        public IActionResult Post(AddEditExternalLinkModel addEditExternalLinkModel)
        {
            bool isSaved = _externalLinkService.Insert(addEditExternalLinkModel, _identityHelper.UserId);
            if (!isSaved)
                return BadRequest(AddValidation("", "Unable to save data"));
            return Ok();
        }

        [HttpGet("[action]/{id}")]
        public IActionResult Get(int id)
        {
            var data = _externalLinkService.GetById(id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPost("[action]")]
        public IActionResult Put(AddEditExternalLinkModel addEditExternalLinkModel)
        {
            bool isSaved = _externalLinkService.Update(addEditExternalLinkModel, _identityHelper.UserId);
            if (!isSaved)
                return BadRequest(AddValidation("", "Unable to save data"));
            return Ok();
        }
    }
}