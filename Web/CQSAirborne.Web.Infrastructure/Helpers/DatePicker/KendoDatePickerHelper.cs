using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CQSAirborne.Web.Infrastructure.Helpers.DatePicker
{
    public static class KendoDatePickerHelper
    {
        public static DatePickerBuilder<TModel> KendoDatePickerFor<TModel>(
       this IHtmlHelper<TModel> htmlHelper,
       Expression<Func<TModel, DateTime?>> expression)
               where TModel : class
        {
            return new DatePickerBuilder<TModel>(htmlHelper, expression);
        }

        public class DatePickerBuilder<TModel> : BuilderBase
            where TModel : class
        {
            private readonly string _fieldName;
            private readonly IHtmlHelper<TModel> _htmlHelper;
            private Dictionary<string, string> _htmlAttributes;
            private string _dateFormat;
            //private readonly Expression<Func<TModel, DateTime>> _expression;
            private readonly Expression<Func<TModel, DateTime?>> _expression;


            //public DatePickerBuilder(IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, DateTime?>> expression)
            //{
            //    _htmlHelper = htmlHelper;
            //    _fieldName = (expression.Body as MemberExpression).Member.Name;
            //    _expression = expression;
            //}
            public DatePickerBuilder(IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, DateTime?>> expression)
            {
                _htmlHelper = htmlHelper;

                var body = expression.Body as MemberExpression;
                if (body == null && expression.Body is UnaryExpression unary && unary.Operand is MemberExpression member)
                {
                    body = member;
                }

                if (body == null)
                    throw new InvalidOperationException("Invalid expression type. Expected a property expression.");

                _fieldName = body.Member.Name;
                _expression = expression;
            }


            public DatePickerBuilder<TModel> HtmlAttributes(object htmlAttributes)
            {
                _htmlAttributes = ConvertToHtmlAttributes(htmlAttributes);
                return this;
            }

            public DatePickerBuilder<TModel> Format(string formatString)
            {
                _dateFormat = formatString;
                return this;
            }

            public IHtmlContent Render()
            {
                //DateTime value = _expression.ToHtmlValue(_htmlHelper);

                //TagBuilder datetimeBuilder = new TagBuilder("input");
                //datetimeBuilder.Attributes.Add("id", _fieldName);
                //datetimeBuilder.Attributes.Add("name", _fieldName);
                //datetimeBuilder.Attributes.Add("value", string.IsNullOrEmpty(_dateFormat) ? value.ToShortDateString() : value.ToString(_dateFormat));
                DateTime? value = _expression.ToHtmlValue(_htmlHelper);

                TagBuilder datetimeBuilder = new TagBuilder("input");
                datetimeBuilder.Attributes.Add("id", _fieldName);
                datetimeBuilder.Attributes.Add("name", _fieldName);
                datetimeBuilder.Attributes.Add("value", value.HasValue
                    ? (string.IsNullOrEmpty(_dateFormat) ? value.Value.ToShortDateString() : value.Value.ToString(_dateFormat))
                    : "");


                if (_htmlAttributes != null)
                {
                    datetimeBuilder.MergeAttributes(_htmlAttributes);
                }
                string element = GetString(datetimeBuilder.RenderSelfClosingTag());
                StringBuilder builder = new StringBuilder("<script>");
                builder.Append("$('#" + _fieldName + "').kendoDatePicker({");
                builder.Append($"format: '{_dateFormat}',");
                builder.Append("});");

                builder.Append("</script>");

                return new HtmlString(element + builder.ToString());
            }
        }
    }
}
