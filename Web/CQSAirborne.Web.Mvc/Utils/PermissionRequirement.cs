using CQSAirborne.Model.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckAccessAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private string _permission;
        private long _Permissiontype;
        public CheckAccessAttribute(string permission, long PType)
        {
            _permission = permission;
            _Permissiontype = PType;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                return;
            }

            List<dynamic> permissionModel = JsonConvert.DeserializeObject<List<dynamic>>(user.Claims.FirstOrDefault(x => x.Type.Equals("Permission", StringComparison.OrdinalIgnoreCase)).Value);
            string isAdmin = (user.Claims.FirstOrDefault(x => x.Type.Equals("OrgRole", StringComparison.OrdinalIgnoreCase)).Value);

            var success = isAdmin != "Admin" ? CheckAccess(permissionModel) : true;
            if (!success)
            {
                context.Result = new RedirectToRouteResult(
                         new RouteValueDictionary {
                            {"Controller", "Account" },
                            { "Action", "UnAuthorize" }
                         });
                return;
            }
            return;
        }

        public bool CheckAccess(List<dynamic> model)
        {
            if (_Permissiontype == Constants.PermissionTypeConstant.Add)
            {
                return model.Any(m => m.Code == _permission && m.IsAdd == true);
            }
            else if (_Permissiontype == Constants.PermissionTypeConstant.Edit)
            {
                return model.Any(m => m.Code == _permission && m.IsEdit == true);
            }
            else if (_Permissiontype == Constants.PermissionTypeConstant.List)
            {
                return model.Any(m => m.Code == _permission && m.IsList == true);
            }
            else if (_Permissiontype == Constants.PermissionTypeConstant.Delete)
            {
                return model.Any(m => m.Code == _permission && m.IsDelete == true);
            }
            return false;
        }
    }

    public class PermissionChecker
    {
        public static bool CheckAccess(List<dynamic> model, long _Permissiontype, string _permission)
        {
            if (_Permissiontype == Constants.PermissionTypeConstant.Add)
            {
                return model.Any(m => m.Code == _permission && m.IsAdd == true);
            }
            else if (_Permissiontype == Constants.PermissionTypeConstant.Edit)
            {
                return model.Any(m => m.Code == _permission && m.IsEdit == true);
            }
            else if (_Permissiontype == Constants.PermissionTypeConstant.List)
            {
                return model.Any(m => m.Code == _permission && m.IsList == true);
            }
            else if (_Permissiontype == Constants.PermissionTypeConstant.Delete)
            {
                return model.Any(m => m.Code == _permission && m.IsDelete == true);
            }
            return false;
        }
    }
}
