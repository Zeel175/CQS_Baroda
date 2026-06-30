using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Employee;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _employeeService;
        private readonly RoleService _roleService;
        private readonly PlantService _plantService;

        public EmployeeController(EmployeeService employeeService, RoleService roleService, PlantService plantService)
        {
            _employeeService = employeeService;
            _roleService = roleService;
            _plantService = plantService;
        }

        [CheckAccess(ScreenCode.Employee, PermissionTypeConstant.List)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetData(DataSourceRequest dataSourceRequest)
        {
            var response = await _employeeService.GetAllAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> GetAllDataForViewPage(DataSourceRequest dataSourceRequest)
        {
            var response = await _employeeService.GetEmployeesForViewPageAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [CheckAccess(ScreenCode.Employee, PermissionTypeConstant.Add)]
        public async Task<IActionResult> Create()
        {
            var roleData = await _roleService.GetRoleList();
            ViewBag.Roles = roleData != null && roleData.Data != null ? roleData.Data : null;

            var plantData = await _plantService.GetPlantList();
            ViewBag.PlantList = plantData != null && plantData.Data != null ? plantData.Data : null;

            return View(new AddEditEmployeeViewModel());
        }

        [CheckAccess(ScreenCode.Employee, PermissionTypeConstant.Edit)]
        public async Task<IActionResult> Edit(long id)
        {
            var roleData = await _roleService.GetRoleList();
            ViewBag.Roles = roleData != null && roleData.Data != null ? roleData.Data : null;
            var plantData = await _plantService.GetPlantList();
            ViewBag.PlantList = plantData != null && plantData.Data != null ? plantData.Data : null;

            var response = await _employeeService.GetByIdAsync(id);
            if (response.IsSuccess)
            {
                return View(response.Data);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddEditEmployeeViewModel model)
        {
            model.IsManual = true;
            var response = await _employeeService.InsertAsync(model);
            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> Edit(AddEditEmployeeViewModel model)
        {
            model.IsManual = true;
            var response = await _employeeService.UpdateAsync(model);
            if (response.IsSuccess)
                return RedirectToAction("Index");
            return View(model);
        }

        public async Task<IActionResult> DownloadTemplate()
        {
            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot/TempDocuments", "Employee_Sample_Upload.xlsx");

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(path));

        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", "Please select file to upload");
                return View();
            }

            if (!new string[] { ".xlsx", ".xls" }.Contains(Path.GetExtension(file.FileName)))
            {
                ModelState.AddModelError("", "File format not supported. Please select excel file to upload employees.");
                return View();
            }


            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                var workbook = package.Workbook;

                var headers = workbook.Worksheets.First().GetHeaderColumns();
                if (headers.Count() != 20)
                {
                    ModelState.AddModelError("", "File is not as per provided template. Header data is incorrect.");
                    return View();
                }

                List<string> missingHeaders = new List<string>();
                if (!IsValidHeaderRow(headers, ref missingHeaders))
                {
                    ModelState.AddModelError("", $"Excel is not valid! Following headers are missing : {string.Join(',', missingHeaders)}");
                    return View();
                }

                var employeeData = workbook.Worksheets.First().ToEmployeeData();

                List<string> duplicateEmployeeIds = new List<string>();
                if (!IsUniqueEmployeeId(employeeData, ref duplicateEmployeeIds))
                {
                    foreach (var duplicateEmployee in duplicateEmployeeIds)
                    {
                        ModelState.AddModelError("", $"Employee Id {duplicateEmployee} is duplicate");
                    }
                    return View();
                }


                var response = await _employeeService.InsertBulkEmployee(employeeData);
                if (response.Data.IsSuccess)
                {
                    // ✅ Show Summary Message on Page
                    string msg =
        $"🟢 Created: {response.Data.CreatedCount}<br/>" +
        $"🟡 Updated: {response.Data.UpdatedCount}<br/>" +
        $"Updated EmpIds: {string.Join(", ", response.Data.UpdatedEmpIds)}";
                    
                    // ✅ Use ViewBag for success message
                    ViewBag.SuccessMessage = msg;

                    return View();
                }
                else
                {
                    ModelState.AddModelError("", "Unable to upload employee data.Please try again.");
                    return View();
                }
            }
        }

        private bool IsUniqueEmployeeId(List<AddEditEmployeeViewModel> employeeData, ref List<string> duplicateEmployeeIds)
        {
            duplicateEmployeeIds = employeeData.GroupBy(w => w.EmpId).Where(w => w.Count() > 1).Select(w => w.Key).ToList();
            return duplicateEmployeeIds.Count == 0;
        }

        private bool IsValidHeaderRow(string[] headers, ref List<string> missingHeaders)
        {
            if (headers[0] != "EmployeeName")
            {
                missingHeaders.Add("EmployeeName");
            }

            if (headers[1] != "GroupId")
            {
                missingHeaders.Add("GroupId");
            }

            if (headers[2] != "EmpId")
            {
                missingHeaders.Add("EmpId");
            }

            if (headers[3] != "Department")
            {
                missingHeaders.Add("Department");
            }

            if (headers[4] != "Designation")
            {
                missingHeaders.Add("Designation");
            }

            if (headers[5] != "ReportingManagerGID")
            {
                missingHeaders.Add("ReportingManagerGID");
            }

            if (headers[6] != "ReportingManagerEmpID")
            {
                missingHeaders.Add("ReportingManagerEmpID");
            }

            if (headers[7] != "ReportingManagerName")
            {
                missingHeaders.Add("ReportingManagerName");
            }

            if (headers[8] != "ReportingManagerEmailID")
            {
                missingHeaders.Add("ReportingManagerEmailID");
            }

            if (headers[9] != "HODGID")
            {
                missingHeaders.Add("HODGID");
            }

            if (headers[10] != "HODEmpID")
            {
                missingHeaders.Add("HODEmpID");
            }

            if (headers[11] != "HODName")
            {
                missingHeaders.Add("HODName");
            }

            if (headers[12] != "HODEmailID")
            {
                missingHeaders.Add("HODEmailID");
            }

            if (headers[13] != "OfficalEmpEmailID")
            {
                missingHeaders.Add("OfficalEmpEmailID");
            }

            if (headers[14] != "ADID")
            {
                missingHeaders.Add("ADID");
            }

            if (headers[15] != "Plant")
            {
                missingHeaders.Add("Plant");
            }

            if (headers[16] != "PlantAccess")
            {
                missingHeaders.Add("PlantAccess");
            }
            if (headers[17] != "UserName")
            {
                missingHeaders.Add("UserName");
            }
            if (headers[18] != "Password")
            {
                missingHeaders.Add("Password");
            }
            if (headers[19] != "OrgRole")
            {
                missingHeaders.Add("OrgRole");
            }
            return missingHeaders.Count == 0;
        }

        public async Task<IActionResult> DeleteEmployee(long id)
        {
            var response = await _employeeService.DeleteEmployee(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, bool status)
        {
            var response = await _employeeService.ChangeStatus(id, status);
            if (response.IsSuccess)
            {
                return Json(new { Status = true });
            }
            return Json(new { Status = false });
        }

    }
}