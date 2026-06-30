using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class RolePermissionEntity : BaseEntity
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public virtual RoleEntity RoleMaster { get; set; }
        public virtual PermissionEntity PermissionMaster { get; set; }
    }
}
