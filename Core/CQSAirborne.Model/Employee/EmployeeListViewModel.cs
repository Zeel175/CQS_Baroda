using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Employee
{
    public class EmployeeListViewModel : BaseValidateModel
    {
        public long Id { get; set; }
        public string EmployeeName { get; set; }
        public string GroupId { get; set; }
        public string EmpId { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public bool IsActive { get; set; }
        public string ADID { get; set; }
        public string OrgRole { get; set; }
        public int? HitCount { get; set; }
        public int? PlantId { get; set; }
        public string PlantName { get; set; } 
        public string PlantIds { get; set; }   // ✅ Multiselect
        public string PlantAlias { get; set; } // ✅ Show Alias instead of Name
        public DateTime? LastHitDate { get; set; }
        public DateTime? SuspendDateTime { get; set; }
    }
}
