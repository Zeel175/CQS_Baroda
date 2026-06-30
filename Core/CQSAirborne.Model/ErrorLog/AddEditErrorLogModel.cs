using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.ErrorLog
{
    public class AddEditErrorLogModel : BaseModel
    {
        //public long Id { get; set; }
        public string ApplicationName { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        //public DateTime CreatedOn { get; set; }
    }
}
