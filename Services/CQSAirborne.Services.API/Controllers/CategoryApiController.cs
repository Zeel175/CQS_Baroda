using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/Category")]
    [ApiController]
    public class CategoryApiController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IIdentityHelper _identityHelper;

        public CategoryApiController(ICategoryService categoryService
            , IIdentityHelper identityHelper)
        {
            _categoryService = categoryService;
            _identityHelper = identityHelper;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get(DataSourceRequest dataSourceRequest)
        {
            var data = await _categoryService.GetAll()
                .ToDataSourceResultAsync(dataSourceRequest);
            return Ok(data);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _categoryService.GetByIdAsync(id);
            return Ok(data);
        }

        [HttpGet("CategoryTypes")]
        public IActionResult CategoryTypes()
        {
            var data = _categoryService.GetCategoryTypes();
            return Ok(data);
        }

        [HttpGet("PrimaryCategories")]
        public IActionResult PrimaryCategories()
        {
            var data = _categoryService.GetPrimaryCategories();
            return Ok(data);
        }

        [HttpGet("AllCategories/{id}")]
        public IActionResult AllCategories(int id)
        {
            var data = _categoryService.GetAllCategories(id);
            return Ok(data);
        }



        [HttpGet("CreateModel")]
        public async Task<IActionResult> CreateModel()
        {
            var data = await _categoryService.GetCreateModelAsync();
            return Ok(data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Post(AddEditCategoryModel addEditCategoryModel)
        {
            bool result = await _categoryService.CreateAsync(addEditCategoryModel, _identityHelper.UserId);
            if (!result)
            {
                return BadRequest(AddValidation("", "Unable to save data"));
            }
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Put(AddEditCategoryModel addEditCategoryModel)
        {
            bool result = await _categoryService.UpdateAsync(addEditCategoryModel, _identityHelper.UserId);
            if (!result)
            {
                return BadRequest(AddValidation("", "Unable to save data"));
            }
            return Ok();
        }

        [HttpPost("[action]/{id}/{status}")]
        public IActionResult ChangeStatus(int id, bool status)
        {
            bool isSaved = _categoryService.ChangeStatus(id, status);
            if (!isSaved)
                return BadRequest();
            return Ok();
        }

        [HttpGet("GetCategorywiseDocumentCount/{plantId}")]
        public async Task<IActionResult> GetCategorywiseDocumentCount(int? plantId)
        {
            int employeeId = User.Identity.GetUserId();
            var data = await _categoryService.GetCategorywiseDocumentCount(employeeId, plantId);
            return Ok(data);
        }

        [HttpGet("GetSecondaryCategorywiseDocumentCount/{plantId}")]
        public async Task<IActionResult> GetSecondaryCategorywiseDocumentCount(int? plantId)
        {
            int employeeId = User.Identity.GetUserId();
            var data = await _categoryService.GetSecondaryCategorywiseDocumentCount(employeeId, plantId);
            return Ok(data);
        }

        [HttpGet("GetCategorywiseDocumentCountByParentCategory/{categoryId}/{plantId}")]
        public async Task<IActionResult> GetCategorywiseDocumentCountByParentCategory(int categoryId, int? plantId)
        {
            int employeeId = User.Identity.GetUserId();
            var data = await _categoryService.GetCategorywiseDocumentCountByParentCategory(categoryId, employeeId, plantId);
            return Ok(data);
        }
    }
}