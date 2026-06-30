using CQSAirborne.Model.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IEmployeeService
    {
        BulkUploadResponseModel BulkUpdate(List<AddEditEmployeeViewModel> addEditEmployeeViewModels);
        bool BulkUpdateWithProcedure(List<AddEditEmployeeViewModel> addEditEmployeeViewModels, bool isManual);
        IQueryable<EmployeeListViewModel> GetAll();
        bool Insert(AddEditEmployeeViewModel addEditEmployeeViewModel);
        AddEditEmployeeViewModel GetById(long id);
        bool Update(AddEditEmployeeViewModel addEditEmployeeViewModel);
        IQueryable<EmployeeListViewModel> GetAllActive();
        Task<bool> ChangePassword(string Password, string UserName);
        Task<bool> ResetPassword(ResetPasswordViewModel model);
        bool DeleteEmployee(long id);
        bool ChangeStatus(int id, bool status);
        Task<List<EmployeeListViewModel>> GetEmployeeListForViewPageAsync();
    }
}
