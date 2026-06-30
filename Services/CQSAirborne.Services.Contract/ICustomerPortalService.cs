using CQSAirborne.Model.Customer;
using CQSAirborne.Model.Employee;
using CQSAirborne.Web.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface ICustomerPortalService
    {
        Task<bool> UploadFile(CustomerFileModel model, MemoryStream memory);
    }
}
