using CQS.Log.Engine.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CQS.Log.Engine.Controllers
{
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly LogContext _logContext;

        public LogController(LogContext logContext)
        {
            _logContext = logContext;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Exception(ExpectionModel expectionModel)
        {
            _logContext.Log.Add(new LogModel
            {
                ApplicationName = expectionModel.Application,
                ErrorMessage = expectionModel.Message,
                StackTrace = expectionModel.StackTrace,
                CreatedOn = DateTime.Now
            });
            await _logContext.SaveChangesAsync();
            return Ok();
        }
    }

    public class ExpectionModel
    {
        public string Message { get; set; }
        public string Application { get; set; }
        public string StackTrace { get; set; }
    }
}
