using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Core
{
    public class ValidationResultModel : BaseModel
    {
        public ValidationResultModel()
        {
            Errors = new List<string>();
        }
        public string FieldName { get; set; }
        public List<string> Errors { get; set; }
    }
}
