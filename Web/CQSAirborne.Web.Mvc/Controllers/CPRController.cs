using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CQSAirborne.Model.Core;
using CQSAirborne.Model.CPRMaster;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.Role;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class CPRController : BaseController
    {
        private readonly CPRMasterService _cprService;
        private readonly IConfiguration _configuration;
        private readonly ISessionManager _sessionManager;
        private readonly EmployeeService _employeeService;
        private readonly PlantService _plantService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConverter _converter;
        public CPRController(CPRMasterService cprService, IConfiguration configuration,
            ISessionManager sessionManager, EmployeeService employeeService
            , IHostingEnvironment hostingEnvironment, IConverter converter, PlantService plantService)
        {
            _cprService = cprService;
            _configuration = configuration;
            _sessionManager = sessionManager;
            _employeeService = employeeService;
            _hostingEnvironment = hostingEnvironment;
            _converter = converter;
            _plantService = plantService;
        }


        // List page
        [CheckAccess(ScreenCode.CPRMaster, PermissionTypeConstant.List)]
        public IActionResult Index()
        {
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
            return View();
        }

        // Create/Edit page (GET)
        [CheckAccess(ScreenCode.CPRMaster, PermissionTypeConstant.Add)]
        public async Task<IActionResult> CreateEdit(long? id)
        {
            var model = new CPRMasterModel();
            // CPRController.CreateEdit(long? id)
            if (model.SessionId == null)
                model.SessionId = Guid.NewGuid().ToString("N");

            // put your own claim type here; commonly NameIdentifier or a custom "UserId"
            var userIdStr = User.Claims.FirstOrDefault(c =>
                                c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                             || c.Type.Equals(System.Security.Claims.ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value;
            var currentUserId = string.IsNullOrWhiteSpace(userIdStr) ? 0 : int.Parse(userIdStr);
            ViewBag.CurrentUserId = currentUserId;
            var stageList = await _cprService.GetCPRMasterStage();
            ViewBag.StageList = stageList.Data;
            var statusList = await _cprService.GetCPRMasterStatus();

            //model.EDC = DateTime.Now;   // ensure not null
            //model.ADC = DateTime.Now;   // ensure not null

            var plantList = await _plantService.GetPlantList();
            ViewBag.PlantList = plantList.Data;


            if (id.HasValue && id.Value > 0)
            {
                var response = await _cprService.GetByIdAsync(id.Value);
                if (response.IsSuccess && response.Data != null)
                {
                    var employees = await _employeeService.GetAllEmployees();
                    if (employees != null && employees.Data != null)
                    {
                        var filtered = employees.Data
                            .Where(e => e.PlantId == response.Data.PlantId);

                        ViewBag.EmployeeList = filtered;
                    }
                    if (response.Data.StatusId == 9)
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == 9 || a.Id == 10);
                    }
                    else if (response.Data.StatusId == 10)
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == 10);
                    }
                    else if (response.Data.StatusId == 12)
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == 10 || a.Id == 12);
                    }
                    else
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == response.Data.StatusId);
                    }

                    var approvalDetails = await _cprService.GetCPRApprovalDetailsByCPRId(id ?? 0);
                    ViewBag.ApprovalDetails = approvalDetails.Data;

                    bool isAdmin = false;
                    var EmpData = await _employeeService.GetByIdAsync(currentUserId);
                    if (EmpData != null && EmpData.Data != null)
                    {
                        var role = EmpData.Data.OrgRole;
                        if (role != null && role.ToLower().Contains("admin") == true)
                        {
                            if (!role.ToLower().Contains("site admin"))
                            {
                                isAdmin = true;
                            }
                        }
                    }
                    ViewBag.IsAdmin = isAdmin;

                    // Set any UI defaults if needed
                    response.Data.RequestedDate = response.Data.RequestedDate;
                    return View(response.Data);
                }
            }
            var employeeList = await _employeeService.GetAllEmployees();
            if (employeeList != null && employeeList.Data != null && employeeList.Data.Count() > 0)
            {
                ViewBag.EmployeeList = employeeList.Data.Where(a => a.PlantId != null
                && plantList.Data.Any(b => b.IsCPREnable != null && b.IsCPREnable == true && b.Id == a.PlantId));
            }

            ViewBag.StatusList = statusList.Data.Where(a => a.Id == Constants.CPRStatusType.Awaiting_Approval || a.Id == Constants.CPRStatusType.Open);
            // defaults for new record
            model.RequestedDate = DateTime.Now;
            model.StageId = 13; //Generate
            model.StatusId = 9; //Open
            return View(model);
        }


        // Create/Edit page (GET)
        [CheckAccess(ScreenCode.CPRMaster, PermissionTypeConstant.Edit)]
        public async Task<IActionResult> Edit(long id)
        {
            var model = new CPRMasterModel();
            // CPRController.CreateEdit(long? id)
            if (model.SessionId == null)
                model.SessionId = Guid.NewGuid().ToString("N");

            // put your own claim type here; commonly NameIdentifier or a custom "UserId"
            var userIdStr = User.Claims.FirstOrDefault(c =>
                                c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                             || c.Type.Equals(System.Security.Claims.ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value;
            var currentUserId = string.IsNullOrWhiteSpace(userIdStr) ? 0 : int.Parse(userIdStr);
            ViewBag.CurrentUserId = currentUserId;
            var stageList = await _cprService.GetCPRMasterStage();
            ViewBag.StageList = stageList.Data;
            var statusList = await _cprService.GetCPRMasterStatus();

            //model.EDC = DateTime.Now;   // ensure not null
            //model.ADC = DateTime.Now;   // ensure not null

            var plantList = await _plantService.GetPlantList();
            ViewBag.PlantList = plantList.Data;


            if (id != null && id > 0)
            {
                var response = await _cprService.GetByIdAsync(id);
                if (response.IsSuccess && response.Data != null)
                {
                    var employees = await _employeeService.GetAllEmployees();
                    if (employees != null && employees.Data != null)
                    {
                        var filtered = employees.Data
                            .Where(e => e.PlantId == response.Data.PlantId);

                        ViewBag.EmployeeList = filtered;
                    }
                    if (response.Data.StatusId == 9)
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == 9 || a.Id == 10);
                    }
                    else if (response.Data.StatusId == 10)
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == 10);
                    }
                    else if (response.Data.StatusId == 12)
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == 10 || a.Id == 12);
                    }
                    else
                    {
                        ViewBag.StatusList = statusList.Data.Where(a => a.Id == response.Data.StatusId);
                    }

                    var approvalDetails = await _cprService.GetCPRApprovalDetailsByCPRId(id);
                    ViewBag.ApprovalDetails = approvalDetails.Data;

                    bool isAdmin = false;
                    var EmpData = await _employeeService.GetByIdAsync(currentUserId);
                    if (EmpData != null && EmpData.Data != null)
                    {
                        var role = EmpData.Data.OrgRole;
                        if (role != null && role.ToLower().Contains("admin") == true)
                        {
                            if (!role.ToLower().Contains("site admin"))
                            {
                                isAdmin = true;
                            }
                        }
                    }
                    ViewBag.IsAdmin = isAdmin;

                    // Set any UI defaults if needed
                    response.Data.RequestedDate = response.Data.RequestedDate;
                    return View("CreateEdit", response.Data);
                }
            }
            //var employeeList = await _employeeService.GetAllEmployees();
            //if (employeeList != null && employeeList.Data != null && employeeList.Data.Count() > 0)
            //{
            //    ViewBag.EmployeeList = employeeList.Data.Where(a => a.PlantId != null
            //    && plantList.Data.Any(b => b.IsCPREnable != null && b.IsCPREnable == true && b.Id == a.PlantId));
            //}

            //ViewBag.StatusList = statusList.Data.Where(a => a.Id == 9);
            //// defaults for new record
            //model.RequestedDate = DateTime.Now;
            //model.StageId = 13; //Generate
            //model.StatusId = 9; //Open
            return Ok("CPR request not found or this page is only for approval. To submit a CPR request, please use the CPR Form.");
        }

        [CheckAccess(ScreenCode.CPRAddFormMaster, PermissionTypeConstant.Add)]
        public async Task<IActionResult> CreateCPR()
        {
            var model = new CPRMasterModel();
            // CPRController.CreateEdit(long? id)
            if (model.SessionId == null)
                model.SessionId = Guid.NewGuid().ToString("N");

            // put your own claim type here; commonly NameIdentifier or a custom "UserId"
            var userIdStr = User.Claims.FirstOrDefault(c =>
                                c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase)
                             || c.Type.Equals(System.Security.Claims.ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value;
            var currentUserId = string.IsNullOrWhiteSpace(userIdStr) ? 0 : int.Parse(userIdStr);
            ViewBag.CurrentUserId = currentUserId;
            var stageList = await _cprService.GetCPRMasterStage();
            ViewBag.StageList = stageList.Data;
            var statusList = await _cprService.GetCPRMasterStatus();

            //model.EDC = DateTime.Now;   // ensure not null
            //model.ADC = DateTime.Now;   // ensure not null

            var plantList = await _plantService.GetPlantList();
            ViewBag.PlantList = plantList.Data;

            var employeeList = await _employeeService.GetAllEmployees();
            if (employeeList != null && employeeList.Data != null && employeeList.Data.Count() > 0)
            {
                ViewBag.EmployeeList = employeeList.Data.Where(a => a.PlantId != null
                && plantList.Data.Any(b => b.IsCPREnable != null && b.IsCPREnable == true && b.Id == a.PlantId));
            }
            ViewBag.StageList = stageList.Data.Where(a => a.Id == Constants.CPRStage.Level_I);
            ViewBag.StatusList = statusList.Data.Where(a => a.Id == Constants.CPRStatusType.Awaiting_Approval || a.Id == Constants.CPRStatusType.Open);
            // defaults for new record
            model.RequestedDate = DateTime.Now;
            model.StageId = Constants.CPRStage.Level_I;
            model.StatusId = Constants.CPRStatusType.Open; //Open
            return View(model);
        }


        // Create/Edit (POST) — called via AJAX from form submit
        //[HttpPost]
        //public async Task<IActionResult> CreateEdit(CPRMasterModel model)
        //{
        //    model.DocumentFilePath = model.DocumentFilePath1;//
        //    var response = await _cprService.InsertAsync(model);
        //    if (response.IsSuccess)
        //    {
        //        return Ok(response.Data);
        //    }
        //    return Ok(); // keep same behavior as your Customer controller
        //}
        [HttpPost]
        public async Task<IActionResult> CreateEdit(CPRMasterModel model)
        {
            model.DocumentFilePath = model.DocumentFilePath1;

            //// ✅ Get current user
            //var userIdStr = User.Claims.FirstOrDefault(c =>
            //    c.Type.Equals("UserId", StringComparison.OrdinalIgnoreCase) ||
            //    c.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)
            //)?.Value;

            //var currentUserId = string.IsNullOrWhiteSpace(userIdStr) ? 0 : int.Parse(userIdStr);

            //// ✅ Get existing CPR (only if edit)
            //if (model.Id > 0)
            //{
            //    var existing = await _cprService.GetByIdAsync(model.Id);

            //    if (existing == null || existing.Data == null)
            //        return BadRequest("Invalid CPR");

            //    // ✅ Check Admin
            //    bool isAdmin = false;
            //    var emp = await _employeeService.GetByIdAsync(currentUserId);
            //    if (emp?.Data?.OrgRole != null &&
            //        emp.Data.OrgRole.ToLower().Contains("admin") &&
            //        !emp.Data.OrgRole.ToLower().Contains("site admin"))
            //    {
            //        isAdmin = true;
            //    }

            //    // ✅ Check Requestor
            //    bool isRequestedUser = existing.Data.RequestedById == currentUserId;

            //    // ❌ BLOCK if not allowed
            //    if (!isAdmin && !isRequestedUser)
            //    {
            //        return Unauthorized("You are not allowed to edit this CPR.");
            //    }
            //}

            // ✅ Proceed
            var response = await _cprService.InsertAsync(model);

            if (response.IsSuccess)
                return Ok(response.Data);

            return Ok();
        }

        // DataTables server-side
        [HttpPost]
        public async Task<IActionResult> GetData(DataSourceRequest dataSourceRequest)
        {
            var response = await _cprService.GetAllAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        // Delete (soft delete)
        //[HttpPost]   // kept same style as your Customer Delete (no attribute)
        public async Task<IActionResult> DeleteCPRMaster(long id)
        {
            var response = await _cprService.DeleteAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> GetCPRStatusType()
        {
            var response = await _cprService.GetCPRMasterStatus();
            if (response.IsSuccess)
            {
                return await Task.Run(() => Json(response));
            }
            return Json(new { });
        }
        [HttpGet]
        public async Task<IActionResult> GetCPRMasterStage()
        {
            var response = await _cprService.GetCPRMasterStage();
            if (response.IsSuccess)
            {
                return await Task.Run(() => Json(response));
            }
            return Json(new { });
        }

        // DataTables server-side
        [HttpPost]
        public async Task<IActionResult> GetDataBySP(DataSourceRequest dataSourceRequest)
        {
            var response = await _cprService.GetAllBySPAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }


        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadDocument1(IEnumerable<IFormFile> file)
        {
            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot");
            path = Path.Combine(path, "CPRDocument");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string finalFiles = "";

            if (file != null)
            {
                foreach (IFormFile fl in file)
                {
                    if (fl.Length > 0)
                    {
                        var fileSpit = fl.FileName.Split(".");
                        var key = fileSpit.FirstOrDefault() + "_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + "." + fileSpit.LastOrDefault();
                        var filePath = Path.Combine(path, key);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await fl.CopyToAsync(fileStream);
                        }
                        finalFiles = finalFiles == "" ? key : finalFiles + "," + key;
                    }
                }
            }
            else
            {
                return BadRequest();
            }

            return Ok(new { filestring = finalFiles });
        }

        public async Task<IActionResult> DownloadCPRAttachment(string FileName)
        {
            if (FileName == null)
                return Content("filename not present");

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot");
            path = Path.Combine(path, "CPRDocument", FileName);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                //{".txt", "text/plain"},
                //{".pdf", "application/pdf"},
                //{".doc", "application/vnd.ms-word"},
                //{".docx", "application/vnd.ms-word"},
                //{".xls", "application/vnd.ms-excel"},
                //{".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                //{".png", "image/png"},
                //{".jpg", "image/jpeg"},
                //{".jpeg", "image/jpeg"},
                //{".gif", "image/gif"},
                //{".csv", "text/csv"},
                {".323", "text/h323"},
        {".3g2", "video/3gpp2"},
        {".3gp", "video/3gpp"},
        {".3gp2", "video/3gpp2"},
        {".3gpp", "video/3gpp"},
        {".7z", "application/x-7z-compressed"},
        {".aa", "audio/audible"},
        {".AAC", "audio/aac"},
        {".aaf", "application/octet-stream"},
        {".aax", "audio/vnd.audible.aax"},
        {".ac3", "audio/ac3"},
        {".aca", "application/octet-stream"},
        {".accda", "application/msaccess.addin"},
        {".accdb", "application/msaccess"},
        {".accdc", "application/msaccess.cab"},
        {".accde", "application/msaccess"},
        {".accdr", "application/msaccess.runtime"},
        {".accdt", "application/msaccess"},
        {".accdw", "application/msaccess.webapplication"},
        {".accft", "application/msaccess.ftemplate"},
        {".acx", "application/internet-property-stream"},
        {".AddIn", "text/xml"},
        {".ade", "application/msaccess"},
        {".adobebridge", "application/x-bridge-url"},
        {".adp", "application/msaccess"},
        {".ADT", "audio/vnd.dlna.adts"},
        {".ADTS", "audio/aac"},
        {".afm", "application/octet-stream"},
        {".ai", "application/postscript"},
        {".aif", "audio/x-aiff"},
        {".aifc", "audio/aiff"},
        {".aiff", "audio/aiff"},
        {".air", "application/vnd.adobe.air-application-installer-package+zip"},
        {".amc", "application/x-mpeg"},
        {".application", "application/x-ms-application"},
        {".art", "image/x-jg"},
        {".asa", "application/xml"},
        {".asax", "application/xml"},
        {".ascx", "application/xml"},
        {".asd", "application/octet-stream"},
        {".asf", "video/x-ms-asf"},
        {".ashx", "application/xml"},
        {".asi", "application/octet-stream"},
        {".asm", "text/plain"},
        {".asmx", "application/xml"},
        {".aspx", "application/xml"},
        {".asr", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".atom", "application/atom+xml"},
        {".au", "audio/basic"},
        {".avi", "video/x-msvideo"},
        {".axs", "application/olescript"},
        {".bas", "text/plain"},
        {".bcpio", "application/x-bcpio"},
        {".bin", "application/octet-stream"},
        {".bmp", "image/bmp"},
        {".c", "text/plain"},
        {".cab", "application/octet-stream"},
        {".caf", "audio/x-caf"},
        {".calx", "application/vnd.ms-office.calx"},
        {".cat", "application/vnd.ms-pki.seccat"},
        {".cc", "text/plain"},
        {".cd", "text/plain"},
        {".cdda", "audio/aiff"},
        {".cdf", "application/x-cdf"},
        {".cer", "application/x-x509-ca-cert"},
        {".chm", "application/octet-stream"},
        {".class", "application/x-java-applet"},
        {".clp", "application/x-msclip"},
        {".cmx", "image/x-cmx"},
        {".cnf", "text/plain"},
        {".cod", "image/cis-cod"},
        {".config", "application/xml"},
        {".contact", "text/x-ms-contact"},
        {".coverage", "application/xml"},
        {".cpio", "application/x-cpio"},
        {".cpp", "text/plain"},
        {".crd", "application/x-mscardfile"},
        {".crl", "application/pkix-crl"},
        {".crt", "application/x-x509-ca-cert"},
        {".cs", "text/plain"},
        {".csdproj", "text/plain"},
        {".csh", "application/x-csh"},
        {".csproj", "text/plain"},
        {".css", "text/css"},
        {".csv", "text/csv"},
        {".cur", "application/octet-stream"},
        {".cxx", "text/plain"},
        {".dat", "application/octet-stream"},
        {".datasource", "application/xml"},
        {".dbproj", "text/plain"},
        {".dcr", "application/x-director"},
        {".def", "text/plain"},
        {".deploy", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dgml", "application/xml"},
        {".dib", "image/bmp"},
        {".dif", "video/x-dv"},
        {".dir", "application/x-director"},
        {".disco", "text/xml"},
        {".dll", "application/x-msdownload"},
        {".dll.config", "text/xml"},
        {".dlm", "text/dlm"},
        {".doc", "application/msword"},
        {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".dot", "application/msword"},
        {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
        {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
        {".dsp", "application/octet-stream"},
        {".dsw", "text/plain"},
        {".dtd", "text/xml"},
        {".dtsConfig", "text/xml"},
        {".dv", "video/x-dv"},
        {".dvi", "application/x-dvi"},
        {".dwf", "drawing/x-dwf"},
        {".dwp", "application/octet-stream"},
        {".dxr", "application/x-director"},
        {".eml", "message/rfc822"},
        {".emz", "application/octet-stream"},
        {".eot", "application/octet-stream"},
        {".eps", "application/postscript"},
        {".etl", "application/etl"},
        {".etx", "text/x-setext"},
        {".evy", "application/envoy"},
        {".exe", "application/octet-stream"},
        {".exe.config", "text/xml"},
        {".fdf", "application/vnd.fdf"},
        {".fif", "application/fractals"},
        {".filters", "Application/xml"},
        {".fla", "application/octet-stream"},
        {".flr", "x-world/x-vrml"},
        {".flv", "video/x-flv"},
        {".fsscript", "application/fsharp-script"},
        {".fsx", "application/fsharp-script"},
        {".generictest", "application/xml"},
        {".gif", "image/gif"},
        {".group", "text/x-ms-group"},
        {".gsm", "audio/x-gsm"},
        {".gtar", "application/x-gtar"},
        {".gz", "application/x-gzip"},
        {".h", "text/plain"},
        {".hdf", "application/x-hdf"},
        {".hdml", "text/x-hdml"},
        {".hhc", "application/x-oleobject"},
        {".hhk", "application/octet-stream"},
        {".hhp", "application/octet-stream"},
        {".hlp", "application/winhlp"},
        {".hpp", "text/plain"},
        {".hqx", "application/mac-binhex40"},
        {".hta", "application/hta"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".htt", "text/webviewhtml"},
        {".hxa", "application/xml"},
        {".hxc", "application/xml"},
        {".hxd", "application/octet-stream"},
        {".hxe", "application/xml"},
        {".hxf", "application/xml"},
        {".hxh", "application/octet-stream"},
        {".hxi", "application/octet-stream"},
        {".hxk", "application/xml"},
        {".hxq", "application/octet-stream"},
        {".hxr", "application/octet-stream"},
        {".hxs", "application/octet-stream"},
        {".hxt", "text/html"},
        {".hxv", "application/xml"},
        {".hxw", "application/octet-stream"},
        {".hxx", "text/plain"},
        {".i", "text/plain"},
        {".ico", "image/x-icon"},
        {".ics", "application/octet-stream"},
        {".idl", "text/plain"},
        {".ief", "image/ief"},
        {".iii", "application/x-iphone"},
        {".inc", "text/plain"},
        {".inf", "application/octet-stream"},
        {".inl", "text/plain"},
        {".ins", "application/x-internet-signup"},
        {".ipa", "application/x-itunes-ipa"},
        {".ipg", "application/x-itunes-ipg"},
        {".ipproj", "text/plain"},
        {".ipsw", "application/x-itunes-ipsw"},
        {".iqy", "text/x-ms-iqy"},
        {".isp", "application/x-internet-signup"},
        {".ite", "application/x-itunes-ite"},
        {".itlp", "application/x-itunes-itlp"},
        {".itms", "application/x-itunes-itms"},
        {".itpc", "application/x-itunes-itpc"},
        {".IVF", "video/x-ivf"},
        {".jar", "application/java-archive"},
        {".java", "application/octet-stream"},
        {".jck", "application/liquidmotion"},
        {".jcz", "application/liquidmotion"},
        {".jfif", "image/pjpeg"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpb", "application/octet-stream"},
        {".jpe", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".json", "application/json"},
        {".jsx", "text/jscript"},
        {".jsxbin", "text/plain"},
        {".latex", "application/x-latex"},
        {".library-ms", "application/windows-library+xml"},
        {".lit", "application/x-ms-reader"},
        {".loadtest", "application/xml"},
        {".lpk", "application/octet-stream"},
        {".lsf", "video/x-la-asf"},
        {".lst", "text/plain"},
        {".lsx", "video/x-la-asf"},
        {".lzh", "application/octet-stream"},
        {".m13", "application/x-msmediaview"},
        {".m14", "application/x-msmediaview"},
        {".m1v", "video/mpeg"},
        {".m2t", "video/vnd.dlna.mpeg-tts"},
        {".m2ts", "video/vnd.dlna.mpeg-tts"},
        {".m2v", "video/mpeg"},
        {".m3u", "audio/x-mpegurl"},
        {".m3u8", "audio/x-mpegurl"},
        {".m4a", "audio/m4a"},
        {".m4b", "audio/m4b"},
        {".m4p", "audio/m4p"},
        {".m4r", "audio/x-m4r"},
        {".m4v", "video/x-m4v"},
        {".mac", "image/x-macpaint"},
        {".mak", "text/plain"},
        {".man", "application/x-troff-man"},
        {".manifest", "application/x-ms-manifest"},
        {".map", "text/plain"},
        {".master", "application/xml"},
        {".mda", "application/msaccess"},
        {".mdb", "application/x-msaccess"},
        {".mde", "application/msaccess"},
        {".mdp", "application/octet-stream"},
        {".me", "application/x-troff-me"},
        {".mfp", "application/x-shockwave-flash"},
        {".mht", "message/rfc822"},
        {".mhtml", "message/rfc822"},
        {".mid", "audio/mid"},
        {".midi", "audio/mid"},
        {".mix", "application/octet-stream"},
        {".mk", "text/plain"},
        {".mmf", "application/x-smaf"},
        {".mno", "text/xml"},
        {".mny", "application/x-msmoney"},
        {".mod", "video/mpeg"},
        {".mov", "video/quicktime"},
        {".movie", "video/x-sgi-movie"},
        {".mp2", "video/mpeg"},
        {".mp2v", "video/mpeg"},
        {".mp3", "audio/mpeg"},
        {".mp4", "video/mp4"},
        {".mp4v", "video/mp4"},
        {".mpa", "video/mpeg"},
        {".mpe", "video/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpf", "application/vnd.ms-mediapackage"},
        {".mpg", "video/mpeg"},
        {".mpp", "application/vnd.ms-project"},
        {".mpv2", "video/mpeg"},
        {".mqv", "video/quicktime"},
        {".ms", "application/x-troff-ms"},
        {".msi", "application/octet-stream"},
        {".mso", "application/octet-stream"},
        {".mts", "video/vnd.dlna.mpeg-tts"},
        {".mtx", "application/xml"},
        {".mvb", "application/x-msmediaview"},
        {".mvc", "application/x-miva-compiled"},
        {".mxp", "application/x-mmxp"},
        {".nc", "application/x-netcdf"},
        {".nsc", "video/x-ms-asf"},
        {".nws", "message/rfc822"},
        {".ocx", "application/octet-stream"},
        {".oda", "application/oda"},
        {".odc", "text/x-ms-odc"},
        {".odh", "text/plain"},
        {".odl", "text/plain"},
        {".odp", "application/vnd.oasis.opendocument.presentation"},
        {".ods", "application/oleobject"},
        {".odt", "application/vnd.oasis.opendocument.text"},
        {".one", "application/onenote"},
        {".onea", "application/onenote"},
        {".onepkg", "application/onenote"},
        {".onetmp", "application/onenote"},
        {".onetoc", "application/onenote"},
        {".onetoc2", "application/onenote"},
        {".orderedtest", "application/xml"},
        {".osdx", "application/opensearchdescription+xml"},
        {".p10", "application/pkcs10"},
        {".p12", "application/x-pkcs12"},
        {".p7b", "application/x-pkcs7-certificates"},
        {".p7c", "application/pkcs7-mime"},
        {".p7m", "application/pkcs7-mime"},
        {".p7r", "application/x-pkcs7-certreqresp"},
        {".p7s", "application/pkcs7-signature"},
        {".pbm", "image/x-portable-bitmap"},
        {".pcast", "application/x-podcast"},
        {".pct", "image/pict"},
        {".pcx", "application/octet-stream"},
        {".pcz", "application/octet-stream"},
        {".pdf", "application/pdf"},
        {".pfb", "application/octet-stream"},
        {".pfm", "application/octet-stream"},
        {".pfx", "application/x-pkcs12"},
        {".pgm", "image/x-portable-graymap"},
        {".pic", "image/pict"},
        {".pict", "image/pict"},
        {".pkgdef", "text/plain"},
        {".pkgundef", "text/plain"},
        {".pko", "application/vnd.ms-pki.pko"},
        {".pls", "audio/scpls"},
        {".pma", "application/x-perfmon"},
        {".pmc", "application/x-perfmon"},
        {".pml", "application/x-perfmon"},
        {".pmr", "application/x-perfmon"},
        {".pmw", "application/x-perfmon"},
        {".png", "image/png"},
        {".pnm", "image/x-portable-anymap"},
        {".pnt", "image/x-macpaint"},
        {".pntg", "image/x-macpaint"},
        {".pnz", "image/png"},
        {".pot", "application/vnd.ms-powerpoint"},
        {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
        {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
        {".ppa", "application/vnd.ms-powerpoint"},
        {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
        {".ppm", "image/x-portable-pixmap"},
        {".pps", "application/vnd.ms-powerpoint"},
        {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
        {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".prf", "application/pics-rules"},
        {".prm", "application/octet-stream"},
        {".prx", "application/octet-stream"},
        {".ps", "application/postscript"},
        {".psc1", "application/PowerShell"},
        {".psd", "application/octet-stream"},
        {".psess", "application/xml"},
        {".psm", "application/octet-stream"},
        {".psp", "application/octet-stream"},
        {".pub", "application/x-mspublisher"},
        {".pwz", "application/vnd.ms-powerpoint"},
        {".qht", "text/x-html-insertion"},
        {".qhtm", "text/x-html-insertion"},
        {".qt", "video/quicktime"},
        {".qti", "image/x-quicktime"},
        {".qtif", "image/x-quicktime"},
        {".qtl", "application/x-quicktimeplayer"},
        {".qxd", "application/octet-stream"},
        {".ra", "audio/x-pn-realaudio"},
        {".ram", "audio/x-pn-realaudio"},
        {".rar", "application/octet-stream"},
        {".ras", "image/x-cmu-raster"},
        {".rat", "application/rat-file"},
        {".rc", "text/plain"},
        {".rc2", "text/plain"},
        {".rct", "text/plain"},
        {".rdlc", "application/xml"},
        {".resx", "application/xml"},
        {".rf", "image/vnd.rn-realflash"},
        {".rgb", "image/x-rgb"},
        {".rgs", "text/plain"},
        {".rm", "application/vnd.rn-realmedia"},
        {".rmi", "audio/mid"},
        {".rmp", "application/vnd.rn-rn_music_package"},
        {".roff", "application/x-troff"},
        {".rpm", "audio/x-pn-realaudio-plugin"},
        {".rqy", "text/x-ms-rqy"},
        {".rtf", "application/rtf"},
        {".rtx", "text/richtext"},
        {".ruleset", "application/xml"},
        {".s", "text/plain"},
        {".safariextz", "application/x-safari-safariextz"},
        {".scd", "application/x-msschedule"},
        {".sct", "text/scriptlet"},
        {".sd2", "audio/x-sd2"},
        {".sdp", "application/sdp"},
        {".sea", "application/octet-stream"},
        {".searchConnector-ms", "application/windows-search-connector+xml"},
        {".setpay", "application/set-payment-initiation"},
        {".setreg", "application/set-registration-initiation"},
        {".settings", "application/xml"},
        {".sgimb", "application/x-sgimb"},
        {".sgml", "text/sgml"},
        {".sh", "application/x-sh"},
        {".shar", "application/x-shar"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".sitemap", "application/xml"},
        {".skin", "application/xml"},
        {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
        {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
        {".slk", "application/vnd.ms-excel"},
        {".sln", "text/plain"},
        {".slupkg-ms", "application/x-ms-license"},
        {".smd", "audio/x-smd"},
        {".smi", "application/octet-stream"},
        {".smx", "audio/x-smd"},
        {".smz", "audio/x-smd"},
        {".snd", "audio/basic"},
        {".snippet", "application/xml"},
        {".snp", "application/octet-stream"},
        {".sol", "text/plain"},
        {".sor", "text/plain"},
        {".spc", "application/x-pkcs7-certificates"},
        {".spl", "application/futuresplash"},
        {".src", "application/x-wais-source"},
        {".srf", "text/plain"},
        {".SSISDeploymentManifest", "text/xml"},
        {".ssm", "application/streamingmedia"},
        {".sst", "application/vnd.ms-pki.certstore"},
        {".stl", "application/vnd.ms-pki.stl"},
        {".sv4cpio", "application/x-sv4cpio"},
        {".sv4crc", "application/x-sv4crc"},
        {".svc", "application/xml"},
        {".swf", "application/x-shockwave-flash"},
        {".t", "application/x-troff"},
        {".tar", "application/x-tar"},
        {".tcl", "application/x-tcl"},
        {".testrunconfig", "application/xml"},
        {".testsettings", "application/xml"},
        {".tex", "application/x-tex"},
        {".texi", "application/x-texinfo"},
        {".texinfo", "application/x-texinfo"},
        {".tgz", "application/x-compressed"},
        {".thmx", "application/vnd.ms-officetheme"},
        {".thn", "application/octet-stream"},
        {".tif", "image/tiff"},
        {".tiff", "image/tiff"},
        {".tlh", "text/plain"},
        {".tli", "text/plain"},
        {".toc", "application/octet-stream"},
        {".tr", "application/x-troff"},
        {".trm", "application/x-msterminal"},
        {".trx", "application/xml"},
        {".ts", "video/vnd.dlna.mpeg-tts"},
        {".tsv", "text/tab-separated-values"},
        {".ttf", "application/octet-stream"},
        {".tts", "video/vnd.dlna.mpeg-tts"},
        {".txt", "text/plain"},
        {".u32", "application/octet-stream"},
        {".uls", "text/iuls"},
        {".user", "text/plain"},
        {".ustar", "application/x-ustar"},
        {".vb", "text/plain"},
        {".vbdproj", "text/plain"},
        {".vbk", "video/mpeg"},
        {".vbproj", "text/plain"},
        {".vbs", "text/vbscript"},
        {".vcf", "text/x-vcard"},
        {".vcproj", "Application/xml"},
        {".vcs", "text/plain"},
        {".vcxproj", "Application/xml"},
        {".vddproj", "text/plain"},
        {".vdp", "text/plain"},
        {".vdproj", "text/plain"},
        {".vdx", "application/vnd.ms-visio.viewer"},
        {".vml", "text/xml"},
        {".vscontent", "application/xml"},
        {".vsct", "text/xml"},
        {".vsd", "application/vnd.visio"},
        {".vsi", "application/ms-vsi"},
        {".vsix", "application/vsix"},
        {".vsixlangpack", "text/xml"},
        {".vsixmanifest", "text/xml"},
        {".vsmdi", "application/xml"},
        {".vspscc", "text/plain"},
        {".vss", "application/vnd.visio"},
        {".vsscc", "text/plain"},
        {".vssettings", "text/xml"},
        {".vssscc", "text/plain"},
        {".vst", "application/vnd.visio"},
        {".vstemplate", "text/xml"},
        {".vsto", "application/x-ms-vsto"},
        {".vsw", "application/vnd.visio"},
        {".vsx", "application/vnd.visio"},
        {".vtx", "application/vnd.visio"},
        {".wav", "audio/wav"},
        {".wave", "audio/wav"},
        {".wax", "audio/x-ms-wax"},
        {".wbk", "application/msword"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wcm", "application/vnd.ms-works"},
        {".wdb", "application/vnd.ms-works"},
        {".wdp", "image/vnd.ms-photo"},
        {".webarchive", "application/x-safari-webarchive"},
        {".webtest", "application/xml"},
        {".wiq", "application/xml"},
        {".wiz", "application/msword"},
        {".wks", "application/vnd.ms-works"},
        {".WLMP", "application/wlmoviemaker"},
        {".wlpginstall", "application/x-wlpg-detect"},
        {".wlpginstall3", "application/x-wlpg3-detect"},
        {".wm", "video/x-ms-wm"},
        {".wma", "audio/x-ms-wma"},
        {".wmd", "application/x-ms-wmd"},
        {".wmf", "application/x-msmetafile"},
        {".wml", "text/vnd.wap.wml"},
        {".wmlc", "application/vnd.wap.wmlc"},
        {".wmls", "text/vnd.wap.wmlscript"},
        {".wmlsc", "application/vnd.wap.wmlscriptc"},
        {".wmp", "video/x-ms-wmp"},
        {".wmv", "video/x-ms-wmv"},
        {".wmx", "video/x-ms-wmx"},
        {".wmz", "application/x-ms-wmz"},
        {".wpl", "application/vnd.ms-wpl"},
        {".wps", "application/vnd.ms-works"},
        {".wri", "application/x-mswrite"},
        {".wrl", "x-world/x-vrml"},
        {".wrz", "x-world/x-vrml"},
        {".wsc", "text/scriptlet"},
        {".wsdl", "text/xml"},
        {".wvx", "video/x-ms-wvx"},
        {".x", "application/directx"},
        {".xaf", "x-world/x-vrml"},
        {".xaml", "application/xaml+xml"},
        {".xap", "application/x-silverlight-app"},
        {".xbap", "application/x-ms-xbap"},
        {".xbm", "image/x-xbitmap"},
        {".xdr", "text/plain"},
        {".xht", "application/xhtml+xml"},
        {".xhtml", "application/xhtml+xml"},
        {".xla", "application/vnd.ms-excel"},
        {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
        {".xlc", "application/vnd.ms-excel"},
        {".xld", "application/vnd.ms-excel"},
        {".xlk", "application/vnd.ms-excel"},
        {".xll", "application/vnd.ms-excel"},
        {".xlm", "application/vnd.ms-excel"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
        {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".xlt", "application/vnd.ms-excel"},
        {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
        {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
        {".xlw", "application/vnd.ms-excel"},
        {".xml", "text/xml"},
        {".xmta", "application/xml"},
        {".xof", "x-world/x-vrml"},
        {".XOML", "text/plain"},
        {".xpm", "image/x-xpixmap"},
        {".xps", "application/vnd.ms-xpsdocument"},
        {".xrm-ms", "text/xml"},
        {".xsc", "application/xml"},
        {".xsd", "text/xml"},
        {".xsf", "text/xml"},
        {".xsl", "text/xml"},
        {".xslt", "text/xml"},
        {".xsn", "application/octet-stream"},
        {".xss", "application/xml"},
        {".xtp", "application/octet-stream"},
        {".xwd", "image/x-xwindowdump"},
        {".z", "application/x-compress"},
        {".zip", "application/x-zip-compressed"},
            };
        }

        // Update Stage and Status (POST) 
        [HttpPost]
        public async Task<IActionResult> UpdateStageStatus([FromBody] CPRStageStatusUpdateModel model)
        {
            if (model == null)
                return BadRequest("Invalid request data.");

            if (model.CPRId <= 0)
                return BadRequest("Invalid CPR ID.");

            try
            {
                // Example: Call your service layer to update the CPR stage status
                var result = await _cprService.UpdateStageStatus(model);

                if (result.IsSuccess)
                    return Ok(new { success = true, message = "Status updated successfully." });
                else
                    return BadRequest(new { success = false, message = "Failed to update status." });
            }
            catch (Exception ex)
            {
                // Log exception here (ex.Message)
                return StatusCode(500, new { success = false, message = "An error occurred while updating the status." });
            }
        }



        // EXPORT — same pattern as CustomerExport
        [HttpPost("CPRMasterExport")]
        public async Task<IActionResult> CPRMasterExport(DataSourceRequest dataSourceRequest)
        {
            try
            {
                var listResponse = await _cprService.GetAllBySPAsync(dataSourceRequest);
                if (!listResponse.IsSuccess || listResponse.Data == null)
                    return BadRequest();

                // NOTE: your Customer sample pulls data out of DataSourceResult.Data (JToken).
                // Keeping identical approach for compatibility.
                var items = JsonConvert.DeserializeObject<List<CPRMasterModel>>(
                    listResponse.Data.Data.ToString()
                ) ?? new List<CPRMasterModel>();

                string fileName = "CPRMasterConsolidatedList_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xlsx";
                string relativePath = $"wwwroot/Exported/{fileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                FileInfo file = new FileInfo(filePath);
                if (file.Directory != null && !file.Directory.Exists)
                    file.Directory.Create();

                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("CPRMasterList");

                    // Header row
                    int r = 1;
                    ws.Cells[r, 1].Value = "Sl. No.";
                    ws.Cells[r, 2].Value = "Unique Code";
                    ws.Cells[r, 3].Value = "Category";
                    ws.Cells[r, 4].Value = "Document No";
                    ws.Cells[r, 5].Value = "Requested Date";
                    ws.Cells[r, 6].Value = "Department";
                    ws.Cells[r, 7].Value = "Revision";
                    ws.Cells[r, 8].Value = "Program";
                    ws.Cells[r, 9].Value = "Action Requested";
                    ws.Cells[r, 10].Value = "Raised Due To";
                    ws.Cells[r, 11].Value = "Relevant Vertical Head(s)";
                    ws.Cells[r, 12].Value = "Process Owner(s)";
                    ws.Cells[r, 13].Value = "Current Stage";
                    ws.Cells[r, 14].Value = "Status";
                    ws.Cells[r, 15].Value = "QMS Status";
                    ws.Cells[r, 16].Value = "EDC";
                    ws.Cells[r, 17].Value = "ADC";
                    ws.Cells[r, 18].Value = "Is Closed?";

                    // Now total columns = 18
                    for (int c = 1; c <= 18; c++)
                    {
                        ws.Cells[r, c].Style.Font.Bold = true;
                        ws.Cells[r, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[r, c].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGoldenrodYellow);
                    }

                    // Data rows
                    int rowNo = 2;
                    int serial = 1;

                    foreach (var a in items)
                    {
                        ws.Cells[rowNo, 1].Value = serial++;
                        ws.Cells[rowNo, 2].Value = a.CPRUniqueCode;
                        ws.Cells[rowNo, 3].Value = a.CategoryName;      // ✅ Added
                        ws.Cells[rowNo, 4].Value = a.DocumentNo;        // ✅ Added
                        ws.Cells[rowNo, 5].Value = a.RequestedDate.ToString("dd-MMM-yyyy");
                        ws.Cells[rowNo, 6].Value = a.Department;
                        ws.Cells[rowNo, 7].Value = a.Revision;
                        ws.Cells[rowNo, 8].Value = a.Program;
                        ws.Cells[rowNo, 9].Value = a.ActionRequested;
                        ws.Cells[rowNo, 10].Value = a.CPRRaisedDueTo;
                        ws.Cells[rowNo, 11].Value = a.RelevantVerticalHeadNames;
                        ws.Cells[rowNo, 12].Value = a.ProcessOwnerNames;
                        ws.Cells[rowNo, 13].Value = a.StageName;
                        ws.Cells[rowNo, 14].Value = a.StatusName;
                        ws.Cells[rowNo, 15].Value = a.QMSStatus;
                        ws.Cells[rowNo, 16].Value = a.EDC != null ? a.EDC?.ToString("dd-MMM-yyyy") : "";
                        ws.Cells[rowNo, 17].Value = a.ADC != null ? a.ADC?.ToString("dd-MMM-yyyy") : "";
                        ws.Cells[rowNo, 18].Value = a.IsClosed != null && a.IsClosed == true ? "Yes" : "No";

                        rowNo++;
                    }


                    // Autofit a bit
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    package.Save();

                    // Your Customer sample returns "../ExPorted/" (note the casing); replicate that pattern:
                    return Ok(new FileResponseModel { FilePath = "../ExPorted/" + fileName, FileStatus = true });
                }
            }
            catch (Exception)
            {
                // swallow to keep same style as sample
            }
            return null;
        }

        // View as HTML (for preview)
        [HttpGet("/CPR/Print/{id:long}")]
        public async Task<IActionResult> Print(long id)
        {
            var m = await _cprService.GetCPRPrintByIdFromSp(id);
            if (m == null) return NotFound();

            // Build a file:// URI for the logo so wkhtmltopdf can read it
            var logoPath = Path.Combine(_hostingEnvironment.WebRootPath, "pdf-icons", "logo.png"); // adjust name/path
            ViewBag.LogoUri = new Uri(logoPath).AbsoluteUri;                          // file:///C:/...


            var vm = await BuildVmAsync(m.Data);
            return View("CPRPrint", vm);
        }

        // Export to PDF
        [HttpGet("/CPR/ExportPdf/{id:long}")]
        public async Task<IActionResult> ExportPdf(long id)
        {
            var m = await _cprService.GetCPRPrintByIdFromSp(id);
            if (m == null) return NotFound();

            // Build a file:// URI for the logo so wkhtmltopdf can read it
            var logoPath = Path.Combine(_hostingEnvironment.WebRootPath, "pdf-icons", "logo.png"); // adjust name/path
            ViewBag.LogoUri = new Uri(logoPath).AbsoluteUri;                          // file:///C:/...


            var vm = await BuildVmAsync(m.Data);

            var html = await this.RenderViewToStringAsync("CPRPrint", vm); // same Razor view
                                                                           // turn ~/xxx into file:///C:/…/wwwroot/xxx
            var webRoot = _hostingEnvironment.WebRootPath;
            html = RebaseStaticUrls(html, webRoot);

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    Margins = new MarginSettings { Top = 6, Bottom = 9, Left = 3, Right = 3 }
                },
                Objects =
        {
            new ObjectSettings
            {
                HtmlContent = html,
                WebSettings = new WebSettings
                {
                    DefaultEncoding = "utf-8",
                    LoadImages      = true,
                    EnableJavascript = true,
                    PrintMediaType   = true
                    // UserStyleSheet = Path.Combine(_env.WebRootPath, "css", "print.css") // optional
                },
                LoadSettings = new LoadSettings
                {
                    BlockLocalFileAccess = false,  // allow file://
                    StopSlowScript       = false
                },
                  // ✅ Add footer here
            FooterSettings = new FooterSettings
            {
                FontSize = 8,
                Line = false, // draws a line above footer
                Right = "Page [page] of [toPage]", // auto page numbers
                //Center = "Confidential Document", // your custom text
                Left = "Form B-13101-05_Rev.01_ Effective Date: 24-06-2024"
                //HtmlUrl = "path-to-footer.html" // alternatively use an HTML footer file
            }
            }
        }
            };


            var pdfBytes = _converter.Convert(doc);
            var fileName = $"{(vm.CPRNo ?? ("CPR-" + vm.Id))}.pdf";
            return File(pdfBytes, "application/pdf", fileName);

        }

        // --- helper: map your CPRMasterModel to the print VM
        private static bool Has(string src, string token) =>
     !string.IsNullOrWhiteSpace(src) &&
     src.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

        private async Task<CPRPrintModel> BuildVmAsync(CPRPrintModel m)
        {
            var vm = new CPRPrintModel
            {
                Id = m.Id,
                CPRNo = string.IsNullOrWhiteSpace(m.CPRNo) ? m.CPRUniqueCode : m.CPRNo,
                RequestedDate = m.RequestedDate,

                CategoryName = m.CategoryName,
                DocumentNo = m.DocumentNo,
                DepartmentTitle = m.DepartmentTitle,    // fixed: use proper title
                Department = m.Department,
                Program = m.Program,
                Revision = m.Revision,
                CreatedDate = m.CreatedDate,

                // Action Requested (checkmarks)
                IsNewRelease = Has(m.ActionRequested, "New Release"),
                IsRevision = Has(m.ActionRequested, "Revision"),
                IsObsolete = Has(m.ActionRequested, "Obsolete"),
                IsOtherAction = Has(m.ActionRequested, "Other"),

                // CPR Raised Due To (checkmarks)
                DueToCibChange = Has(m.CPRRaisedDueTo, "CIB change"),
                DueToImprovement = Has(m.CPRRaisedDueTo, "Improvement"),
                DueToAuditOutput = Has(m.CPRRaisedDueTo, "Audit"),
                DueToOther = Has(m.CPRRaisedDueTo, "Other"),

                SectionRequiringChange = m.SectionRequiringChange,

                ChangeDescription = m.ChangeDescription,

                AdminStatusId = m.AdminStatusId,
                QMSAdminStatus = m.QMSAdminStatus,
                ReasonForChange = m.ReasonForChange,
                QMSAdmin = m.QMSAdmin,
                AdminStatusDate = m.AdminStatusDate,
                ClosedDate = m.ClosedDate,
                EDC = m.EDC,
                ADC = m.ADC,

                RequestedByName = m.RequestedByName,

                // SP already returns comma-separated NAMES for both groups
                RelevantVerticalHeadNames = m.RelevantVerticalHeadNames ?? string.Empty,
                ProcessOwnerNames = m.ProcessOwnerNames ?? string.Empty,

                Remarks = m.Remarks,         // not in SP; set if you later add it

                Level1Approvals = m.Level1Approvals,
                Level2Approvals = m.Level2Approvals,
            };

            if (m.AdminStatusId != null && (m.AdminStatusId == Constants.CPRStatusType.Approved || m.AdminStatusId == Constants.CPRStatusType.Rejected))
            {
                vm.StatusName = m.QMSAdminStatus;
                vm.StatusId = (int)m.AdminStatusId;
            }
            else
            {
                vm.StatusName = m.StatusName;
                vm.StatusId = m.StatusId;
            }
            //string _downloadBaseUrl = _configuration["DocumentSettings:DownloadBaseUrl"];
            //if (!string.IsNullOrEmpty(m.DocumentFilePath))
            //{
            //    vm.Attachments = m.DocumentFilePath
            //    .Split(',', StringSplitOptions.RemoveEmptyEntries)
            //    .Select(file =>
            //    {
            //    var fileName = Path.GetFileName(file.Trim());

            //    return (
            //        DisplayName: Path.GetFileNameWithoutExtension(fileName),
            //        FileName: fileName,
            //        DownloadUrl: $"{_downloadBaseUrl}{fileName}"
            //        );
            //    })
            //    .ToList();
            //}
            //else
            //{
            //    vm.Attachments = new List<(string DisplayName, string FileName, string DownloadUrl)>();
            //}
            string _downloadBaseUrl = _configuration["DocumentSettings:DownloadBaseUrl"];

            if (!string.IsNullOrEmpty(m.DocumentFilePath))
            {
                vm.Attachments = m.DocumentFilePath
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(file => file.Trim())
                    // ✅ Only allow PDF files
                    .Where(file => Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    .Select(file =>
                    {
                        var fileName = Path.GetFileName(file);

                        return (
                            DisplayName: Path.GetFileNameWithoutExtension(fileName),
                            FileName: fileName,
                            DownloadUrl: $"{_downloadBaseUrl}{fileName}"
                        );
                    })
                    .ToList();
            }
            else
            {
                vm.Attachments = new List<(string DisplayName, string FileName, string DownloadUrl)>();
            }

            return vm;
        }

        private string RebaseStaticUrls(string html, string webRootPath)
        {
            // file:///C:/.../wwwroot/
            var baseFileUrl = $"file:///{webRootPath.Replace("\\", "/").TrimEnd('/')}/";

            // src="/xxx"  |  src='~/xxx'  |  href="/xxx"  ->  file:///.../wwwroot/xxx
            string pattern = @"(?<attr>src|href)\s*=\s*(?<q>['""])\s*(?<url>~?\/[^'"">\s]+)(?<q2>['""])";
            return Regex.Replace(html, pattern, m =>
            {
                var attr = m.Groups["attr"].Value;
                var q = m.Groups["q"].Value;         // the original quote
                var url = m.Groups["url"].Value.TrimStart('~', '/');  // remove ~/ or /
                return $"{attr}={q}{baseFileUrl}{url}{q}";
            }, RegexOptions.IgnoreCase);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeesByPlant(int plantId)
        {
            var employees = await _employeeService.GetAllEmployees();
            if (employees == null || employees.Data == null)
                return Json(new { data = new List<object>() });

            var filtered = employees.Data
                .Where(e => e.PlantId == plantId)
                .Select(e => new { e.Id, e.EmployeeName })
                .ToList();

            return Json(new { data = filtered });
        }
    }
}
