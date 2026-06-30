using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CQSAirborne.Web.Infrastructure.Helpers.MultiSelect
{
    public static class KendoMultiSelectHelper
    {
        public static MultiSelectBuilder<TModel, TResult> KendoMultiSelectFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, List<TResult>>> expression)
            where TModel : class
        {
            //string fieldName = (expression.Body as MemberExpression).Member.Name;// expression.Body.

            return new MultiSelectBuilder<TModel, TResult>(htmlHelper, expression);
        }

        public class MultiSelectBuilder<TModel, TResult> : BuilderBase
            where TModel : class
        {
            private readonly string _fieldName;
            private readonly IHtmlHelper<TModel> _htmlHelper;
            private Dictionary<string, string> _htmlAttributes;
            private string readUrl;
            private string dataTextField;
            private string dataValueField;
            private string _placeHolderText;
            private string _changeEventHandler;
            private readonly Expression<Func<TModel, List<TResult>>> _expression;

            public MultiSelectBuilder(IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, List<TResult>>> expression)
            {
                _htmlHelper = htmlHelper;
                _fieldName = (expression.Body as MemberExpression).Member.Name;
                _expression = expression;
            }

            public MultiSelectBuilder<TModel, TResult> HtmlAttributes(object htmlAttributes)
            {
                _htmlAttributes = ConvertToHtmlAttributes(htmlAttributes);
                return this;
            }

            public MultiSelectBuilder<TModel, TResult> Read(string url)
            {
                readUrl = url;
                return this;
            }

            public MultiSelectBuilder<TModel, TResult> DataTextField(string fieldName)
            {
                dataTextField = fieldName;
                return this;
            }

            public MultiSelectBuilder<TModel, TResult> DataValueField(string fieldName)
            {
                dataValueField = fieldName;
                return this;
            }

            public MultiSelectBuilder<TModel, TResult> OnChange(string changeEventHandler)
            {
                _changeEventHandler = changeEventHandler;
                return this;
            }

            public MultiSelectBuilder<TModel, TResult> PlaceHolder(string placeHolderText)
            {
                _placeHolderText = placeHolderText;
                return this;
            }

            public IHtmlContent Render()
            {
                string value = JsonConvert.SerializeObject(_expression.Compile()(_htmlHelper.ViewData.Model));


                TagBuilder multiSelectBuilder = new TagBuilder("select");
                multiSelectBuilder.Attributes.Add("id", _fieldName);
                multiSelectBuilder.Attributes.Add("name", _fieldName);

                if (_htmlAttributes != null)
                {
                    multiSelectBuilder.MergeAttributes(_htmlAttributes);
                }
                string element = GetString(multiSelectBuilder);
                StringBuilder builder = new StringBuilder("<script>");
                builder.Append("$('#" + _fieldName + "').kendoMultiSelect({");
                builder.Append("dataSource: { transport: { read: { dataType: 'json', url: '" + readUrl + "', } } },");
                builder.Append("dataTextField: '" + dataTextField + "',");
                builder.Append("dataValueField: '" + dataValueField + "',");
                builder.Append("value: " + value + ",");
                builder.Append("tagMode: 'single',");

                if (!string.IsNullOrEmpty(_changeEventHandler))
                    builder.Append("change: " + _changeEventHandler + ",");

                if (!string.IsNullOrEmpty(_placeHolderText))
                    builder.Append("placeholder: '" + _placeHolderText + "',");


                builder.Append("});");

                builder.Append("</script>");

                return new HtmlString(element + builder.ToString());
            }
        }
    }
}
