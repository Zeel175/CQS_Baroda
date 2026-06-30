using CQSAirborne.Model.Core;
using CQSAirborne.Services.API.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CQSAirborne.Services.API.Controllers
{
    [ApiController]
    [ValidateModel]
    public class BaseController : ControllerBase
    {
        [NonAction]
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<ValidationResultModel> AddValidation(string fieldName, string message)
        {
            return new List<ValidationResultModel>
            {
                new ValidationResultModel
                {
                    FieldName = fieldName,
                    Errors = new List<string>{ message }
                }
            };
        }
    }
}