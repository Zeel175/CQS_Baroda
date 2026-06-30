using CQSAirborne.Model.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Services.API.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var modelState = context.ModelState;
                var errorResponse = modelState.Keys.Select(key => new ValidationResultModel
                {
                    FieldName = key,
                    Errors = modelState[key].Errors.Select(w => w.ErrorMessage).ToList()
                })
                .ToList();
                context.Result = new ValidationFailedResult(errorResponse);
            }
        }
    }

    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(List<ValidationResultModel> modelState)
            : base(modelState)
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
