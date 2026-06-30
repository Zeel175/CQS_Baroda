using System.Linq;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Plant;
using CQSAirborne.Services.API.Extensions;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CQSAirborne.Services.API.Controllers
{
    [Authorize]
    [Route("api/Plant")]
    [ApiController]
    public class PlantApiController : BaseController
    {
        private readonly IPlantService _plantService;
        private readonly IEmployeeService _employeeService;

        public PlantApiController(IPlantService plantService, IEmployeeService employeeService)
        {
            _plantService = plantService;
            _employeeService = employeeService;
        }

        [HttpPost("[action]")]
        public IActionResult Get([FromBody] DataSourceRequest dataSourceRequest)
        {
            var data = _plantService.GetAll()
                .ToDataSourceResult(dataSourceRequest);
            return Ok(data);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var model = await _plantService.GetByIdAsync(id);
            return Ok(model);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Post(AddEditPlantModel addEditPlantModel)
        {
            bool isSaved = await _plantService.InsertAsync(addEditPlantModel);
            if (!isSaved)
                return BadRequest(AddValidation("", "Unable to save data"));
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Put(AddEditPlantModel addEditPlantModel)
        {
            bool isSaved = await _plantService.UpdateAsync(addEditPlantModel);
            if (!isSaved)
                return BadRequest(AddValidation("", "Unable to save data"));
            return Ok();
        }

        [HttpGet("GetSelectList")]
        public IActionResult GetSelectList()
        {
            int employeeId = User.Identity.GetUserId();
            var empData = _employeeService.GetById(employeeId);
            var plantidlist = empData.PlantIds != null && empData.PlantIds != "" ? empData.PlantIds.Split(",") : null;
            if(plantidlist != null && plantidlist.Length > 0)
            {
                var data = _plantService.GetSelectList();
                var dt = data.Where(a => plantidlist.Any(b => b.ToString() == a.Id.ToString()));
                return Ok(dt);
            }
            else
            {
                var data = _plantService.GetSelectList();
                return Ok(data);
            }
        }

        [HttpPost("[action]/{id}/{status}")]
        public IActionResult ChangeStatus(int id, bool status)
        {
            bool isSaved = _plantService.ChangeStatus(id, status);
            if (!isSaved)
                return BadRequest();
            return Ok();
        }
    }
}