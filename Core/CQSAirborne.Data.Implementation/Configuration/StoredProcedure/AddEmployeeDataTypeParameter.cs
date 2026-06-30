using CQSAirborne.Data.Implementation.Utility;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;

namespace CQSAirborne.Data.Implementation.Configuration.StoredProcedure
{
    public class AddEmployeeDataTypeParameter : SqlMapper.ICustomQueryParameter
    {
        private readonly IEnumerable<AddEmployeeDataType> _rows;
        public AddEmployeeDataTypeParameter(IEnumerable<AddEmployeeDataType> rows) => _rows = rows;

        public void AddParameter(IDbCommand command, string name)
        {
            var sqlParam = new SqlParameter
            {
                ParameterName = name,
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.AddEmployeeDataType",
                Value = ToDataTable(_rows)
            };
            command.Parameters.Add(sqlParam);
        }

        private static DataTable ToDataTable(IEnumerable<AddEmployeeDataType> rows)
        {
            var dt = new DataTable();
            dt.Columns.Add("EmployeeName", typeof(string));
            dt.Columns.Add("GroupId", typeof(string));
            dt.Columns.Add("EmpId", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("Designation", typeof(string));
            dt.Columns.Add("ProgramName", typeof(string));
            dt.Columns.Add("ReportingManagerGID", typeof(string));
            dt.Columns.Add("ReportingManagerEmpID", typeof(string));
            dt.Columns.Add("ReportingManagerName", typeof(string));
            dt.Columns.Add("ReportingManagerEmailID", typeof(string));
            dt.Columns.Add("HODGID", typeof(string));
            dt.Columns.Add("HODEmpID", typeof(string));
            dt.Columns.Add("HODName", typeof(string));
            dt.Columns.Add("HODEmailID", typeof(string));
            dt.Columns.Add("OfficalEmpEmailID", typeof(string));
            dt.Columns.Add("ADID", typeof(string));
            dt.Columns.Add("Plant", typeof(string));
            dt.Columns.Add("OrgRole", typeof(string));
            dt.Columns.Add("UserName", typeof(string));
            dt.Columns.Add("Password", typeof(string));

            foreach (var r in rows)
                dt.Rows.Add(
                    r.EmployeeName, r.GroupId, r.EmpId, r.Department, r.Designation, r.ProgramName,
                    r.ReportingManagerGID, r.ReportingManagerEmpID, r.ReportingManagerName, r.ReportingManagerEmailID,
                    r.HODGID, r.HODEmpID, r.HODName, r.HODEmailID, r.OfficalEmpEmailID,
                    r.ADID, r.Plant, r.OrgRole, r.UserName, r.Password
                );
            return dt;
        }
    }
}
