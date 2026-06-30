using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CQSAirborne.Model.Role
{
    public class AddEditRoleModel
    {
        public AddEditRoleModel()
        {
            this.Permissions = new List<AssignPermissionViewModel>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage ="Please Insert Role Name")]
        public string RoleName { get; set; }
        public bool? IsActive { get; set; }
        public string PermissionData { get; set; }

        public List<AssignPermissionViewModel> Permissions { get; set; }
    }
}
