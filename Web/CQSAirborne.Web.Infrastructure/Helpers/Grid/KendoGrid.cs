using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CQSAirborne.Web.Infrastructure.Helpers.Grid
{
    public static class KendoGrid
    {
        public static DataGridBuilder<TModel> Grid<TModel>(this IHtmlHelper htmlHelper)
            where TModel : class
        {
            return new DataGridBuilder<TModel>();
        }
    }

    public class DataGridBuilder<TModel>
        where TModel : class
    {
        private List<string> columns { get; set; }

        public DataGridBuilder<TModel> AddColumn<TResult>(Expression<Func<TModel, TResult>> expression, object htmlAttribute = null)
        {
            return this;
        }

        public DataGridBuilder<TModel> AddCommand(string template)
        {
            return this;
        }

        public DataGridBuilder<TModel> EnableRefresh(bool isEnabled = false)
        {
            return this;
        }

        public DataGridBuilder<TModel> EnableSorting(bool isEnabled = false)
        {
            return this;
        }

        public IHtmlContent Render()
        {
            TagBuilder tagBuilder = new TagBuilder("li");
            return tagBuilder;
        }
    }
}
