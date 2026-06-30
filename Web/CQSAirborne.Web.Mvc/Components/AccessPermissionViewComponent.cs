using CQSAirborne.Model.Core;
using CQSAirborne.Web.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Components
{
    [ViewComponent(Name = "AccessPermission")]
    public class AccessPermissionViewComponent : ViewComponent
    {
        private readonly RoleService _permissionRestService;
        public AccessPermissionViewComponent(RoleService permissionRestService)
        {
            _permissionRestService = permissionRestService;
        }

        public async Task<IViewComponentResult> InvokeAsync(PermissionScreenType permissionScreenType, string id, int userId = 0)
        {
            var response = await _permissionRestService.GetAccessPermission(permissionScreenType, id, userId);
            if (response.IsSuccess)
            {
                return View("_AccessPermission", response.Data);
            }
            return View("_AccessPermission");
        }
    }
}
