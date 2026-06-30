using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Plant;
using CQSAirborne.Model.Role;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    //[Authorize]
    public class RoleController : BaseController
    {
        private readonly RoleService _roleService;
        public RoleController(RoleService roleRestService)
        {
            _roleService = roleRestService;
        }

        [CheckAccess(ScreenCode.Role, PermissionTypeConstant.List)]
        public IActionResult Index()
        {
            List<AssignPermissionViewModel> permissionModel = JsonConvert.DeserializeObject<List<AssignPermissionViewModel>>(User.Claims.FirstOrDefault(x => x.Type.Equals("Permission", StringComparison.OrdinalIgnoreCase)).Value);
            string isAdmin = (User.Claims.FirstOrDefault(x => x.Type.Equals("OrgRole", StringComparison.OrdinalIgnoreCase)).Value);

            //bool IsDelete = true;
            //if (!isAdmin)
            //{
            //    IsDelete = permissionModel.Any(m => m.Code == ScreenCode.User && m.IsDelete == true);
            //}

            ViewBag.IsDelete = "";// IsDelete == true ? "" : "none";

            return View();
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> GetRoleData(DataSourceRequest dataSourceRequest)
        {
            var result = await _roleService.GetAllRoleAsync(dataSourceRequest);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest();
        }

        //[Authorize]
        [CheckAccess(ScreenCode.Role, PermissionTypeConstant.Add)]
        public IActionResult Create()
        {
            AddEditRoleModel model = new AddEditRoleModel();
            return View("CreateEdit",model);
        }

        //[Authorize]
        [CheckAccess(ScreenCode.Role, PermissionTypeConstant.Edit)]
        public async Task<IActionResult> Edit(int Id)
        {
            var response = await _roleService.GetEditModelAsync(Id);
            if (response.IsSuccess)
            {
                return View("CreateEdit", response.Data);
            }
            return BadRequest();
        }

        //[Authorize]
        //[CheckAccess(ScreenCode.Role, PermissionTypeConstant.Delete)]
        //[HttpPost]
        //public async Task<ActionResult> deleteRole(string Id)
        //{
        //    var data= await _roleService.DeleteRole(Convert.ToInt64(Id));
        //    if (data.IsSuccess)
        //    {
        //        return Ok(data.Data);
        //    }
        //    return BadRequest();
        //}

        [HttpPost]
        public async Task<IActionResult> Create(AddEditRoleModel model)
        {
            model.Permissions = JsonConvert.DeserializeObject<List<AssignPermissionViewModel>>(model.PermissionData);
            var response = await _roleService.CreateEditAsync(model);
            if (response.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}