using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Employee;
using CQSAirborne.Repository.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Implementation
{
    public class EmployeeRepository : BaseRepository<EmployeeEntity>, IEmployeeRepository
    {
        private readonly IStoredProcedureContext _storedProcedureContext;
        public EmployeeRepository(IDataContext dataContext
            , IStoredProcedureContext storedProcedureContext)
            : base(dataContext)
        {
            _storedProcedureContext = storedProcedureContext;
        }

        public bool BulkInsertEmployeeWithProcedure(List<AddEmployeeDataType> addEmployeeDataTypes, bool isManual)
        {
            return _storedProcedureContext.InsertBulkEmployee(addEmployeeDataTypes, isManual);
        }

        public bool UpdateBulkEmployees(List<AddEmployeeDataType> addEmployeeDataTypes, int plantId)
        {
            return _storedProcedureContext.UpdateBulkEmployees(addEmployeeDataTypes, plantId);
        }

        public IQueryable<EmployeeEntity> GetAllActive()
        {
            return GetAllNoTracking().Where(w => w.IsActive);
        }
        public IQueryable<EmployeeEntity> GetAllEmpNoTracking()
        {
            return GetAll().AsNoTracking();
        }

        public EmployeeEntity GetByEmpId(string empId)
        {
            return GetAll().Where(w => w.EmpId == empId).FirstOrDefault();
        }

        public override EmployeeEntity GetById(object id)
        {
            long employeeId = Convert.ToInt64(id);
            return GetAll().FirstOrDefault(w => w.Id == employeeId);
        }
        public async Task<List<EmployeeListViewModel>> GetEmployeeListWithProcedure()
        {
            var data = await _storedProcedureContext.GetEmployeeListForViewPage();
            return data;
        }
    }
}
