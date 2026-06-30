using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.Employee;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Data.Contract
{
    public interface IStoredProcedureContext
    {
        bool InsertBulkEmployee(List<AddEmployeeDataType> addEmployeeDataTypes, bool isManual);
        Task<List<DocumentChartModel>> GetDocumentChart(long? employeeId, int? plantId);
        Task<List<DocumentChartModel>> GetSecondaryDocumentChart(long? employeeId, int? plantId);
        Task<List<DocumentChartModel>> GetDocumentChartByCategoryId(int categoryId, long? employeeId, int? plantId);
        Task<List<DocumentListModel>> GetAllPrefixDocNumber();
        Task<List<DocumentListModel>> GetAllDocumentList();
        Task<List<DocumentListModel>> GetFilteredDocumentsForExportAsync(int statusId, string docTypePrefix, long? employeeId);
        Task<List<DocumentListModel>> GetAllDocumentViewScreenAsync(int employeeId);
        bool UpdateBulkEmployees(List<AddEmployeeDataType> addEmployeeDataTypes, int plantId);
        Task<List<EmployeeListViewModel>> GetEmployeeListForViewPage();
        Task<List<DocumentEmailDataModel>> GetAllDocumentEmailDataAsync(int employeeId, DateTime? date);
    }
}
