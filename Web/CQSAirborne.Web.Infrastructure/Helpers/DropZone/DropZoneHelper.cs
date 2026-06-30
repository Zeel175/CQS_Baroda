using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CQSAirborne.Web.Infrastructure.Helpers.DropZone
{
    public static class DropZoneHelper
    {
        public static DropZoneBuilder<TModel> UploadFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, int>> expression)
            where TModel : class
        {
            string fieldName = (expression.Body as MemberExpression).Member.Name;// expression.Body.
            string value = expression.ToHtmlValue(htmlHelper);
            return new DropZoneBuilder<TModel>(htmlHelper, fieldName, value);
        }

        public static DropZoneBuilder<TModel> UploadFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, int?>> expression)
            where TModel : class
        {
            string fieldName = (expression.Body as MemberExpression).Member.Name;// expression.Body.
            string value = expression.ToHtmlValue(htmlHelper);
            return new DropZoneBuilder<TModel>(htmlHelper, fieldName, value);
        }

        public static DropZoneBuilder<TModel> UploadFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, long>> expression)
            where TModel : class
        {
            string fieldName = (expression.Body as MemberExpression).Member.Name;// expression.Body.
            string value = expression.ToHtmlValue(htmlHelper);
            return new DropZoneBuilder<TModel>(htmlHelper, fieldName, value);
        }

        public static DropZoneBuilder<TModel> UploadFor<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, long?>> expression)
            where TModel : class
        {
            string fieldName = (expression.Body as MemberExpression).Member.Name;// expression.Body.
            string value = expression.ToHtmlValue(htmlHelper);
            return new DropZoneBuilder<TModel>(htmlHelper, fieldName, value);
        }

        public class DropZoneBuilder<TModel> : BuilderBase
            where TModel : class
        {
            private readonly string _fieldName;
            private readonly IHtmlHelper<TModel> _htmlHelper;
            private Dictionary<string, string> _htmlAttributes;
            private string _uploadUrl;
            private readonly string _value;

            public DropZoneBuilder(IHtmlHelper<TModel> htmlHelper, string fieldName, string value)
            {
                _htmlHelper = htmlHelper;
                _fieldName = fieldName;
                _value = value;
            }

            public DropZoneBuilder<TModel> HtmlAttributes(object htmlAttributes)
            {
                _htmlAttributes = ConvertToHtmlAttributes(htmlAttributes);
                return this;
            }

            public DropZoneBuilder<TModel> UploadUrl(string url)
            {
                _uploadUrl = url;
                return this;
            }


            public IHtmlContent Render()
            {
                string elementName = $"{ _fieldName }_uploader";
                TagBuilder builder = new TagBuilder("div");
                builder.Attributes.Add("id", elementName);

                if (_htmlAttributes != null)
                {
                    builder.MergeAttributes(_htmlAttributes);
                }

                var hiddenAttribute = _htmlHelper.Hidden(_fieldName, _value);

                StringBuilder scriptBuilder = new StringBuilder("<script>");
                scriptBuilder.Append("Dropzone.autoDiscover = false;");
                scriptBuilder.Append("$('div#" + elementName + "').dropzone({");
                scriptBuilder.Append($"url: '{_uploadUrl}',");

                scriptBuilder.Append($"maxFiles: {1},");
                scriptBuilder.Append("init: function () { $('div#" + elementName + "').data('uploader', this); this.on('maxfilesexceeded', function(file) { this.removeAllFiles(); this.addFile(file); }); this.on('removedfile', function() { $('#" + _fieldName + "').val(0); });  },");
                scriptBuilder.Append("addRemoveLinks: true,");
                scriptBuilder.Append("uploadMultiple: false,");
                scriptBuilder.Append("maxFilesize: 12,");
                scriptBuilder.Append("acceptedFiles: '.jpeg,.jpg,.png,.gif,.txt,.docx,.xlsx,.xls,.pptx,.xlsm,.pdf,.bmp,.csv',");
                scriptBuilder.Append("success: function (sender, response) { $('#" + _fieldName + "').val(response.id); }");
                scriptBuilder.Append("});");

                scriptBuilder.Append("</script>");

                return new HtmlString(GetString(hiddenAttribute) + GetString(builder) + scriptBuilder.ToString());
            }
        }
    }

}
