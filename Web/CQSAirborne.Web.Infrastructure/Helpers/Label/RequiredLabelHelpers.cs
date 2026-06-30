using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;

namespace CQSAirborne.Web.Infrastructure.Helpers.Label
{
    public static class RequiredLabelHelpers
    {
        public static IHtmlContent RequiredLabelFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression, object htmlAttribute = null)
           where TModel : class
        {
            var labelString = htmlHelper.LabelFor(expression, htmlAttribute);
            StringBuilder labelBuilder = new StringBuilder();
            using (var writer = new System.IO.StringWriter())
            {
                labelString.WriteTo(writer, HtmlEncoder.Default);
                labelBuilder.Append(writer.ToString());
            }
            labelBuilder.AppendLine("<span class='asterick'>*</span>");
            return new HtmlString(labelBuilder.ToString());
        }
    }
}
