using CQSAirborne.Model.Core;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.ErrorLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IErrorLogService
    {
        void Insert(AddEditErrorLogModel addEditErrorLogModel);
    }
}
