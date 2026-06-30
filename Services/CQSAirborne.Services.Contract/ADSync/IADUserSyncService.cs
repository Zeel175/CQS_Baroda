using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract.ADSync
{
    public interface IADUserSyncService
    {
        Task SyncNewUsersAsync();
    }
}
