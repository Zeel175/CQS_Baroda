using CQSAirborne.Model.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Customer
{
    public class CustomerFileModel : BaseValidateModel
    {
        public CustomerFileModel()
        {
        }
       
        public IFormFile File { get; set; }
        public string Path { get; set; }
    }
}
