using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.CPRMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface ICPRMasterRepository : IBaseRepository<CPRMasterEntity>
    {
        Task<IQueryable<CPRMasterModel>> GetAllFromSPAsync(int employeeId);
        Task<CPRPrintModel> GetCprPrintByIdFromSpAsync(long cprId);
        Task<IQueryable<NonStandardCategoryModel>> GetAllNonStandardCategoriesAsync();
        Task<IQueryable<CPRApprovalDetailsModel>> GetCPRApprovalDetailsByCPRIdAsync(long CPRId);
    }
}
