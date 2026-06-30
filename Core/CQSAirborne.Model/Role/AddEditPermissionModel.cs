using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Role
{
    public class AddEditPermissionModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public int PermissionTypeId { get; set; }
        public int DisplayOrder { get; set; }
        public string Controller { get; set; }
        public string ActionName { get; set; }
    }
}
