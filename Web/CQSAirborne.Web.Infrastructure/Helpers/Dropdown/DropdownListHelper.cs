using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;

namespace CQSAirborne.Web.Infrastructure.Helpers.Dropdown
{
    public static class DropdownListHelper
    {
        public static DropdownBuilder<TModel, TResult> KendoDropDownListFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
            where TModel : class
        {
            //string fieldName = (expression.Body as MemberExpression).Member.Name;// expression.Body.

            return new DropdownBuilder<TModel, TResult>(htmlHelper, expression);
        }

        public class DropdownBuilder<TModel, TResult> : BuilderBase
            where TModel : class
        {
            private string readUrl;
            private string dataTextField;
            private string dataValueField;
            private string optionLabel;
            private string filter;
            private string _changeEventHandler;
            private string _dataBoundEventHandler;
            private readonly string _fieldName;
            private readonly IHtmlHelper<TModel> _htmlHelper;
            private IDictionary<string, string> _htmlAttributes;
            private readonly Expression<Func<TModel, TResult>> _expression;

            public DropdownBuilder(IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
            {
                _htmlHelper = htmlHelper;
                _fieldName = (expression.Body as MemberExpression).Member.Name;
                _expression = expression;
            }

            public DropdownBuilder<TModel, TResult> Read(string url)
            {
                readUrl = url;
                return this;
            }

            public DropdownBuilder<TModel, TResult> DataTextField(string fieldName)
            {
                dataTextField = fieldName;
                return this;
            }

            public DropdownBuilder<TModel, TResult> DataValueField(string fieldName)
            {
                dataValueField = fieldName;
                return this;
            }

            public DropdownBuilder<TModel, TResult> OptionLabel(string labelName)
            {
                optionLabel = labelName;
                return this;
            }
            public DropdownBuilder<TModel, TResult> Filter(string name)
            {
                filter = name;
                return this;
            }

            public DropdownBuilder<TModel, TResult> OnChange(string changeEventHandler)
            {
                _changeEventHandler = changeEventHandler;
                return this;
            }

            public DropdownBuilder<TModel, TResult> OnDataBound(string dataBoundEventHandler)
            {
                _dataBoundEventHandler = dataBoundEventHandler;
                return this;
            }

            public DropdownBuilder<TModel, TResult> HtmlAttributes(object htmlAttributes)
            {
                _htmlAttributes = ConvertToHtmlAttributes(htmlAttributes);
                return this;
            }

            public IHtmlContent Render()
            {
                var value = _expression.Compile()(_htmlHelper.ViewData.Model);


                TagBuilder element = new TagBuilder("input");
                element.Attributes.Add("id", _fieldName);
                element.Attributes.Add("name", _fieldName);
                element.Attributes.Add("value", value.ToString());

                if (_htmlAttributes != null)
                {
                    element.MergeAttributes(_htmlAttributes);
                }

                StringBuilder builder = new StringBuilder();

                builder.Append("<script>");
                builder.Append("$('#" + _fieldName + "').kendoDropDownList({");
                builder.Append("dataTextField: '" + dataTextField + "',");
                builder.Append("dataValueField: '" + dataValueField + "',");
                if (!string.IsNullOrEmpty(optionLabel))
                    builder.Append("optionLabel: '" + optionLabel + "',");
                if (!string.IsNullOrEmpty(_changeEventHandler))
                    builder.Append("change: " + _changeEventHandler + ",");

                if (!string.IsNullOrEmpty(_dataBoundEventHandler))
                    builder.Append("dataBound: " + _dataBoundEventHandler + ",");

                if (!string.IsNullOrEmpty(filter))
                    builder.Append("filter: 'contains',");
                builder.Append("dataSource: { transport: { read: { dataType: 'json', url: '" + readUrl + "', } } }");
                
                builder.Append("});");

                builder.Append("</script>");

                string elementToRender = GetString(element) + Environment.NewLine + builder.ToString();
                return new HtmlString(elementToRender);
            }


        }
    }



}
