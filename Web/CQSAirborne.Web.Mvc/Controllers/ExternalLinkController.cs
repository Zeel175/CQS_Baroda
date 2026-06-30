using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.ExternalLink;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Mvc;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class ExternalLinkController : Controller
    {
        private readonly ExternalLinkService _externalLinkService;

        public ExternalLinkController(ExternalLinkService externalLinkService)
        {
            _externalLinkService = externalLinkService;
        }

        [CheckAccess(ScreenCode.ExternalLink, PermissionTypeConstant.List)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetExternalLinkData(DataSourceRequest dataSourceRequest)
        {
            var response = await _externalLinkService.GetAllAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        [CheckAccess(ScreenCode.ExternalLink, PermissionTypeConstant.Add)]
        public async Task<IActionResult> Create()
        {
            var model = await _externalLinkService.GetCreateModelAsync();
            return View(model.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddEditExternalLinkModel model)
        {
            var result = await _externalLinkService.InsertAsync(model);
            if (result.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet]
        [CheckAccess(ScreenCode.ExternalLink, PermissionTypeConstant.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _externalLinkService.GetEditModelAsync(id);
            if (model.IsSuccess)
                return View(model.Data);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddEditExternalLinkModel model)
        {
            var result = await _externalLinkService.UpdateAsync(model);
            if (result.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public async Task<IActionResult> _GetExternalLinksPartial()
        {
            var response = await _externalLinkService.GetAllLinksAsync();
            return PartialView(response.Data);
        }
    }
}