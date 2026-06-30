using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Customer;
using CQSAirborne.Model.Document;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly CustomerService _customerService;
        private readonly DocumentService _documentService;
        private readonly IConfiguration _configuration;
        public CustomerController(CustomerService customerService, DocumentService documentService, IConfiguration configuration)
        {
            _customerService = customerService;
            _documentService = documentService;
            _configuration = configuration;
        }

        [CheckAccess(ScreenCode.Customer, PermissionTypeConstant.List)]
        public IActionResult Index()
        {
            return View();
        }

        [CheckAccess(ScreenCode.Customer, PermissionTypeConstant.Add)]
        public async Task<IActionResult> CreateEdit(long? Id)
        {
            AddEditCustomerModel model = new AddEditCustomerModel();
            if (Id != null)
            {
                var response = await _customerService.GetByIdAsync(Id.Value);
                if (response.IsSuccess)
                {
                    response.Data.StartDate = DateTime.Now;
                    response.Data.EndDate = DateTime.Now.AddMonths(1).Date;
                    return View(response.Data);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEdit(AddEditCustomerModel model)
        {
            var response = await _customerService.InsertAsync(model);
            if (response.IsSuccess)
            {
                return Ok(response.Data);
            }
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> GetData(DataSourceRequest dataSourceRequest)
        {
            var response = await _customerService.GetAllAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> CreateEditCustomerDocument(CustomerDocumentMappingModel model)
        {
            var response = await _customerService.CreateEditCustomerDocument(model);
            if (response.IsSuccess)
            {
                return Ok(response.Data);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomerDocument(long pictureid, long documentid, long customerid)
        {
            var data = await _customerService.GetByIdAsync(customerid);
            var mapping = data.Data.CustomerDocumentDetail.Where(a => a.PictureId == pictureid && a.DocumentId == documentid).FirstOrDefault();
            long id = mapping.Id != null ? mapping.Id : 0;
            var response = await _customerService.DeleteCustomerDocument(id);
            if (response.IsSuccess)
            {
                return Ok(response.Data);
            }
            return BadRequest();
        }

        //[HttpPost]
        public async Task<IActionResult> DeleteCustomer(long customerid)
        {
            var response = await _customerService.DeleteCustomer(customerid);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest();
        }

        [HttpPost("CustomerExport")]
        public async Task<IActionResult> CustomerExport(DataSourceRequest dataSourceRequest)
        {
            try
            {

                AddEditCustomerModel model = new AddEditCustomerModel();
                var docDetails = await _customerService.GetAllAsync(dataSourceRequest);

                //if (ForExport == 2)
                {
                    string FileName = "CustomerConsolidatedList_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xlsx";

                    string relativePath = $"wwwroot/Exported/{FileName}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                    FileInfo file = new FileInfo(filePath);

                    using (ExcelPackage package = new ExcelPackage(file))
                    {

                        // (IEnumerable<List<DocumentListModel>>)docDetails.Data;
                        // var docList = ((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JProperty)docDetails.Data.ToString().FirstOrDefault()).Value).Value.ToString();
                        //docList = JsonConvert.DeserializeObject<DocumentListModel>(docDetails.Data.ToString()).docList;
                        var docList = JsonConvert.DeserializeObject<List<AddEditCustomerModel>>(docDetails.Data.Data.ToString());
                        // var docList = List<DocumentListModel>(docDetails.Data);
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("CustomerList");
                        int totalRows = docList.Count();

                        var Applist = docList.ToList();

                        worksheet.Cells[1, 1].Value = "Sl. No.";
                        worksheet.Cells[1, 1].Style.Font.Bold = true;
                        worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 2].Value = "Shared Date";
                        worksheet.Cells[1, 2].Style.Font.Bold = true;
                        worksheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 3].Value = "Customer Name";
                        worksheet.Cells[1, 3].Style.Font.Bold = true;
                        worksheet.Cells[1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 4].Value = "Customer Email Ids";
                        worksheet.Cells[1, 4].Style.Font.Bold = true;
                        worksheet.Cells[1, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);


                        worksheet.Cells[1, 5].Value = "Document Number";
                        worksheet.Cells[1, 5].Style.Font.Bold = true;
                        worksheet.Cells[1, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 6].Value = "Document Title";
                        worksheet.Cells[1, 6].Style.Font.Bold = true;
                        worksheet.Cells[1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 7].Value = "Valid From";
                        worksheet.Cells[1, 7].Style.Font.Bold = true;
                        worksheet.Cells[1, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 8].Value = "Valid To";
                        worksheet.Cells[1, 8].Style.Font.Bold = true;
                        worksheet.Cells[1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);

                        worksheet.Cells[1, 9].Value = "Document Access Link";
                        worksheet.Cells[1, 9].Style.Font.Bold = true;
                        worksheet.Cells[1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);


                        int i = 0;
                        int CellRow = 2;
                        for (int tr = 0; tr < totalRows; tr++)
                        {
                            try
                            {
                                worksheet.Cells[CellRow, 1].Value = 1;
                                worksheet.Cells[CellRow, 2].Value = Applist[i].CreatedOn.ToString("dd-MMM-yyyy");
                                worksheet.Cells[CellRow, 3].Value = Applist[i].CustomerName;
                                worksheet.Cells[CellRow, 4].Value = Applist[i].Email;
                                int nextColumnNo = 5;
                                if (Applist[tr].CustomerDocumentDetail != null)
                                {
                                    for (int t = 0; t < Applist[tr].CustomerDocumentDetail.Count; t++)
                                    {
                                        if (Applist[tr].CustomerDocumentDetail[t].IsActive == true)
                                        {
                                            worksheet.Cells[CellRow, 1].Value = CellRow - 1;
                                            worksheet.Cells[CellRow, 2].Value = Applist[i].CreatedOn.ToString("dd-MMM-yyyy");
                                            worksheet.Cells[CellRow, 3].Value = Applist[i].CustomerName;
                                            worksheet.Cells[CellRow, 4].Value = Applist[i].Email;

                                            for (int e = nextColumnNo; e <= 9; e++)
                                            {

                                                if (worksheet.Cells[1, e].Value.ToString().ToLower() == "document number")
                                                {
                                                    worksheet.Cells[CellRow, e].Value = Applist[tr].CustomerDocumentDetail[t].DocumentName;
                                                }
                                                if (worksheet.Cells[1, e].Value.ToString().ToLower() == "document title")
                                                {
                                                    worksheet.Cells[CellRow, e].Value = Applist[tr].CustomerDocumentDetail[t].AttachmentName;
                                                }
                                                if (worksheet.Cells[1, e].Value.ToString().ToLower() == "valid from")
                                                {
                                                    worksheet.Cells[CellRow, e].Value = Applist[tr].CustomerDocumentDetail[t].StartDate != null && Applist[tr].CustomerDocumentDetail[t].StartDate != DateTime.MinValue ? Applist[tr].CustomerDocumentDetail[t].StartDate.Value.ToString("dd-MMM-yyyy") : "";
                                                }
                                                if (worksheet.Cells[1, e].Value.ToString().ToLower() == "valid to")
                                                {
                                                    worksheet.Cells[CellRow, e].Value = Applist[tr].CustomerDocumentDetail[t].EndDate != null && Applist[tr].CustomerDocumentDetail[t].EndDate != DateTime.MinValue ? Applist[tr].CustomerDocumentDetail[t].EndDate.Value.ToString("dd-MMM-yyyy") : "";
                                                }
                                                if (worksheet.Cells[1, e].Value.ToString().ToLower() == "document access link")
                                                {
                                                    worksheet.Cells[CellRow, e].Value = Applist[tr].CustomerDocumentDetail[t].CreatedLink;
                                                }
                                            }
                                            if (t != Applist[tr].CustomerDocumentDetail.Count - 1)
                                            {
                                                CellRow++;
                                            }
                                        }
                                    }
                                }
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

    }
}