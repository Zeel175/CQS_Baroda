using CQSAirborne.Model.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CQSAirborne.Services.API.Extensions
{
    public static class DataGridExtensions
    {
        public static DataSourceResult ToDataSourceResult<TSource>(this IQueryable<TSource> sources, DataSourceRequest request)
        {
            DataSourceResult result = ProcessQuery(ref sources, request);
            result.Data = sources.ToList();
            return result;
        }

        public static async Task<DataSourceResult> ToDataSourceResultAsync<TSource>(this IQueryable<TSource> sources, DataSourceRequest request)
        {
            DataSourceResult result = ProcessQuery(ref sources, request);
            result.Data = await sources.ToListAsync();
            return result;
        }



        private static DataSourceResult ProcessQuery<TSource>(ref IQueryable<TSource> sources, DataSourceRequest request)
        {
            DataSourceResult result = new DataSourceResult
            {
                Draw = request.Draw
            };
            

            // Count after filter and pagination is applied
            result.RecordsFiltered = sources.Count();

            // Filter logic goes here
            foreach (var column in request.Columns.Where(w => !string.IsNullOrEmpty(w.Search.Value)))
            {
                if (!string.IsNullOrEmpty(request.Operator) && request.Operator == "cn")
                {
                    //sources = sources.Where($"{column.Data}.Contains(@0)", column.Search.Value);
                    sources = sources.Where($"{column.Data} != null && {column.Data}.Contains(@0)", column.Search.Value);
                }
                else
                {
                    //sources = sources.Where($"{column.Data} == @0", column.Search.Value);
                    sources = sources.Where($"{column.Data} != null && {column.Data} == @0", column.Search.Value);
                }
            }

            // Global Filter Logic
            if (request.Search != null && !string.IsNullOrEmpty(request.Search.Value))
            {
                string whereClause = string.Empty;
                bool isFirstClause = true;
                foreach (var column in request.Columns.Where(w => w.Searchable))
                {
                    if (isFirstClause)
                    {
                        //whereClause += $"{column.Data}.ToString().ToLower().Contains(@0)";
                        whereClause += $"({column.Data} != null && {column.Data}.ToString().ToLower().Contains(@0))";
                        isFirstClause = false;
                    }
                    else
                    {
                        whereClause += $" or {column.Data}.ToString().ToLower().Contains(@0)";
                    }
                }
                if (!string.IsNullOrEmpty(whereClause))
                {
                    sources = sources.Where(whereClause, request.Search.Value.ToLower());
                }
            }

            // Count before pagination is applied
            result.RecordsTotal = sources.Count();

            // Sorting logic goes here
            if (request.Columns.Count > 0)
            {
                string orderColumnName = request.Columns[request.Order[0].Column].Data;
                string orderDirection = request.Order[0].Dir;
                if(orderColumnName == "roleName")
                {
                    orderColumnName = "id";
                }

                if (orderColumnName != null)
                {
                    if (orderDirection == "asc")
                    {
                        sources = sources.OrderBy(orderColumnName);
                    }
                    else
                    {
                        sources = sources.OrderBy(orderColumnName + " descending");
                    }
                }
            }

            // Apply pagination
            if (request.Length > 0)
            {
                sources = sources.Skip(request.Start).Take(request.Length);
            }

            return result;
        }
    }
}
