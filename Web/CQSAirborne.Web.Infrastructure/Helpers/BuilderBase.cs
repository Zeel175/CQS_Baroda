using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;

namespace CQSAirborne.Web.Infrastructure.Helpers
{
    public abstract class BuilderBase
    {
        protected static string GetString(IHtmlContent content)
        {
            using (var writer = new System.IO.StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        protected Dictionary<string, string> ConvertToHtmlAttributes(object source)
        {
            return source.GetType().GetProperties().ToDictionary
                (
                    propInfo => propInfo.Name,
                    propInfo => propInfo.GetValue(source, null).ToString()
                );
        }
    }

    //public static class HelperExtensions
    //{
    //    public static string ToHtmlValue<TModel, TResult>(this Expression<Func<TModel, TResult>> expression, IHtmlHelper<TModel> htmlHelper)
    //    {
    //        return Convert.ToString(expression.Compile()(htmlHelper.ViewData.Model));
    //    }

    //    public static DateTime ToHtmlValue<TModel>(this Expression<Func<TModel, DateTime>> expression, IHtmlHelper<TModel> htmlHelper)
    //    {
    //        return expression.Compile()(htmlHelper.ViewData.Model);
    //    }
    //}

    public static class HelperExtensions
    {
        public static string ToHtmlValue<TModel, TResult>(
            this Expression<Func<TModel, TResult>> expression,
            IHtmlHelper<TModel> htmlHelper)
        {
            return Convert.ToString(expression.Compile()(htmlHelper.ViewData.Model));
        }

        public static DateTime ToHtmlValue<TModel>(
            this Expression<Func<TModel, DateTime>> expression,
            IHtmlHelper<TModel> htmlHelper)
        {
            return expression.Compile()(htmlHelper.ViewData.Model);
        }

        // NEW: Supports nullable DateTime
        public static DateTime? ToHtmlValue<TModel>(
            this Expression<Func<TModel, DateTime?>> expression,
            IHtmlHelper<TModel> htmlHelper)
        {
            return expression.Compile()(htmlHelper.ViewData.Model);
        }
    }

}
