using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace CQSAirborne.Domain
{
    public class RoleEntity : BaseAuditableEntity
    {
        public RoleEntity()
        {
            this.RolePermissions = new List<RolePermissionEntity>();
        }
        public int Id { get; set; }
        public string RoleName { get; set; }
        public bool? IsActive { get; set; }
        public virtual ICollection<UserEntity> UserEntities { get; set; }
        public virtual ICollection<RolePermissionEntity> RolePermissions { get; set; }
    }
}
