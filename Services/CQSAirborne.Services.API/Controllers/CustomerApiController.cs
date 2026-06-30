using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Customer;
using CQSAirborne.Model.Document;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/Customer")]
    public class CustomerApiController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly IIdentityHelper _identityHelper;

        public CustomerApiController(ICustomerService customerService
            , IIdentityHelper identityHelper)
        {
            _customerService = customerService;
            _identityHelper = identityHelper;
        }


        [HttpPost("CreateEditCustomer")]
        public IActionResult CreateEditCustomer(AddEditCustomerModel model)
        {
            bool isSuccess = _customerService.CreateEdit(model, _identityHelper.UserId);
            if (isSuccess)
                return Ok();
            return BadRequest();
        }

        [HttpGet("GetCustomerById/{Id}")]
        public async Task<IActionResult> GetCustomerById(long Id)
        {
            var data = await _customerService.GetCustomerById(Id);
            if (data != null)
                return Ok(data);
            return BadRequest();
        }


        [HttpPost("Get")]
        public async Task<IActionResult> Get(DataSourceRequest dataSourceRequest)
        {
            var data = await _customerService.GetAll()
                .ToDataSourceResultAsync(dataSourceRequest);
            return Ok(data);
        }

        [HttpPost("CreateEditCustomerDocument")]
        public async Task<IActionResult> CreateEditCustomerDocumentAsync(CustomerDocumentMappingModel model)
        {
            string EncryptedLink = await _customerService.CreateEditCustomerDocumentAsync(model, _identityHelper.UserId);
            return Ok(EncryptedLink);
        }

        [HttpGet("[action]/{id}")]
        public IActionResult DeleteCustomerDocument(long id)
        {
            bool isSuccess = _customerService.DeleteCustomerDocument(id,_identityHelper.UserId);
            if (isSuccess)
                return Ok();
            return BadRequest();
        }

        [HttpGet("[action]/{id}")]
        public IActionResult DeleteCustomer(long id)
        {
            bool isSuccess = _customerService.DeleteCustomer(id, _identityHelper.UserId);
            if (isSuccess)
                return Ok();
            return BadRequest();
        }
    }
}