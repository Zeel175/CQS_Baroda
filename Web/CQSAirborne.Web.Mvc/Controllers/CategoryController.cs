using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly CategoryService _categoryService;
        private readonly PlantService _plantService;
        public CategoryController(CategoryService categoryService, PlantService plantService)
        {
            _categoryService = categoryService;
            _plantService = plantService;
        }

        [CheckAccess(ScreenCode.Category, PermissionTypeConstant.List)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCategoryData(DataSourceRequest dataSourceRequest)
        {
            var response = await _categoryService.GetAllAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryTypes()
        {
            var response = await _categoryService.GetCategoryTypes();
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetPrimaryCategories()
        {
            var response = await _categoryService.GetPrimaryCategories();
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategory(int id)
        {
            var response = await _categoryService.GetAllCategory(id);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }




        [HttpGet]
        [CheckAccess(ScreenCode.Category, PermissionTypeConstant.Add)]
        public async Task<IActionResult> Create()
        {
            var model = await _categoryService.GetCreateModel();
            return View(model.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddEditCategoryModel model)
        {
            var result = await _categoryService.InsertAsync(model);
            if (result.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            BindErrors(result);
            return View(model);
        }

        [HttpGet]
        [CheckAccess(ScreenCode.Category, PermissionTypeConstant.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoryService.GetEditModel(id);
            if (model.IsSuccess)
                return View(model.Data);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddEditCategoryModel model)
        {
            var result = await _categoryService.UpdateAsync(model);
            if (result.IsSuccess)
            {
                return RedirectToAction("Index");
            }
            BindErrors(result);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, bool status)
        {
            var response = await _categoryService.ChangeStatus(id, status);
            if (response.IsSuccess)
            {
                return Json(new { Status = true });
            }
            return Json(new { Status = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorywiseDocumentCount(int? plantId)
        {
            var response = await _categoryService.GetCategorywiseDocumentCount(plantId);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetSecondaryCategorywiseDocumentCount(int? plantId)
        {
            var response = await _categoryService.GetSecondaryCategorywiseDocumentCount(plantId);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorywiseDocumentCountByParentCategory(int categoryId, int? plantId)
        {
            var response = await _categoryService.GetCategorywiseDocumentCountByParentCategory(categoryId, plantId);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [CheckAccess(ScreenCode.TASLPCFCount, PermissionTypeConstant.List)]
        public async Task<IActionResult> TASLCount()
        {
            var plantList = await _plantService.GetPlantList();
            ViewBag.PlantList = plantList.Data;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SubCategoryChart(int categoryId, string categoryName, int? plantId)
        {
            var plantList = await _plantService.GetPlantList();
            ViewBag.PlantList = plantList.Data;
            ViewBag.PlantId = plantId;
            ViewBag.categoryId = categoryId;
            ViewBag.categoryName = categoryName;
            return View();
        }
    }
}