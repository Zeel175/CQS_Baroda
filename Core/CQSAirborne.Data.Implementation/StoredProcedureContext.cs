using CQSAirborne.Data.Contract;
using CQSAirborne.Data.Implementation.Configuration.StoredProcedure;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Document;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using CQSAirborne.Model.Employee;

namespace CQSAirborne.Data.Implementation
{
    public class StoredProcedureContext : IStoredProcedureContext
    {
        private readonly string _connectionString;
        public StoredProcedureContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool InsertBulkEmployee(List<AddEmployeeDataType> addEmployeeDataTypes, bool isManual)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var affectedRows = connection.Execute("adm_AddEmployee", new
                {
                    Employees = new AddEmployeeDataTypeParameter(addEmployeeDataTypes),
                    IsManualUpload = isManual
                }
                , commandType: CommandType.StoredProcedure);

                return affectedRows > 0;
            }
        }
        
        public bool UpdateBulkEmployees(List<AddEmployeeDataType> addEmployeeDataTypes, int plantId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var affectedRows = connection.Execute("adm_UpdateEmployeePlants", new
                {
                    Employees = new AddEmployeeDataTypeParameter(addEmployeeDataTypes),
                    NewPlantId = plantId
                }
                , commandType: CommandType.StoredProcedure);

                return affectedRows > 0;
            }
        }

        public async Task<List<DocumentChartModel>> GetDocumentChart(long? employeeId, int? plantId)
        {
            List<DocumentChartModel> document = new List<DocumentChartModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<DocumentChartModel>("adm_GetPrimaryCategorywiseDocumentCount", new
                {
                    PlantId = plantId,
                    EmployeeId = employeeId
                }, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
        }

        public async Task<List<DocumentChartModel>> GetSecondaryDocumentChart(long? employeeId, int? plantId)
        {
            List<DocumentChartModel> document = new List<DocumentChartModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<DocumentChartModel>("adm_GetSecondaryCategorywiseDocumentCount", new
                {
                    PlantId = plantId,
                    EmployeeId = employeeId
                }, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
        }
        public async Task<List<DocumentChartModel>> GetDocumentChartByCategoryId(int categoryId, long? employeeId, int? plantId)
        {
            List<DocumentChartModel> document = new List<DocumentChartModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<DocumentChartModel>("adm_GetDocumentCountByCategoryId", new
                {
                    PlantId = plantId,
                    CategoryId = categoryId,
                    EmployeeId = employeeId,
                }, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
        }

        public async Task<List<DocumentListModel>> GetAllPrefixDocNumber()
        {
            List<DocumentListModel> document = new List<DocumentListModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<DocumentListModel>("adm_GetAllPrefixDocNumber", null, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
        }
        public async Task<List<DocumentListModel>> GetAllDocumentList()
        {
            List<DocumentListModel> document = new List<DocumentListModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var data = await connection.QueryAsync<DocumentListModel>("adm_GetAllDocumentList", null, commandType: CommandType.StoredProcedure);
                return data.ToList();
            }
        }

        public async Task<List<DocumentListModel>> GetFilteredDocumentsForExportAsync(int statusId, string docTypePrefix, long? employeeId)
        {
            var documentDictionary = new Dictionary<string, DocumentListModel>(); // use composite key

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var result = await connection.QueryAsync<DocumentListModel, Model.Dashboard.PlantDocumentListModel, DocumentListModel>(
                    "usp_GetFilteredDocumentsForExport",
                    (doc, plant) =>
                    {
                        string compositeKey = $"{doc.Id}_{doc.Source}_{doc.RevisionNumber}";

                        if (!documentDictionary.TryGetValue(compositeKey, out var documentEntry))
                        {
                            documentEntry = doc;
                            documentEntry.Plants = new List<Model.Dashboard.PlantDocumentListModel>();
                            documentDictionary.Add(compositeKey, documentEntry);
                        }

                        if (plant != null && plant.PlantId != 0)
                        {
                            documentEntry.Plants.Add(new Model.Dashboard.PlantDocumentListModel
                            {
                                Id = (plant.PlantTableId != null && plant.PlantTableId.HasValue && plant.PlantTableId > 0 ? plant.PlantTableId.Value : doc.Id),
                                PlantId = plant.PlantId,
                                PlantName = plant.PlantName,
                                PlantAlias = plant.PlantAlias,
                                DisplayOrder = plant.DisplayOrder,
                                CanDownload = doc.CanDownload,
                                DocumentDisplayName = doc.Alias
                            });
                        }

                        return documentEntry;
                    },
                    new
                    {
                        StatusId = statusId,
                        DocTypePrefix = docTypePrefix,
                        EmployeeId = employeeId
                    },
                    splitOn: "PlantId", // first column of second model
                    commandType: CommandType.StoredProcedure
                );
            }

            return documentDictionary.Values.ToList();
        }


        public async Task<List<DocumentListModel>> GetAllDocumentViewScreenAsync(int employeeId)
        {
            var documentDictionary = new Dictionary<string, DocumentListModel>(); // use composite key

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var result = await connection.QueryAsync<DocumentListModel, Model.Dashboard.PlantDocumentListModel, DocumentListModel>(
                    "usp_GetAllDocumentData",
                    (doc, plant) =>
                    {
                        string compositeKey = $"{doc.Id}_{doc.Source}_{doc.RevisionNumber}";

                        if (!documentDictionary.TryGetValue(compositeKey, out var documentEntry))
                        {
                            documentEntry = doc;
                            documentEntry.Plants = new List<Model.Dashboard.PlantDocumentListModel>();
                            documentDictionary.Add(compositeKey, documentEntry);
                        }

                        if (plant != null && plant.PlantId != 0)
                        {
                            documentEntry.Plants.Add(new Model.Dashboard.PlantDocumentListModel
                            {
                                Id = (plant.PlantTableId != null && plant.PlantTableId.HasValue && plant.PlantTableId > 0 ? plant.PlantTableId.Value : doc.Id),
                                PlantId = plant.PlantId,
                                PlantName = plant.PlantName,
                                PlantAlias = plant.PlantAlias,
                                DisplayOrder = plant.DisplayOrder,
                                CanDownload = doc.CanDownload,
                                DocumentDisplayName = doc.Alias
                            });
                        }

                        return documentEntry;
                    },
                    new
                    {
                        EmployeeId = employeeId
                    },
                    splitOn: "PlantId", // first column of second model
                    commandType: CommandType.StoredProcedure
                );
            }

            return documentDictionary.Values.ToList();
        }

        public async Task<List<EmployeeListViewModel>> GetEmployeeListForViewPage()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var data = await connection.QueryAsync<EmployeeListViewModel>(
                    "adm_GetEmployeeListForViewPage",
                    null,
                    commandType: CommandType.StoredProcedure
                );

                return data.ToList();
            }
        }
        public async Task<List<DocumentEmailDataModel>> GetAllDocumentEmailDataAsync(int employeeId, DateTime? date)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var data = await connection.QueryAsync<DocumentEmailDataModel>(
                    "usp_GetAllDocumentEmailData",
                    new
                    {
                        EmployeeId = employeeId,
                        Date = date
                    },
                    commandType: CommandType.StoredProcedure
                );

                return data.ToList();
            }
        }
    }
}
