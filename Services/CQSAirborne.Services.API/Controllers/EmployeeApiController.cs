using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Employee;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/Employee")]
    public class EmployeeApiController : BaseController
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeApiController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpPost("[action]/{isManual?}")]
        public IActionResult BulkUpdate(List<AddEditEmployeeViewModel> addEditEmployeeViewModels, bool isManual = false)
        {
            //bool isSuccess = _employeeService.BulkUpdateWithProcedure(addEditEmployeeViewModels, isManual);
            var result = _employeeService.BulkUpdate(addEditEmployeeViewModels);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest();
        }

        [HttpPost("[action]")]
        public IActionResult Post(AddEditEmployeeViewModel addEditEmployeeViewModel)
        {
            bool isSuccess = _employeeService.Insert(addEditEmployeeViewModel);
            if (isSuccess)
                return Ok();
            return BadRequest();
        }

        [HttpGet("[action]/{id}")]
        public IActionResult Get(long id)
        {
            AddEditEmployeeViewModel data = _employeeService.GetById(id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPost("[action]")]
        public IActionResult Put(AddEditEmployeeViewModel addEditEmployeeViewModel)
        {
            bool isSuccess = _employeeService.Update(addEditEmployeeViewModel);
            if (isSuccess)
                return Ok();
            return BadRequest();
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get(DataSourceRequest dataSourceRequest)
        {
            //var data = await _employeeService.GetAllActive()
            //    .ToDataSourceResultAsync(dataSourceRequest);
            //return Ok(data);
            var data = await _employeeService.GetAll()
              .ToDataSourceResultAsync(dataSourceRequest);
            return Ok(data);
        }

        [HttpGet("GetAllEmployees")]
        public IActionResult GetAllEmployees()
        {
            var data = _employeeService.GetAllActive().ToList();
            return Ok(data);
        }
        [HttpGet("GetAllEmployeesWithNoTracking")]
        public IActionResult GetAllEmployeesWithNoTracking()
        {
            var data = _employeeService.GetAll().ToList();
            return Ok(data);
        }

        [HttpGet("[action]/{id}")]
        public IActionResult DeleteEmployee(long id)
        {
            bool isSuccess = _employeeService.DeleteEmployee(id);
            if (isSuccess)
                return Ok();
            return BadRequest();
        }

        [HttpPost("[action]/{id}/{status}")]
        public IActionResult ChangeStatus(int id, bool status)
        {
            bool isSaved = _employeeService.ChangeStatus(id, status);
            if (!isSaved)
                return BadRequest();
            return Ok();
        }

        [HttpPost("GetEmployeesForViewPage")]
        public async Task<IActionResult> GetEmployeesForViewPage(DataSourceRequest dataSourceRequest)
        {
            var data1 = await _employeeService.GetEmployeeListForViewPageAsync();
            var data =  data1.AsQueryable().ToDataSourceResult(dataSourceRequest);
            return Ok(data);
        }
    }
}