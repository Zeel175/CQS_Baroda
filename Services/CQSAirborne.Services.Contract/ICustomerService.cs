using CQSAirborne.Model.Customer;
using CQSAirborne.Model.Employee;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface ICustomerService
    {
        IQueryable<AddEditCustomerModel> GetAll();
        bool CreateEdit(AddEditCustomerModel model, int userId);
        Task<AddEditCustomerModel> GetCustomerById(long id);

        Task<string> CreateEditCustomerDocumentAsync(CustomerDocumentMappingModel model, int userId);
        bool DeleteCustomerDocument(long id, int UserId);
        bool DeleteCustomer(long id, int UserId);
    }
}
