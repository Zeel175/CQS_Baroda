using System.Threading.Tasks;

namespace CQSAirborne.Chroma.Integration.Service.Services
{
    public interface IADUserSyncService
    {
        Task SyncNewUsersAsync();
    }
}
