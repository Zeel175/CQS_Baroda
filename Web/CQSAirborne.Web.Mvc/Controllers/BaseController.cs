using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Web.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Web.Mvc.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected void BindErrors(RestReponse restReponse)
        {
            foreach (var error in restReponse.Errors)
            {
                foreach (var errorMessage in error.Errors)
                {
                    ModelState.AddModelError(error.FieldName, errorMessage);
                }
            }
        }
    }

    public static class BaseControllerExtensions
    {
        public static DataSourceResult ToDataSourceResult<TSource>(this List<TSource> sources, DataSourceRequest request)
        {
            DataSourceResult result = new DataSourceResult
            {
                Data = sources,
                Draw = request.Draw,
                RecordsFiltered = sources.Count,
                RecordsTotal = sources.Count
            };

            return result;
        }
    }
}