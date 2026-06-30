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
    public class AddEmployeeDataTypeParameter1 : SqlMapper.ICustomQueryParameter
    {
        private readonly DataTable _addEmployeeDataTypes;
        public AddEmployeeDataTypeParameter1(List<AddEmployeeDataType> addEmployeeDataTypes)
        {
            _addEmployeeDataTypes = addEmployeeDataTypes.ToDataTable();
        }

        public void AddParameter(IDbCommand command, string name)
        {
            var parameter = (SqlParameter)command.CreateParameter();

            parameter.ParameterName = "Employees";
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.Value = _addEmployeeDataTypes;
            parameter.TypeName = "AddEmployeeDataType";

            command.Parameters.Add(parameter);
        }
    }
}
