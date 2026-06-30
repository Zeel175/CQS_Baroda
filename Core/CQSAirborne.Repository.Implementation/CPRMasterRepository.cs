using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.CPRMaster;
using CQSAirborne.Repository.Contract;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class CPRMasterRepository : BaseRepository<CPRMasterEntity>, ICPRMasterRepository
    {
        private readonly IStoredProcedureContext _storedProcedureContext;
        private readonly string _connString;
        public CPRMasterRepository(IDataContext dataContext
            , IStoredProcedureContext storedProcedureContext, IConfiguration configuration)
            : base(dataContext)
        {
            _storedProcedureContext = storedProcedureContext;
            _connString = configuration.GetConnectionString("DefaultConnection"); // adjust key if different
        }

        public async Task<IQueryable<CPRMasterModel>> GetAllFromSPAsync(int employeeId)
        {
            using (var connection = new SqlConnection(_connString))
            {
                await connection.OpenAsync();

                var p = new DynamicParameters();
                p.Add("@EmployeeId", employeeId, DbType.Int32);

                var data = await connection.QueryAsync<CPRMasterModel>("uspGetAllCPRMaster", p, commandType: CommandType.StoredProcedure);
                return data.AsQueryable();
            }
        }

        public async Task<IQueryable<CPRApprovalDetailsModel>> GetCPRApprovalDetailsByCPRIdAsync(long CPRId)
        {
            using (var connection = new SqlConnection(_connString))
            {
                await connection.OpenAsync();

                var p = new DynamicParameters();
                p.Add("@CPRId", CPRId, DbType.Int32);

                var data = await connection.QueryAsync<CPRApprovalDetailsModel>("uspGetCPRApprovalDetailsByCPRId", p, commandType: CommandType.StoredProcedure);
                return data.AsQueryable();
            }
        }

        /* -------- Repository method -------- */
        public async Task<CPRPrintModel> GetCprPrintByIdFromSpAsync(long cprId)
        {
            using (var connection = new SqlConnection(_connString))
            {
                await connection.OpenAsync();

                var p = new DynamicParameters();
                p.Add("@CprId", cprId, DbType.Int64);

                using (var multi = await connection.QueryMultipleAsync(
                    "dbo.uspGetCPRPrintById", p, commandType: CommandType.StoredProcedure))
                {
                    // 1) header (single row)
                    var header = await multi.ReadFirstOrDefaultAsync<CprHeaderRow>();
                    if (header == null) return null;

                    // 2) approvals (all stages in one set per your SP)
                    var allApprovals = (await multi.ReadAsync<CprApprovalRow>()).ToList();

                    var vm = new CPRPrintModel
                    {
                        Id = header.Id,
                        CPRUniqueCode = header.CPRUniqueCode,
                        CPRNo = header.CPRNo,
                        CreatedDate = header.CreatedDate,
                        RequestedDate = header.RequestedDate,
                        CategoryName = header.CategoryName,
                        DocumentNo = header.DocumentNo,
                        DepartmentTitle = header.DepartmentTitle,
                        Department = header.Department,
                        Revision = header.Revision,
                        Program = header.Program,
                        ActionRequested = header.ActionRequested,
                        CPRRaisedDueTo = header.CPRRaisedDueTo,
                        SectionRequiringChange = header.SectionRequiringChange,
                        ChangeDescription = header.ChangeDescription,
                        StatusId = header.StatusId,
                        StatusName = header.StatusName,
                        StageId = header.StageId,
                        StageName = header.StageName,
                        EDC = header.EDC,
                        ADC = header.ADC,
                        RequestedById = header.RequestedById,
                        RequestedByName = header.RequestedByName,
                        RelevantVerticalHeadNames = header.RelevantVerticalHeadNames,
                        ProcessOwnerNames = header.ProcessOwnerNames,
                        DocumentFilePath = header.DocumentFilePath,
                        ReasonForChange = header.ReasonforChange,
                        AdminStatusId = header.AdminStatusId,
                        QMSAdminStatus = header.QMSAdminStatus,
                        AdminStatusDate = header.AdminStatusDate,
                        ClosedDate = header.ClosedDate,
                        QMSAdmin = header.QMSAdmin,
                        Level1Approvals = allApprovals
                            .Where(a => a.StageId == 14)
                            .OrderBy(a => a.StatusDateTime ?? DateTime.MaxValue)
                            .ThenBy(a => a.Id)
                            .ToList(),
                        Level2Approvals = allApprovals
                            .Where(a => a.StageId == 15)
                            .OrderBy(a => a.StatusDateTime ?? DateTime.MaxValue)
                            .ThenBy(a => a.Id)
                            .ToList()
                    };

                    return vm;
                }
            }
        }

        public async Task<IQueryable<NonStandardCategoryModel>> GetAllNonStandardCategoriesAsync()
        {
            using (var connection = new SqlConnection(_connString))
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<NonStandardCategoryModel>("usp_GetNonStandardCategories", null, commandType: CommandType.StoredProcedure);
                return data.AsQueryable();
            }
        }
    }
}
