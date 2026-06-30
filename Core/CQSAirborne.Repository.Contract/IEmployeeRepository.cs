using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Repository.Contract
{
    public interface IEmployeeRepository : IBaseRepository<EmployeeEntity>
    {
        EmployeeEntity GetByEmpId(string empId);
        bool BulkInsertEmployeeWithProcedure(List<AddEmployeeDataType> addEmployeeDataTypes, bool isManual);
        IQueryable<EmployeeEntity> GetAllActive();
        bool UpdateBulkEmployees(List<AddEmployeeDataType> addEmployeeDataTypes, int plantId);
        IQueryable<EmployeeEntity> GetAllEmpNoTracking();
        Task<List<EmployeeListViewModel>> GetEmployeeListWithProcedure();
    }
}
