using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Utils
{
    public static class HtmlHelper
    {
            public static IHtmlContent EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> modelExpression, string firstElement)
            {
                var typeOfProperty = modelExpression.ReturnType;
                if (!typeOfProperty.IsEnum)
                    throw new ArgumentException(string.Format("Type {0} is not an enum", typeOfProperty));
                var enumValues = new SelectList(Enum.GetValues(typeOfProperty));
                return htmlHelper.DropDownListFor(modelExpression, enumValues, firstElement);
            }
    }
}
