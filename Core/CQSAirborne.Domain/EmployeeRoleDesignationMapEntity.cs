using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain
{
    public class EmployeeRoleDesignationMapEntity : BaseAuditableEntity
    {
        public long Id { get; set; }
        public string Designation { get; set; }
        public string EmployeeRoleId { get; set; }
        public virtual EmployeeRoleEntity EmployeeRole { get; set; }
    }
}
