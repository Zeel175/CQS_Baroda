using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.Web;
using Microsoft.AspNetCore;


namespace CQSAirborne.Model.Employee
{
    public class CustomExcelColumn : Attribute
    {
        public int ColumnIndex { get; }
        public CustomExcelColumn(int index)
        {
            ColumnIndex = index;
        }
    }

    public class AddEditEmployeeViewModel : BaseValidateModel
    {
        public long Id { get; set; }

        [CustomExcelColumn(1)]
        public string EmployeeName { get; set; }

        [CustomExcelColumn(2)]
        public string GroupId { get; set; }

        [CustomExcelColumn(3)]
        public string EmpId { get; set; }

        [CustomExcelColumn(4)]
        public string Department { get; set; }

        [CustomExcelColumn(5)]
        public string Designation { get; set; }

        [CustomExcelColumn(6)]
        public string ReportingManagerGID { get; set; }

        [CustomExcelColumn(7)]
        public string ReportingManagerEmpID { get; set; }

        [CustomExcelColumn(8)]
        public string ReportingManagerName { get; set; }

        [CustomExcelColumn(9)]
        public string ReportingManagerEmailID { get; set; }

        [CustomExcelColumn(10)]
        public string HODGID { get; set; }

        [CustomExcelColumn(11)]
        public string HODEmpID { get; set; }

        [CustomExcelColumn(12)]
        public string HODName { get; set; }

        [CustomExcelColumn(13)]
        public string HODEmailID { get; set; }

        [CustomExcelColumn(14)]
        public string OfficalEmpEmailID { get; set; }

        [CustomExcelColumn(15)]
        public string ADID { get; set; }

        [CustomExcelColumn(16)]
        public string Plant { get; set; }
        [CustomExcelColumn(17)]
        public string PlantAccess { get; set; }

        public bool IsManual { get; set; }

        //[Remote("IsUserEmailAvailable", "Account", AdditionalFields="Id", ErrorMessage ="This UserName is not available.")]
        //[Remote(action: "IsUserEmailAvailable", controller: "Account", AdditionalFields = "Id", ErrorMessage = "This UserName is not available.")]
        [CustomExcelColumn(18)]
        public string UserName { get; set; }
        [CustomExcelColumn(19)]
        public string Password { get; set; }
        [CustomExcelColumn(20)]
        public string OrgRole { get; set; }
        [DisplayName("Plant")]
        public int? PlantId { get; set; }
        public int? HitCount { get; set; }
        public string PlantIds { get; set; }
        public string[] PlantIdList { get; set; }
        public DateTime? LastHitDate { get; set; }
        public DateTime? SuspendDateTime { get; set; }
    }
}
public class ResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}
public class BulkUploadResponseModel
{
    public bool IsSuccess { get; set; }

    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }

    public List<string> UpdatedEmpIds { get; set; }
    public List<string> CreatedEmpIds { get; set; }

    public string Message { get; set; }
}
