using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Plant;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using Microsoft.AspNetCore.Mvc;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class PlantController : BaseController
    {
        private readonly PlantService _plantService;

        public PlantController(PlantService plantService)
        {
            _plantService = plantService;
        }

        [CheckAccess(ScreenCode.Plant, PermissionTypeConstant.List)]
        public IActionResult Index()
        {
            return View();
        }

        [CheckAccess(ScreenCode.Plant, PermissionTypeConstant.Add)]
        public IActionResult Create()
        {
            return View(new AddEditPlantModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AddEditPlantModel addEditPlantModel)
        {
            if (!ModelState.IsValid)
                return View(addEditPlantModel);

            var response = await _plantService.InsertAsync(addEditPlantModel);
            if (response.IsSuccess)
                return RedirectToAction("Index");

            BindErrors(response);
            return View(addEditPlantModel);
        }

        [CheckAccess(ScreenCode.Plant, PermissionTypeConstant.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _plantService.GetByIdAsync(id);
            if (model.IsSuccess)
            {
                return View(model.Data);
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Edit(AddEditPlantModel addEditPlantModel)
        {
            if (!ModelState.IsValid)
                return View(addEditPlantModel);

            var response = await _plantService.UpdateAsync(addEditPlantModel);
            if (response.IsSuccess)
                return RedirectToAction("Index");
            BindErrors(response);
            return View(addEditPlantModel);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SavePlant(OperationType operationType, AddEditPlantModel addEditPlantModel)
        //{
        //    string viewToReturn = operationType == OperationType.Add ? "Create" : "Edit";
        //    if (!ModelState.IsValid)
        //        return View(viewToReturn, addEditPlantModel);

        //    var response = await _saveStrategies[operationType](addEditPlantModel);
        //    if (response.IsSuccess)
        //        return RedirectToAction("Index");
        //    return View(viewToReturn, addEditPlantModel);
        //}

        [HttpPost]
        public async Task<IActionResult> GetPlantData(DataSourceRequest dataSourceRequest)
        {
            var response = await _plantService.GetAllAsync(dataSourceRequest);
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        public async Task<IActionResult> GetPlantList()
        {
            RestReponse<List<SelectListModel>> response = await _plantService.GetPlantList();
            if (response.IsSuccess)
            {
                return Json(response.Data);
            }
            return Json(new { });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, bool status)
        {
            var response = await _plantService.ChangeStatus(id, status);
            if (response.IsSuccess)
            {
                return Json(new { Status = true });
            }
            return Json(new { Status = false });
        }
    }

}