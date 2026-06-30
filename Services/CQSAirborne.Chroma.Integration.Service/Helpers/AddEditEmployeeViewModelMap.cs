using CQSAirborne.Chroma.Integration.Service.Models;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Chroma.Integration.Service.Helpers
{
    public class AddEditEmployeeViewModelMap : ClassMap<AddEditEmployeeViewModel>
    {
        public AddEditEmployeeViewModelMap()
        {
            Map(w => w.EmployeeName).Name("Employee Name").Index(0);
            Map(w => w.GroupId).Name("Group ID").Index(1);
            Map(w => w.EmpId).Name("Emp ID").Index(2);
            Map(w => w.Department).Name("Department").Index(3);
            Map(w => w.Designation).Name("Designation").Index(4);
            //Map(w => w.ProgramName).Name("Program Name").Index(5);
            Map(w => w.ReportingManagerGID).Name("Reporting Manager GID").Index(6);
            Map(w => w.ReportingManagerEmpID).Name("Reporting Manager Emp ID").Index(7);
            Map(w => w.ReportingManagerName).Name("Reporting Manager Name").Index(8);
            Map(w => w.ReportingManagerEmailID).Name("Reporting Manager Email ID").Index(9);
            Map(w => w.HODGID).Name("HOD GID").Index(10);
            Map(w => w.HODEmpID).Name("HOD Emp ID").Index(11);
            Map(w => w.HODName).Name("HOD Name").Index(12);
            Map(w => w.HODEmailID).Name("HOD Email ID").Index(13);
            Map(w => w.OfficalEmpEmailID).Name("Offical Emp Email ID").Index(14);
            Map(w => w.ADID).Name("AD ID").Index(15);
            Map(w => w.Plant).Name("Plant").Index(16);
        }
    }
}
