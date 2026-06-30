using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Utils
{
    public static class RazorToStringExtensions
    {
        public static async Task<string> RenderViewToStringAsync(
        this Controller controller, string viewName, object model)
        {
            // set the model
            controller.ViewData.Model = model;

            var sp = controller.HttpContext.RequestServices;
            var viewEngine = sp.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = sp.GetRequiredService<ITempDataProvider>();

            using (var sw = new StringWriter())
            {
                // Try absolute path first (GetView), then relative to the controller (FindView)
                var getView = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
                var viewResult = getView.Success
                    ? getView
                    : viewEngine.FindView(controller.ControllerContext, viewName, isMainPage: true);

                if (!viewResult.Success)
                {
                    var searched = string.Join(Environment.NewLine, viewResult.SearchedLocations ?? Enumerable.Empty<string>());
                    throw new FileNotFoundException(
                        $"View '{viewName}' not found.{Environment.NewLine}Searched:{Environment.NewLine}{searched}");
                }

                // clone ViewData to avoid side effects
                var viewData = new ViewDataDictionary(controller.ViewData)
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    controller.ControllerContext,
                    viewResult.View,
                    viewData,
                    new TempDataDictionary(controller.HttpContext, tempDataProvider),
                    sw,
                    new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext).ConfigureAwait(false);
                return sw.ToString();
            }
        }
    
    }
}
