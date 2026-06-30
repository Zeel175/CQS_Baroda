using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Domain.StoredProcedure.TableTypes
{
    public class AddEmployeeDataType
    {
        public string EmployeeName { get; set; }
        public string GroupId { get; set; }
        public string EmpId { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string ProgramName { get; set; }
        public string ReportingManagerGID { get; set; }
        public string ReportingManagerEmpID { get; set; }
        public string ReportingManagerName { get; set; }
        public string ReportingManagerEmailID { get; set; }
        public string HODGID { get; set; }
        public string HODEmpID { get; set; }
        public string HODName { get; set; }
        public string HODEmailID { get; set; }
        public string OfficalEmpEmailID { get; set; }
        public string ADID { get; set; }
        public string Plant { get; set; }

        public string OrgRole { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
