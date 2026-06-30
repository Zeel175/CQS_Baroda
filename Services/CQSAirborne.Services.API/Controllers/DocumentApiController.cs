using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Services.Description;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/Document")]
    public class DocumentApiController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly IPlantService _plantService;
        private readonly IIdentityHelper _identityHelper;
        private readonly IEmployeeService _employeeService;

        public DocumentApiController(IDocumentService documentService
            , IIdentityHelper identityHelper, IPlantService plantService
            , IEmployeeService employeeService)
        {
            _documentService = documentService;
            _identityHelper = identityHelper;
            _plantService = plantService;
            _employeeService = employeeService;
        }

        [HttpPost("Get")]
        public IActionResult Get(DataSourceRequest dataSourceRequest)
        {
            var data = _documentService.GetAll().ToDataSourceResult(dataSourceRequest);
            if (data.RecordsTotal > 0)
            {
                var result = (List<DocumentListModel>)data.Data;
                foreach (var document in result)
                {
                    if (document.DocumentTypeCode == Constants.DocumentType.Global)
                    {
                        document.Plants = _documentService.AssignAllActivePlant(document.Id);
                    }
                }
                data.Data = result;
            }

            return Ok(data);
        }
        [HttpPost("GetAllDocumentViewScreen")]
        public async Task<IActionResult> GetAllDocumentViewScreen(DataSourceRequest dataSourceRequest)
        { 
            //Assigned Plant wise Get Documents
            int userId = User.Identity.GetUserId();
            var data = await _documentService.GetAllDocumentViewScreenAsync(userId);

            var data1 = data.AsQueryable().ToDataSourceResult(dataSourceRequest);

            return Ok(data1);
        }


        [HttpPost("[action]/{id}")]
        public IActionResult GetHistoryByDocumentId(int id, DataSourceRequest dataSourceRequest)
        {
            var historyQuery = _documentService.GetHistoryByDocumentId(id);
            if (historyQuery == null)
                return NotFound();
            var data = historyQuery.ToDataSourceResult(dataSourceRequest);
            if (data.RecordsTotal > 0)
            {
                var result = (List<DocumentListModel>)data.Data;
                foreach (var document in result)
                {
                    if (document.DocumentTypeCode == Constants.DocumentType.Global)
                    {
                        document.Plants = _documentService.AssignAllActivePlantForHistory(document.Id);
                    }
                }
                data.Data = result;
            }
            return Ok(data);
        }


        [HttpGet("CreateModel")]
        public async Task<IActionResult> CreateModel()
        {
            var data = await _documentService.GetCreateModelAsync();
            return Ok(data);
        }

        [HttpPost("[action]")]
        public IActionResult Post(AddEditDocumentModel addEditDocumentModel)
        {
            int userId = User.Identity.GetUserId();
            var isSaved = _documentService.Insert(addEditDocumentModel, userId);
            if (isSaved.Id == 0)
                return BadRequest(AddValidation("", "Unable to save data"));
            return Ok(isSaved);
        }

        [HttpGet("[action]/{id}")]
        public IActionResult Get(int id)
        {
            var data = _documentService.GetById(id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpGet("[action]/{id}")]
        public IActionResult GetDocumentHistoryById(int id)
        {
            var data = _documentService.GetDocumentHistoryById(id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllDocumentType()
        {
            var data = await _documentService.GetAllDocumentTypeAsync();
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPost("[action]")]
        public IActionResult Put(AddEditDocumentModel addEditDocumentModel)
        {
            bool isSaved = _documentService.Update(addEditDocumentModel, _identityHelper.UserId);
            if (!isSaved)
                return BadRequest(AddValidation("", "Unable to save data"));
            return Ok();
        }

        [HttpPost("[action]/{uploadType}")]
        public async Task<IActionResult> Upload(IFormFile file, UploadType uploadType)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string relativePath = $"uploads/{uploadType.ToString()}/{fileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            UploadResponse response = _documentService.SavePicture(new AddEditPictureModel
            {
                Name = fileName,
                Path = $"~/{relativePath}",
                Extension = Path.GetExtension(fileName),
                DisplayName = file.FileName
            }, _identityHelper.UserId);

            if (response != null)
                return Ok(response);

            return BadRequest();
        }

        [HttpGet("GetAllDocumentPlants")]
        public IActionResult GetAllDocumentPlants()
        {
            //Assigned Plant wise Get Documents
            int userId = User.Identity.GetUserId();
            var userresponse = _employeeService.GetById(userId);
            var plantList = userresponse?.PlantIds?.Split(",");
            if (plantList != null && plantList.Length > 0)
            {
                var data = _documentService.GetDocumentAllPlants().Where(a => plantList.Any(b => b.ToString() == a.Id.ToString()));
                return Ok(data);
            }
            else
            {
                var data = _documentService.GetDocumentAllPlants();
                return Ok(data);
            }
        }

        [HttpGet("[action]/{id}")]
        public IActionResult GetDocumentPlants(int id)
        {
            var data = _documentService.GetDocumentPlants(id);
            return Ok(data);
        }

        [HttpGet("[action]/{id}")]
        public IActionResult GetDocumentHistoryPlantsNew(int id)
        {
            bool isGlobal = false;
            var data = new List<PlantSelectListModel>();
            var newPl = new List<PlantDocumentListModel>();
            var historyQuery = _documentService.GetHistoryByDocumentId(id);
            if (historyQuery == null)
                return NotFound();
            if (historyQuery != null && historyQuery.Count() > 0)
            {
                var result = (List<DocumentListModel>)historyQuery.ToList();
                foreach (var document in result)
                {
                    
                    if (document.DocumentTypeCode == Constants.DocumentType.Global)
                    {
                        isGlobal = true;
                        newPl = _documentService.AssignAllActivePlantForHistory(document.Id);
                    }
                    else
                    {
                        newPl = document.Plants;
                    }
                }
            }
            if (newPl != null && newPl.Count() > 0)
            {
                foreach (var item in newPl)
                {
                    if(data.Where(a => a.Code == item.PlantAlias) == null || data.Where(a => a.Code == item.PlantAlias).Count() == 0)
                    {
                        PlantSelectListModel det = new PlantSelectListModel();
                        det.Id = item.PlantId;
                        det.Name = item.PlantName;
                        det.Code = item.PlantAlias;
                        det.DisplayOrder = item.DisplayOrder;
                        data.Add(det);
                    }
                }
            }
            return Ok(data);
        }

        [HttpPost("[action]/{id}/{status}/{isNotify}")]
        public IActionResult ChangeStatus(int id, bool status, bool isNotify = false)
        {
            bool isSaved = _documentService.ChangeStatus(id, status, isNotify);
            if (!isSaved)
                return BadRequest();
            return Ok();
        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult EmailDocument(SendEmailModel sem)
        {
            var isSent = _documentService.EmailDocument(sem);
            return Ok(isSent);
        }

        [HttpGet("GetAllPrefixDocNumber")]
        public async Task<IActionResult> GetAllPrefixDocNumber()
        {
            //var data = (from a in _documentService.GetAll().Where(m => m.DocTypePrefix != string.Empty).GroupBy(m => m.DocTypePrefix)
            //            select new
            //            {
            //                Name = a.First().DocTypePrefix
            //            }).ToList();

            var data1 = await Task.Run(() => _documentService.GetAllPrefixDocNumber());

            var data = (from a in data1
                        select new
                        {
                            Name = a.Name
                        }).ToList();
            return Ok(data);
        }

        [HttpPost("GetAllForExport")]
        public IActionResult GetAllForExport(DataSourceRequest dataSourceRequest)
        {
            if (dataSourceRequest.StatusId == 0)
            {
                dataSourceRequest.StatusId = 2;
            }
            else if (dataSourceRequest.StatusId == 2)
            {
                dataSourceRequest.StatusId = 0;
            }
            string prefix = string.Empty;
            if (dataSourceRequest.DocTypePrefix != null && dataSourceRequest.DocTypePrefix != string.Empty)
            {
                prefix = dataSourceRequest.DocTypePrefix;
            }

            var data = _documentService.GetAll().Where(m => (m.DocumentNumber.ToLower().Contains(prefix != string.Empty ? prefix.ToLower() : m.DocumentNumber.ToLower())) && (m.IsActive == (dataSourceRequest.StatusId != 2 ? Convert.ToBoolean(dataSourceRequest.StatusId) : m.IsActive)));
            var dataHistory = _documentService.GetAllDocumentHistory().Where(m => (m.DocumentNumber.ToLower().Contains(prefix != string.Empty ? prefix.ToLower() : m.DocumentNumber.ToLower())) && (m.IsActive == (dataSourceRequest.StatusId != 2 ? Convert.ToBoolean(dataSourceRequest.StatusId) : m.IsActive)));

            var plantData = _plantService.GetAll()
                .Where(w => w.IsActive);//DTA Plant
            DataSourceResult data1 = null;
            if (data.Count() > 0 || dataHistory.Count() > 0)
            {
                var result = data.ToList();
                foreach (var document in data)
                {
                    if (document.DocumentTypeCode == Constants.DocumentType.Global)
                    {
                        var plantData1 = plantData.Select(s => new PlantDocumentListModel
                        {
                            Id = document.Id,
                            CanDownload = document.CanDownload,
                            DocumentDisplayName = document.Alias,
                            PlantAlias = s.Alias,
                            PlantId = s.Id,
                            PlantName = s.Name,
                            DisplayOrder = s.DisplayOrder
                        }).ToList();
                        document.Plants = plantData1; //_documentService.AssignAllActivePlant(document.Id);
                    }
                }
                if (dataHistory != null && dataHistory.Count() > 0)
                {
                    result.AddRange(dataHistory);
                }

                data1 = result.AsQueryable().ToDataSourceResult(dataSourceRequest);
            }


            return Ok(data1);
        }


        [HttpPost("GetAllForExportWithSP")]
        public async Task<IActionResult> GetAllForExportWithSP(DataSourceRequest dataSourceRequest)
        {
            //Assigned Plant wise Get Documents
            var employeeId = User.Identity.GetUserId();
            var data = await _documentService.GetFilteredDocumentsForExportAsync(dataSourceRequest.StatusId, dataSourceRequest.DocTypePrefix, employeeId);

            var data1 = data.AsQueryable().ToDataSourceResult(dataSourceRequest);

            return Ok(data1);
        }

        [HttpGet("[action]/{id}")]
        public IActionResult Delete(int id)
        {
            _documentService.Delete(id);
            return Ok();
        }

        [HttpGet("GetAllDocumentList")]
        public async Task<IActionResult> GetAllDocumentList()
        {
            var data1 = await Task.Run(() => _documentService.GetAllDocumentList());

            var data = (from a in data1
                        select new
                        {
                            Id = a.Id,
                            Name = a.Name
                        }).ToList();
            return Ok(data);
        }

        [HttpPost("[action]")]
        public IActionResult InsertDocumentEmailData(DocumentEmailDataModel model)
        {
            bool isSaved = _documentService.InsertDocumentEmailData(model, _identityHelper.UserId);
            if (!isSaved)
                return BadRequest(AddValidation("", "Unable to save data"));
            return Ok();
        }

        [HttpPost("GetPendingEmailDocuments")]
        public IActionResult GetPendingEmailDocuments(DataSourceRequest dataSourceRequest)
        {
            var dataQuery = _documentService.GetPendingEmailDocuments();
            if (dataQuery == null)
                return NotFound();

            if (dataSourceRequest.RevisionDate != null)
            {
                dataQuery = dataQuery.Where(a => a.ModifiedOn.Value.Date == dataSourceRequest.RevisionDate.Date);
            }
            var data = dataQuery.ToDataSourceResult(dataSourceRequest);
            return Ok(data);
        }

        [HttpPost("GetPendingEmailDocumentsBySP")]
        public IActionResult GetPendingEmailDocumentsBySP(DataSourceRequest dataSourceRequest)
        {
            int userId = User.Identity.GetUserId();
            var dataQuery = _documentService.GetPendingEmailDocumentsBySP(userId, dataSourceRequest.RevisionDate);
            if (dataQuery == null)
                return NotFound();

            //if (dataSourceRequest.RevisionDate != null)
            //{
            //    dataQuery = dataQuery.Where(a => a.ModifiedOn.Value.Date == dataSourceRequest.RevisionDate.Date);
            //}
            var data = dataQuery.ToDataSourceResult(dataSourceRequest);
            return Ok(data);
        }


        [HttpPost("[action]/{ids}/{title?}")]
        public IActionResult SendPendingEmail(string ids, string title = "")
        {
            bool isSaved = _documentService.SendPendingEmail(ids, title, _identityHelper.UserId);
            if (!isSaved)
                return BadRequest();
            return Ok();
        }

        [HttpPost("preview-email")]
        public IActionResult PreviewEmail([FromBody] EmailPreviewRequest request)
        {
            var data = _documentService.GetEmailPreview(request.Ids, request.Title, _identityHelper.UserId);
            return Ok(data);
        }
    }
}