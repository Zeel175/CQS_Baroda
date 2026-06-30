using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class EmployeeRoleEntity : BaseAuditableEntity
    {
        public long Id { get; set; }
        public string RoleName { get; set; }
        public virtual ICollection<EmployeeRoleDesignationMapEntity> EmployeeRoleDesignationMaps { get; set; }
    }
}
