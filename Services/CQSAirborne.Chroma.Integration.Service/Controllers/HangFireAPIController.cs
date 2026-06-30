using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CQSAirborne.Model.Document;
using Hangfire;
using Microsoft.Extensions.Configuration;
using CQSAirborne.Chroma.Integration.Service.Helpers;

namespace CQSAirborne.Chroma.Integration.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HangFireApiController : ControllerBase
    {
        private readonly HangFIreService _hangFIreService;
        private readonly IBackgroundJobClient _jobManager;
        private readonly IConfiguration _configuration;
        public HangFireApiController(IBackgroundJobClient jobManager
            , IConfiguration configuration
            , HangFIreService hangFIreService)
        {
            _jobManager = jobManager;
            _configuration = configuration;
            _hangFIreService = hangFIreService;
        }
        [HttpPost("[action]")]
        public IActionResult EmailDocument(SendEmailModel sem)
        {
            var response = BackgroundJob.Enqueue(() => _hangFIreService.EmailDocument(sem));
            return Ok(response);
        }
    }
}
