using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CQSAirborne.Model.CPRMaster; // adjust if you placed the CPR models elsewhere
using CQSAirborne.Model.Core;

namespace CQSAirborne.Services.Contract
{
    public interface ICPRMasterService
    {
        // List & CRUD
        IQueryable<CPRMasterModel> GetAll();                 // server-side filtering/paging ready
        Task<CPRMasterModel> GetByIdAsync(long id);          // gets header + children
        Task<bool> CreateEditAsync(CPRMasterModel model, int userId);
        Task<bool> DeleteAsync(long id, int userId);

        Task<List<SelectListModel>> GetCPRMasterStatusAsync();
        Task<List<SelectListModel>> GetCPRMasterStageAsync();
        Task<bool> UpdateStageStatusAsync(CPRStageStatusUpdateModel model, int userId);
    }
}
