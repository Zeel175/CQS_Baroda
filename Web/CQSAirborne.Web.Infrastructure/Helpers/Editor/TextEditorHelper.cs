using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CQSAirborne.Web.Infrastructure.Helpers.Editor
{
    public static class TextEditorHelper
    {
        public static EditorBuilder<TModel> TextEditorFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
            where TModel : class
        {
            string fieldName = (expression.Body as MemberExpression).Member.Name;// expression.Body.
            string value = expression.ToHtmlValue(htmlHelper);
            return new EditorBuilder<TModel>(htmlHelper, fieldName, value);
        }

        public class EditorBuilder<TModel> : BuilderBase
            where TModel : class
        {
            private readonly string _fieldName;
            private readonly IHtmlHelper<TModel> _htmlHelper;
            private Dictionary<string, string> _htmlAttributes;
            private readonly string _fieldValue;

            public EditorBuilder(IHtmlHelper<TModel> htmlHelper, string fieldName, string fieldValue)
            {
                _htmlHelper = htmlHelper;
                _fieldName = fieldName;
                _fieldValue = fieldValue;
            }

            public EditorBuilder<TModel> HtmlAttributes(object htmlAttributes)
            {
                _htmlAttributes = ConvertToHtmlAttributes(htmlAttributes);
                return this;
            }

            public IHtmlContent Render()
            {
                TagBuilder builder = new TagBuilder("textarea");
                builder.Attributes.Add("id", _fieldName);
                builder.Attributes.Add("name", _fieldName);

                if (!string.IsNullOrEmpty(_fieldValue))
                    builder.InnerHtml.Append(_fieldValue);

                if (_htmlAttributes != null)
                {
                    builder.MergeAttributes(_htmlAttributes);
                }

                StringBuilder scriptBuilder = new StringBuilder("<script>");
                scriptBuilder.AppendLine("CKEDITOR.replace( '"+ _fieldName + "' );");
                scriptBuilder.AppendLine("</script>");

                return new HtmlString(GetString(builder) + scriptBuilder.ToString());
            }
        }
    }
}
