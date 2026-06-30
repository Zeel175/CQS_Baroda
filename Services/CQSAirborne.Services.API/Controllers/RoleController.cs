using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Role;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.Contract;
using CQSAirborne.Services.API.Utils;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly IIdentityHelper _identityHelper;

        public RoleController(IRoleService roleService, IIdentityHelper identityHelper)
        {
            _roleService = roleService;
            _identityHelper = identityHelper;
        }

        [Route("CreateEditRole")]
        [HttpPost]
        public async Task<IActionResult> CreateEditRole(AddEditRoleModel model)
        {
            var response = await _roleService.CreateEditAsync(model, _identityHelper.UserId);
            return Ok(response);
        }

        [HttpPost("GetAllRole")]
        public IActionResult GetAllRole(DataSourceRequest dataSourceRequest)
        {
            var data = _roleService.GetAll().ToDataSourceResult(dataSourceRequest);
            return Ok(data);
        }

        [HttpGet("GetByIdAsync/{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var data = await _roleService.GetByIdAsync(id);
            if (data == null)
                return BadRequest();
            return Ok(data);
        }

        [HttpGet("GetRoleSelectList")]
        public IActionResult GetRoleSelectList()
        {
            List<StructureRoleSelectListModel> data = _roleService.GetRoleSelectList();
            if (data == null)
                return BadRequest();
            return Ok(data);
        }

        [HttpGet("GetAccessPermission/{permissionScreenType}/{userId}/{id}")]
        public async Task<IActionResult> GetAccessPermission(PermissionScreenType permissionScreenType, int userId, string id = "")
        {
            var permissions = await _roleService.GetPermissionByType(permissionScreenType, id, userId);
            return Ok(permissions);
        }

        //[HttpGet("DeleteRole/{Id}")]
        //public async Task<IActionResult> DeleteRole(long Id)
        //{
        //    long UserId = _identityHelper.UserId;

        //    var response = await _roleService.DeleteRole(Id, UserId);
        //    return Ok(response);
        //}
    }
}