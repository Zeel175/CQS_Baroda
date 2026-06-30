using CQSAirborne.Model.Employee;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Utils
{
    public static class ExcelExtensions
    {
        public static List<AddEditEmployeeViewModel> ToEmployeeData(this ExcelWorksheet worksheet)
        {
            Func<CustomAttributeData, bool> columnOnly = y => y.AttributeType == typeof(CustomExcelColumn);

            var columns = typeof(AddEditEmployeeViewModel)
                    .GetProperties()
                    .Where(x => x.CustomAttributes.Any(columnOnly))
            .Select(p => new
            {
                Property = p,
                Column = p.GetCustomAttributes<CustomExcelColumn>().First().ColumnIndex //safe because if where above
            }).ToList();

            var rows = worksheet.Cells
                .Select(cell => cell.Start.Row)
                .Distinct()
                .OrderBy(x => x);


            //Create the collection container
            var collection = rows.Skip(1)
                .Select(row =>
                {
                    var tnew = new AddEditEmployeeViewModel();
                    columns.ForEach(col =>
                    {
                        //This is the real wrinkle to using reflection - Excel stores all numbers as double including int
                        var val = worksheet.Cells[row, col.Column];
                        //Its a string
                        col.Property.SetValue(tnew, val.GetValue<string>());
                    });

                    tnew.IsManual = true;

                    return tnew;
                });


            //Send it back
            return collection.ToList();
        }

        public static string[] GetHeaderColumns(this ExcelWorksheet sheet)
        {
            List<string> columnNames = new List<string>();
            foreach (var firstRowCell in sheet.Cells[sheet.Dimension.Start.Row, sheet.Dimension.Start.Column, 1, sheet.Dimension.End.Column])
                columnNames.Add(firstRowCell.Text);
            return columnNames.ToArray();
        }
    }
}
