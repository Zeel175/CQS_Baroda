using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Role
{
    public class AddEditRolePermissionModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }
}
