using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CQSAirborne.Web.Mvc.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Core;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Model.Category;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly DashboardService _dashboardService;
        public readonly IConfiguration _configuration;

        public HomeController(DashboardService dashboardService
            , IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _dashboardService = dashboardService;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int id = 0)
        {
            string webPath = _configuration.GetValue<string>("OpenDocPath");
            ViewBag.WebPath = webPath;
            var response = await _dashboardService.GetDashboardModelAsync(id);
            if (response.IsSuccess)
            {
                return View(response.Data);
            }

            DashboardModel model = new DashboardModel
            {
                CategoryId = id
            };
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> DashboardCategories(int id, int categoryId = 0)
        {

            var result = await _dashboardService.GetDashboardCategoryAsync(id, categoryId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            ModelState.AddModelError("", "Unable to get category data");
            return BadRequest(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> GetDocuments(int id, DataSourceRequest dataSourceRequest)
        {
            dataSourceRequest.Order.Add(new DataSourceRequest.DataOrderRequest() { Column = 0, Dir = "asc" });
            var result = await _dashboardService.GetDashboardDocumentsAsync(dataSourceRequest, id);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            ModelState.AddModelError("", "Unable to get document data");
            return BadRequest(ModelState);
        }

        public IActionResult GlobalSearch()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetQuickSearchData(QuickSearchRequestModel quickSearchRequestModel)
        {
            var response = await _dashboardService.GetQuickSearchDataAsync(quickSearchRequestModel);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> GetSubCategories(int id, DataSourceRequest dataSourceRequest)
        {
            var result = await _dashboardService.GetDashboardSubCategoriesAsync(dataSourceRequest, id);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            ModelState.AddModelError("", "Unable to get subcategory data");
            return BadRequest(ModelState);
        }

        [Route("Home/ViewDocument/{id}/{documentOperationType}")]
        public async Task<IActionResult> ViewDocument(int id, DocumentOperationType documentOperationType)
        {
            var response = await _dashboardService.GetViewDocumentModel(id, (int)documentOperationType);
            if (response.IsSuccess)
            {
                return View(response.Data);
            }
            return RedirectToAction("Index");
        }

        [Route("Home/GetPdfDocument/{id}/{documentOperationType}")]
        public async Task<IActionResult> GetPdfDocument(int id, DocumentOperationType documentOperationType)
        {
            if (id <= 0)
                return BadRequest();

            var response = await _dashboardService.GetPdfDocument(id, (int)documentOperationType);
            if (response.IsSuccess)
            {
                return File(response.File, response.ContentType);
            }
            return BadRequest();
        }

        [Route("Home/DownloadDocument/{id}/{documentOperationType}")]
        public async Task<IActionResult> DownloadDocument(int id, DocumentOperationType documentOperationType)
        {
            if (id <= 0)
                return BadRequest();

            var response = await _dashboardService.GetDocumentToDownload(id, (int)documentOperationType);
            if (response.IsSuccess)
            {
                return File(response.File, response.ContentType, response.FileName);
            }
            return BadRequest();
        }

        public IActionResult ImageUpload()
        {
            return View(new EditorUploadModel());
        }
        

        public async Task<IActionResult> TemporaryImages(int id)
        {
            if (id <= 0)
                return BadRequest();

            var response = await _dashboardService.TemporaryImages(id);
            if (response.IsSuccess)
            {
                return File(response.File, response.ContentType, response.FileName);
            }
            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost("cspreport")]
        public IActionResult CspReport([FromBody] CspReportRequest request)
        {
            // TODO: log request to a datastore somewhere
           // _logger.LogWarning($"CSP Violation: {request.CspReport.DocumentUri}, {request.CspReport.BlockedUri}");

            return Ok(request.CspReport);
        }
    }

    public class CspReportRequest
    {
        [JsonProperty(PropertyName = "csp-report")]
        public CspReport CspReport { get; set; }
    }

    public class CspReport
    {
        [JsonProperty(PropertyName = "document-uri")]
        public string DocumentUri { get; set; }

        [JsonProperty(PropertyName = "referrer")]
        public string Referrer { get; set; }

        [JsonProperty(PropertyName = "violated-directive")]
        public string ViolatedDirective { get; set; }

        [JsonProperty(PropertyName = "effective-directive")]
        public string EffectiveDirective { get; set; }

        [JsonProperty(PropertyName = "original-policy")]
        public string OriginalPolicy { get; set; }

        [JsonProperty(PropertyName = "blocked-uri")]
        public string BlockedUri { get; set; }

        [JsonProperty(PropertyName = "status-code")]
        public int StatusCode { get; set; }
    }
}
