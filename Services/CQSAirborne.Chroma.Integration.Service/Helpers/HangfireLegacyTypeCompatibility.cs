using Microsoft.Extensions.Configuration;
using CQSAirborne.Data.Contract;

namespace CQS.Chroma.Integration.Service.Helpers
{
    /// <summary>
    /// Compatibility type for Hangfire jobs that were stored before the service was renamed.
    /// Hangfire persists the full CLR type name, so queued/failed jobs created by older builds
    /// still reference CQS.Chroma.Integration.Service.Helpers.EmployeeCsvService.
    /// </summary>
    public class EmployeeCsvService : CQSAirborne.Chroma.Integration.Service.Helpers.EmployeeCsvService
    {
        public EmployeeCsvService(IConfiguration configuration) : base(configuration)
        {
        }
    }

    /// <summary>
    /// Compatibility type for older Hangfire jobs that email documents.
    /// </summary>
    public class HangFIreService : CQSAirborne.Chroma.Integration.Service.Helpers.HangFIreService
    {
        public HangFIreService(IConfiguration configuration) : base(configuration)
        {
        }
    }
}

namespace CQS.Chroma.Integration.Service.Services
{
    /// <summary>
    /// Compatibility interface for recurring jobs persisted with the old service namespace.
    /// </summary>
    public interface IADUserSyncService
    {
        System.Threading.Tasks.Task SyncNewUsersAsync();
    }

    /// <summary>
    /// Compatibility type for AD sync jobs stored before the service assembly/namespace rename.
    /// </summary>
    public class ADUserSyncService : CQSAirborne.Chroma.Integration.Service.Services.ADUserSyncService, IADUserSyncService
    {
        public ADUserSyncService(IDataContext context, IConfiguration configuration) : base(context, configuration)
        {
        }
    }
}
