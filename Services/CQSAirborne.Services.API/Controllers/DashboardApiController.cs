using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/Dashboard")]
    public class DashboardApiController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly Microsoft.Extensions.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;
        private readonly IEmployeeService _employeeService;

        public DashboardApiController(IDashboardService dashboardService
            , Microsoft.Extensions.Hosting.IHostingEnvironment hostingEnvironment
            , IConfiguration configuration
            , IDocumentService documentService, ICategoryService categoryService, IEmployeeService employeeService)
        {
            _dashboardService = dashboardService;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _documentService = documentService;
            _categoryService = categoryService;
            _employeeService = employeeService;
        }

        [HttpGet("[action]/{id}/{parentCategoryId}")]
        public IActionResult GetDashboardCategory(int id, int parentCategoryId)
        {
            var data = _dashboardService.GetDashboardCategory(id, parentCategoryId);
            return Ok(data);
        }

        //[HttpPost("[action]/{id}")]
        //public IActionResult GetDocuments(int id, DataSourceRequest dataSourceRequest)
        //{

        //    var isRestrected = _categoryService.CategoryIsRestricted(id);

        //    var data = _dashboardService.GetDocuments(id)
        //        .ToDataSourceResult(dataSourceRequest);
        //    if (data.RecordsTotal > 0)
        //    {
        //        var result = (List<DocumentListModel>)data.Data;
        //        foreach (var document in result)
        //        {
        //            if (document.DocumentTypeCode == Constants.DocumentType.Global)
        //            {
        //                document.Plants = _documentService.AssignAllActivePlant(document.Id, isRestrected);
        //            }
        //        }
        //        data.Data = result.OrderBy(m=>m.DocumentNumber);
        //    }
        //    return Ok(data);
        //}

        [HttpPost("[action]/{id}")]
        public IActionResult GetDocuments(int id, DataSourceRequest dataSourceRequest)
        {
            //Assigned Plant wise Get Documents
            int userId = User.Identity.GetUserId();
            var userresponse = _employeeService.GetById(userId);
            var isRestrected = _categoryService.CategoryIsRestricted(id);

            var plantList = userresponse?.PlantIds?.Split(",");

            // Get documents
            var documentsQuery = _dashboardService.GetDocuments(id);

            // ✅ Apply plant filter only if plantList is not null and has items
            if (plantList != null && plantList.Length > 0)
            {
                var plantIdsInt = plantList.Select(int.Parse).ToList();
                documentsQuery = documentsQuery
                    .Where(doc => (doc.Plants.Any(b => plantIdsInt.Contains(b.PlantId))) || (doc.DocumentTypeCode == Constants.DocumentType.Global));
            }

            var data = documentsQuery.ToDataSourceResult(dataSourceRequest);

            if (data.RecordsTotal > 0)
            {
                var result = (List<DocumentListModel>)data.Data;
                foreach (var document in result)
                {
                    if (document.DocumentTypeCode == Constants.DocumentType.Global)
                    {
                        if (plantList != null && plantList.Length > 0)
                        {
                            document.Plants = _documentService.AssignAllActivePlant(document.Id, isRestrected).Where(a => plantList.Any(b => b.ToString() == a.PlantId.ToString())).ToList();
                        }
                        else
                        {
                            document.Plants = _documentService.AssignAllActivePlant(document.Id, isRestrected);
                        }
                    }
                }
                data.Data = result.OrderBy(m => m.DocumentNumber);
            }

            return Ok(data);
        }


        [HttpPost("[action]")]
        public IActionResult GetQuickSearchData(QuickSearchRequestModel dataSourceRequest)
        {
            int userId = User.Identity.GetUserId();
            DataSourceResult result = _dashboardService.GetQuickSearchData(dataSourceRequest, userId);
            return Ok(result);
        }

        [HttpPost("[action]/{id}")]
        public IActionResult GetSubCategories(int id, DataSourceRequest dataSourceRequest)
        {
            var data = _dashboardService.GetSubCategories(id)
                .ToDataSourceResult(dataSourceRequest);
            return Ok(data);
        }

        [HttpGet("[action]/{id}/{documentOperationType}")]
        public async Task<IActionResult> GetPdfDocument(int id, DocumentOperationType documentOperationType)
        {
            var data = _dashboardService.GetDocumentById(id, documentOperationType);
            if (data == null)
                return BadRequest(AddValidation("", "Document not found"));

            return File(await GeneratePdfFromDocumentAsync(data), "application/pdf");
        }

        [HttpGet("[action]/{id}/{documentOperationType}")]
        public async Task<IActionResult> GetDocumentToDownload(int id, DocumentOperationType documentOperationType)
        {
            var data = _dashboardService.GetDocumentById(id, documentOperationType);
            if (data == null || !data.CanDownload)
                return BadRequest(AddValidation("", "Document not found"));

            string basePath = _hostingEnvironment.ContentRootPath;
            string filePath = data.Path.Substring(2, data.Path.Length - 2); //data.Path.Replace("~/", string.Empty);
            string path = Path.Combine(basePath, filePath);
            var memory = new MemoryStream();
            //if (System.IO.File.Exists(path))
            //{
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                return File(memory.ToArray(), GetMimeTypeForDownload(data.Extension), data.DisplayFileName.Replace(" ", string.Empty));
            //}
            //return BadRequest(AddValidation("", "Could not find file")); ;
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> TemporaryImages(long id)
        {
            var data = _dashboardService.GetPictureDetails(id);
            if (data == null)
                return BadRequest(AddValidation("", "Image not found"));

            string basePath = _hostingEnvironment.ContentRootPath;
            string filePath = data.Path.Replace("~/", string.Empty);
            string path = Path.Combine(basePath, filePath);
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            return File(memory.ToArray(), GetMimeTypeForDownload(data.Extension), data.DisplayFileName);
        }



        [HttpGet("[action]/{id}")]
        public IActionResult GetDashboardModel(int id)
        {
            var result = _dashboardService.GetCreateModel(id);
            return Ok(result);
        }

        [HttpGet("[action]/{id}/{documentOperationType}")]
        public IActionResult GetViewDocumentModel(int id, DocumentOperationType documentOperationType)
        {
            var result = _dashboardService.GetViewDocumentModel(id, documentOperationType);
            return Ok(result);
        }


        private string GetMimeTypeForDownload(string extension)
        {
            if (!string.IsNullOrEmpty(extension))
                extension = extension.ToLower().Trim();
            switch (extension)
            {
                case ".xls":
                case ".xlsx":
                case ".xlm":
                case ".xlsm":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".doc":
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".ppt":
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".pdf":
                    return "application/pdf";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".bmp":
                    return "image/bmp";
                default:
                    throw new NotImplementedException("Extension not registered");
            }
        }

        private async Task<byte[]> GeneratePdfFromDocumentAsync(DocumentDetailModel data)
        {
            string basePath = _hostingEnvironment.ContentRootPath;
            string filePath = data.Path.Replace("~/", string.Empty);
            string path = Path.Combine(basePath, filePath);

            if (data.Extension == ".pdf")
            {
                return await GetPdfFileStream(data, path);
            }

            return await GetPdfFileFromServiceAsync(data, path);
        }

        private async Task<byte[]> GetPdfFileFromServiceAsync(DocumentDetailModel data, string path)
        {
            string baseUrl = _configuration.GetValue<string>("PdfProviderServiceUrl");
            var _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            var fileData = System.IO.File.ReadAllBytes(path);
            ByteArrayContent bytes = new ByteArrayContent(fileData);
            new FileExtensionContentTypeProvider().TryGetContentType(path, out string mimeType);
            bytes.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);

            MultipartFormDataContent multiContent = new MultipartFormDataContent
            {
                { bytes, "file", System.IO.Path.GetFileName(path) }
            };

            var response = await _httpClient.PostAsync(baseUrl + "PdfConverter/Convert", multiContent);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }

        private async Task<byte[]> GetPdfFileStream(DocumentDetailModel model, string path)
        {
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            return memory.ToArray();
        }
    }
}