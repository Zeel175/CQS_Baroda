using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Role
{
    public class AssignPermissionViewModel : BaseModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsList { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAdmin { get; set; }
    }
}
