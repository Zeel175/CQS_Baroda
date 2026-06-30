using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.Role;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OfficeOpenXml.Style;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class DocumentController : BaseController
    {
        private readonly DocumentService _documentService;
        private readonly ISessionManager _sessionManager;
        private readonly CPRMasterService _cprService;

        public DocumentController(DocumentService documentService
            , ISessionManager sessionManager, CPRMasterService cprService)
        {
            _documentService = documentService;
            _sessionManager = sessionManager;
            _cprService = cprService;
        }

        [CheckAccess(ScreenCode.Document, PermissionTypeConstant.List)]
        public async Task<IActionResult> Index()
        {
            DocumentListDataModel model = new DocumentListDataModel();
            var response = await _documentService.GetAllDocumentPlants();
            if (response.IsSuccess)
            {
                model.Plants = response.Data;
            }

            var permissionModel = JsonConvert.DeserializeObject<List<AssignPermissionViewModel>>(
              User.Claims.FirstOrDefault(x => x.Type.Equals("Permission", StringComparison.OrdinalIgnoreCase))?.Value ?? "[]"
          );
            bool isAdmin = ((ClaimsIdentity)User.Identity).GetSpecificClaim("OrgRole") != null && ((ClaimsIdentity)User.Identity).GetSpecificClaim("OrgRole") == "Admin" ? true : false;

            bool IsDelete = true;
            if (!isAdmin)
            {
                IsDelete = permissionModel.Any(m => m.Code == ScreenCode.CPRMaster && m.IsDelete == true);
            }
            ViewBag.IsDelete = IsDelete ? "" : "none";

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetDocumentData(DataSourceRequest dataSourceRequest)
        {
            var response = await _documentService.GetAllAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> GetDocumentDataNew(DataSourceRequest dataSourceRequest)
        {
            var response = await _documentService.GetAllDocumentViewScreen(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [CheckAccess(ScreenCode.Document, PermissionTypeConstant.Add)]
        public async Task<IActionResult> Create()
        {
            var response = await _documentService.GetCreateModelAsync();
            if (response.IsSuccess)
            {
                _sessionManager.Set(response.Data.SessionId.ToString(), response.Data.Uploads);
                response.Data.InActiveDate = DateTime.Now;

                var nonStandardCategories = await _cprService.GetAllNonStandardCategories();
                ViewBag.NonStandardCategories = nonStandardCategories.Data;

                var CPRList = await _cprService.GetCPRListBySP(); // to load cache
                ViewBag.CPRList = CPRList.Data;

                return View(response.Data);
            }
            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddEditDocumentModel addEditDocumentModel)
        {

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            addEditDocumentModel.Uploads = _sessionManager.Get<AddEditDocumentPictureModel>(addEditDocumentModel.SessionId.ToString());

            var response = await _documentService.InsertAsync(addEditDocumentModel);
            if (response.IsSuccess)
            {
                return Ok(response.Data);
            }
            return Ok(response);
        }

        [CheckAccess(ScreenCode.Document, PermissionTypeConstant.Edit)]
        public async Task<IActionResult> Edit(int id, string historyId = "")
        {
            var CPRList = await _cprService.GetCPRListBySP(); // to load cache
            ViewBag.CPRList = CPRList.Data;
            if (historyId == "")
            {
                var response = await _documentService.GetEditModelAsync(id);
                if (response.IsSuccess)
                {
                   
                    response.Data.InActiveDate = DateTime.Now;
                    _sessionManager.Set(response.Data.SessionId.ToString(), response.Data.Uploads);
                    return View(response.Data);
                }
                return RedirectToAction("Index");
            }
            else
            {
                int id1 = historyId != null ? Convert.ToInt32(historyId) : 0;
                var response = await _documentService.GetEditHistoryModelAsync(id1);
                if (response.IsSuccess)
                {
                    response.Data.InActiveDate = response.Data.InActiveDate != null && response.Data.InActiveDate != DateTime.MinValue ? response.Data.InActiveDate : DateTime.Now;
                    response.Data.OldRevId = id1;
                    response.Data.Id = id;
                    _sessionManager.Set(response.Data.SessionId.ToString(), response.Data.Uploads);
                    return View(response.Data);
                }
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddEditDocumentModel addEditDocumentModel)
        {

            if (!ModelState.IsValid)
                return View(addEditDocumentModel);

            addEditDocumentModel.Uploads = _sessionManager.Get<AddEditDocumentPictureModel>(addEditDocumentModel.SessionId.ToString());
            var response = await _documentService.UpdateAsync(addEditDocumentModel);
            if (response.IsSuccess)
            {
                return Ok(addEditDocumentModel);
            }
            else
            {
                return BadRequest(response.Errors);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, UploadType uploadType)
        {
            bool HasSpecialCharacter = false;
            string fileExtesion = Path.GetExtension(file.FileName);
            if(fileExtesion == ".csv")
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                        MemoryStream stream = new MemoryStream(fileBytes);

                        // convert stream to string
                        StreamReader reader = new StreamReader(stream);
                        String line = reader.ReadLine();
                        if (line != null)
                        {
                            do
                            {
                                line = reader.ReadLine();
                                if (line != null)
                                {
                                    string[] cellData = line.ToString().Split(',');
                                    if (cellData != null && cellData.Count() > 0)
                                    {
                                        //foreach (var item in cellData)
                                        //{
                                        //    //string item = JsonConvert.DeserializeObject<string>(item1);
                                        //    if (item != "" && (item.Substring(0, 1).Contains('=') || item.Substring(0, 1).Contains('+') || item.Substring(0, 1).Contains('-') || item.Substring(0, 1).Contains('/')))
                                        //    {
                                        //        HasSpecialCharacter = true;
                                        //        break;
                                        //    }
                                        //}

                                        //Above code commented for Airborne Plant
                                    }
                                }
                            } while (line != null);
                        }
                        //string csvText = reader.ReadToEnd();
                        if(HasSpecialCharacter)
                        {
                            return BadRequest();
                        }
                    }
                }
                catch(Exception e)
                {

                }
            }


            if (fileExtesion == ".xlsx" || fileExtesion == ".xls")
            {
                try
                {
                    string csvText = string.Empty;
                    bool hasHeader = true;
                    using (var excelPack = new ExcelPackage())
                    {
                        //Load excel stream
                        //using (var stream = System.IO.File.OpenRead(path))
                        //{
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            excelPack.Load(ms);
                        }

                        //Below code commented for Airborne Plant

                        ////Lets Deal with first worksheet.(You may iterate here if dealing with multiple sheets)
                        //var ws = excelPack.Workbook.Worksheets["Sheet1"];

                        ////Get all details as DataTable -because Datatable make life easy :)
                        //DataTable excelasTable = new DataTable();
                        //foreach (var firstRowCell in ws.Cells[1, 1, 2, ws.Dimension.End.Column])
                        //{
                        //    //Get colummn details
                        //    if (!string.IsNullOrEmpty(firstRowCell.Text))
                        //    {
                        //        string firstColumn = string.Format("Column {0}", firstRowCell.Start.Column);
                        //        excelasTable.Columns.Add(hasHeader ? firstRowCell.Text : firstColumn);
                        //    }
                        //}
                        //var startRow = hasHeader ? 2 : 1;
                        ////Get row details
                        //for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                        //{
                        //    var wsRow = ws.Cells[rowNum, 1, rowNum, excelasTable.Columns.Count];
                        //    DataRow row = excelasTable.Rows.Add();
                        //    foreach (var cell in wsRow)
                        //    {
                        //        row[cell.Start.Column - 1] = cell.Text;
                        //        csvText = csvText + " " + cell.Text;

                        //        //Formula Clause is commented
                        //        //if(cell.Text != "" && (cell.Formula != "" || cell.Text.Substring(0, 1).Contains('=') || cell.Text.Substring(0, 1).Contains('+') || cell.Text.Substring(0, 1).Contains('-') || cell.Text.Substring(0, 1).Contains('/')))
                        //        //{
                        //        //    HasSpecialCharacter = true;
                        //        //    break;
                        //        //}
                        //        //End
                        //    }
                        //}

                        //Above code commented for Airborne Plant

                        if (HasSpecialCharacter)
                        {
                            return BadRequest();
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }

            var response = await _documentService.UploadAsync(file, uploadType);
            if (response.IsSuccess)
            {
                return Ok(response.Data);
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocumentType()
        {
            var response = await _documentService.GetAllDocumentType();
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActiveStatus()
        {
            List<SelectListModel> list = new List<SelectListModel>();
            list.Add(new SelectListModel { Id = 0, Name = "All" });
            list.Add(new SelectListModel { Id = 1, Name = "Active" });
            list.Add(new SelectListModel { Id = 2, Name = "InActive" });
            //{ "nodedates":[{"date":thisDate}]};//[{"Id":2,"Name":"All"},{"Id":1,"Name":"Active"},{"Id":0,"Name":"InActive"}];

            //var response = await _documentService.GetAllDocumentType();
            //if (response.IsSuccess)
            //{
            return await Task.Run(() => Json(list));
            //}
            //return Json(new { });
        }

        [HttpPost]
        public IActionResult UploadDocumentToSession(string sessionId, AddEditDocumentPictureModel addEditDocumentPictureModel)
        {
            _sessionManager.AddToCollection(sessionId, addEditDocumentPictureModel);
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteDocumentFromSession(string sessionId)
        {
            _sessionManager.Set(sessionId, new List<AddEditDocumentPictureModel>());
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteSingleDocumentFromSession(long pictureId, string sessionId)
        {
            _sessionManager.Set(sessionId, _sessionManager.Get<AddEditDocumentPictureModel>(sessionId).Where(w => w.PictureId != pictureId).ToList());
            return Ok();
        }


        [HttpPost]
        public IActionResult GetUploadedDocuments(string sessionId, DataSourceRequest dataSourceRequest)
        {
            List<UploadedDocumentListModel> data = GetSessionDocuments(sessionId);
            return Json(data.ToDataSourceResult(dataSourceRequest));
        }

        [HttpPost]
        public async Task<IActionResult> GetDocumentHistoryData(int id, DataSourceRequest dataSourceRequest)
        {
            var response = await _documentService.GetDocumentHistoryAll(id, dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, bool status)
        {
            var response = await _documentService.ChangeStatus(id, status);
            if (response.IsSuccess)
            {
                return Json(new { Status = true });
            }
            return Json(new { Status = false });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatusNew(int id, bool status, bool isNotify)
        {
            var response = await _documentService.ChangeStatus(id, status, isNotify);
            if (response.IsSuccess)
            {
                return Json(new { Status = true });
            }
            return Json(new { Status = false });
        }


        public async Task<PartialViewResult> EmailDocument(int? id, bool? isSendLater)
        {
            bool isLater = isSendLater != null ? isSendLater.Value : false;
            return PartialView("~/Views/Document/_EmailDocumentPartialView.cshtml", new SendEmailModel() { id = id.Value, IsSendLater = isLater });
        }
        [HttpPost]
        public async Task<IActionResult> EmailDocument(SendEmailModel sem)
        {
            if (sem.IsSendLater == null || sem.IsSendLater == false)
            {
                var response = await _documentService.EmailDocument(sem);
                if (response.IsSuccess)
                {
                    return Json(new { Status = true });
                }
                return Json(new { Status = false });
            }
            else
            {
                DocumentEmailDataModel model = new DocumentEmailDataModel();
                model.DocumentId = sem.id;
                model.DocumentType = sem.DocumentType;
                model.SpecificPersonEmail = sem.SpecificPersonEmail;
                var response = await _documentService.InsertDocumentEmailDataAsync(model);
                if (response.IsSuccess)
                {
                    return Json(new { Status = true });
                }
                return Json(new { Status = false });
            }
        }
       
        private List<UploadedDocumentListModel> GetSessionDocuments(string sessionId)
        {
            return _sessionManager.Get<AddEditDocumentPictureModel>(sessionId).Select(s => new UploadedDocumentListModel
            {
                PictureId = s.PictureId,
                Plants = string.Join(',', s.Plants.Select(w => w.Name)),
                DisplayName = s.DisplayName
            }).ToList();
        }

        [CheckAccess(ScreenCode.MasterPCFTracker, PermissionTypeConstant.List)]
        public async Task<IActionResult> ViewDocuments()
        {
            DocumentListDataModel model = new DocumentListDataModel();
            model.RevisionDate = DateTime.Now;

            var response = await _documentService.GetAllDocumentPlants();
            if (response.IsSuccess)
            {
                model.Plants = response.Data;
            }
            return View(model);
        }

        [HttpPost("ViewDocumentsForExport")]
        public async Task<IActionResult> ViewDocumentsForExport(DataSourceRequest dataSourceRequest)
        {
            try
            {

                DocumentListDataModel model = new DocumentListDataModel();
                //model.FromDate = DateTime.Now.AddMonths(-1);
                //model.ToDate = DateTime.Now;
                //var docDetails = await _documentService.GetAllForExport(dataSourceRequest);
                var docDetails = await _documentService.GetAllForExportWithSP(dataSourceRequest);
                var response = await _documentService.GetAllDocumentPlants();
                if (response.IsSuccess)
                {
                    model.Plants = response.Data;
                }


                //if (ForExport == 2)
                {
                    string FileName = "DocumentList_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xlsx";

                    string relativePath = $"wwwroot/Exported/{FileName}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                    FileInfo file = new FileInfo(filePath);

                    using (ExcelPackage package = new ExcelPackage(file))
                    {

                        // (IEnumerable<List<DocumentListModel>>)docDetails.Data;
                        // var docList = ((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JProperty)docDetails.Data.ToString().FirstOrDefault()).Value).Value.ToString();
                        //docList = JsonConvert.DeserializeObject<DocumentListModel>(docDetails.Data.ToString()).docList;
                        var docList = JsonConvert.DeserializeObject<List<DocumentListModel>>(docDetails.Data.Data.ToString());
                        // var docList = List<DocumentListModel>(docDetails.Data);
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("DocumentList");
                        int totalRows = docList.Count();

                        var Applist = docList.ToList();

                        worksheet.Cells[1, 1].Value = "Status";
                        worksheet.Cells[1, 1].Style.Font.Bold = true;
                        worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);
                        worksheet.Cells[1, 2].Value = "Doc. Type";
                        worksheet.Cells[1, 2].Style.Font.Bold = true;
                        worksheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);
                        worksheet.Cells[1, 3].Value = "Doc. Unique No.";
                        worksheet.Cells[1, 3].Style.Font.Bold = true;
                        worksheet.Cells[1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);
                        worksheet.Cells[1, 4].Value = "Document Name";
                        worksheet.Cells[1, 4].Style.Font.Bold = true;
                        worksheet.Cells[1, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 5].Value = "Latest Revision No.";
                        worksheet.Cells[1, 5].Style.Font.Bold = true;
                        worksheet.Cells[1, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 6].Value = "Revision Date";
                        worksheet.Cells[1, 6].Style.Font.Bold = true;
                        worksheet.Cells[1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 7].Value = "Inactive Date";
                        worksheet.Cells[1, 7].Style.Font.Bold = true;
                        worksheet.Cells[1, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 8].Value = "CPR Reference Number";
                        worksheet.Cells[1, 8].Style.Font.Bold = true;
                        worksheet.Cells[1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 9].Value = "Reason for Change";
                        worksheet.Cells[1, 9].Style.Font.Bold = true;
                        worksheet.Cells[1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);


                        int nextColumnNo = 10;
                        if (response.Data.Count > 0)
                        {
                            for (int g = 0; g < response.Data.Count; g++)
                            {
                                worksheet.Cells[1, nextColumnNo].Value = response.Data[g].Name;
                                worksheet.Cells[1, nextColumnNo].Style.Font.Bold = true;
                                worksheet.Cells[1, nextColumnNo].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[1, nextColumnNo].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                                nextColumnNo++;
                            }
                        }

                        worksheet.Cells[1, nextColumnNo].Value = "Process Owner";
                        worksheet.Cells[1, nextColumnNo].Style.Font.Bold = true;
                        worksheet.Cells[1, nextColumnNo].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, nextColumnNo].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);
                        
                        worksheet.Cells[1, nextColumnNo + 1].Value = "Remarks";
                        worksheet.Cells[1, nextColumnNo + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, nextColumnNo + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, nextColumnNo + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        int i = 0;
                        int CellRow = 2;
                        for (int tr = 0; tr < totalRows; tr++)
                        {
                            try
                            {
                                worksheet.Cells[CellRow, 1].Value = Applist[i].IsActive == true ? "Active" : "InActive";
                                worksheet.Cells[CellRow, 2].Value = Applist[i].DocTypePrefix;
                                worksheet.Cells[CellRow, 3].Value = Applist[i].DocumentNumber;
                                worksheet.Cells[CellRow, 4].Value = Applist[i].Name;
                                worksheet.Cells[CellRow, 5].Value = Applist[i].RevisionNumber;
                                worksheet.Cells[CellRow, 6].Value = Applist[i].RevisionDate;
                                worksheet.Cells[CellRow, 7].Value = Applist[i].InActiveDate;
                                worksheet.Cells[CellRow, 8].Value = Applist[i].ReasonForChange;
                                worksheet.Cells[CellRow, 9].Value = Applist[i].CPRNumber;

                                if (Applist[tr].Plants != null && Applist[tr].Plants.Count > 0)
                                {
                                    for (int t = 0; t < Applist[tr].Plants.Count; t++)
                                    {
                                        for (int e = 10; e < nextColumnNo; e++)
                                        {
                                            if (Applist[i].DocumentTypeName.ToLower() != ("Common").ToLower())
                                            {
                                                if (worksheet.Cells[1, e].Value.ToString().ToLower() == Applist[tr].Plants[t].PlantName.ToLower())
                                                {
                                                    worksheet.Cells[CellRow, e].Value = Applist[i].Alias != null ? Applist[i].Alias : "Document";
                                                }
                                            }
                                            else
                                            {
                                                worksheet.Cells[CellRow, e].Value = Applist[i].Alias != null ? Applist[i].Alias : "Document";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    for (int e = 10; e < nextColumnNo; e++)
                                    {
                                        //if (Applist[i].DocumentTypeName.ToLower() != ("Common").ToLower())
                                        //{
                                        //    if (worksheet.Cells[1, e].Value.ToString().ToLower() == Applist[tr].Plants[t].PlantName.ToLower())
                                        //    {
                                        //        worksheet.Cells[CellRow, e].Value = Applist[i].Alias != null ? Applist[i].Alias : "Document";
                                        //    }
                                        //}
                                        //else
                                        //{
                                            worksheet.Cells[CellRow, e].Value = Applist[i].Alias != null ? Applist[i].Alias : "Document";
                                        //}
                                    }
                                }

                                worksheet.Cells[CellRow, nextColumnNo].Value = Applist[i].ProcessOwner;
                                worksheet.Cells[CellRow, nextColumnNo+1].Value = Applist[i].Remarks;

                                CellRow++;

                                i++;
                            }
                            catch (Exception e)
                            {
                            }
                        }

                        package.Save();

                        return Ok(new FileResponseModel { FilePath = "../ExPorted/" + FileName, FileStatus = true });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> GetDocumentMasterTrackerData(DataSourceRequest dataSourceRequest)
        {
            var response = await _documentService.GetAllForExport(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }
        [HttpPost]
        public async Task<IActionResult> GetAllForExportWithSP(DataSourceRequest dataSourceRequest)
        {
            var response = await _documentService.GetAllForExportWithSP(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPrefixDocNumber()
        {
            var response = await _documentService.GetAllPrefixDocNumber();
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _documentService.DeleteAsync(id);
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocumentList()
        {
            var response = await _documentService.GetAllDocumentList();
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAttachmentListByDocumentId(int Id)
        {
            var response = await _documentService.GetEditModelAsync(Id);
            List<SelectListModel> model = new List<SelectListModel>();
            foreach (var item in response.Data.Uploads.OrderByDescending(m => m.PictureId))
            {
                SelectListModel det = new SelectListModel();
                det.Id = Convert.ToInt32(item.PictureId);
                det.Name = item.DisplayName;
                model.Add(det);
            }
            if (model.Count() > 0)
            {
                return Json(model.ToList());
            }
            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> GetPendingEmailDocuments(DataSourceRequest dataSourceRequest)
        {
            var response = await _documentService.GetPendingEmailDocuments(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> GetPendingEmailDocumentsBySP(DataSourceRequest dataSourceRequest)
        {
            var response = await _documentService.GetPendingEmailDocumentsBySP(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }


        [CheckAccess(ScreenCode.EmailDocuments, PermissionTypeConstant.List)]
        public IActionResult PendingEmail()
        {
            DataSourceRequest model = new DataSourceRequest();
            model.RevisionDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendPendingEmail(string ids, string title)
        {
            var response = await _documentService.SendPendingEmail(ids,title);
            if (response.IsSuccess)
            {
                return Json(new { Status = true });
            }
            return Json(new { Status = false });
        }

        [HttpPost]
        public async Task<IActionResult> PreviewEmail(EmailPreviewRequest request)
        {
            var response = await _documentService.PreviewEmail(request);

            if (response == null || !response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response.Data);
        }
    }
}