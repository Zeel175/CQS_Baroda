using CQSAirborne.Domain;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IDocumentService
    {
        IQueryable<DocumentListModel> GetAll();
        Task<string> GetNextDocumentCodeByPlantAsync(int plantId);
        IQueryable<DocumentListModel> GetHistoryByDocumentId(int id);
        IQueryable<DocumentListModel> GetAllDocumentHistory();
        Task<AddEditDocumentModel> GetCreateModelAsync();
        UploadResponse SavePicture(AddEditPictureModel addEditPictureModel, int userId);
        Task<AddEditDocumentModel> Insert(AddEditDocumentModel addEditDocumentModel, int userId);
        bool IsDocumentNumberUnique(int id, string model);
        AddEditDocumentModel GetById(int id);
        AddEditDocumentModel GetDocumentHistoryById(int id);
        bool Update(AddEditDocumentModel addEditDocumentModel, int userId);
        (bool, string) EmailDocument(SendEmailModel sem);

        Task<AddEditDocumentModel> GetByIdAsync(int id);
        Task<List<SelectListModel>> GetAllDocumentTypeAsync();
        List<PlantSelectListModel> GetDocumentAllPlants();
        List<PlantSelectListModel> GetDocumentPlants(int id);
        List<PlantDocumentListModel> AssignAllActivePlant(int id, bool isRestrected=false);
        Dictionary<int, List<PlantDocumentListModel>> AssignAllActivePlants(List<int> docIds, bool isRestricted = false);
        List<PlantDocumentListModel> AssignAllActivePlantForHistory(int id);
        bool ChangeStatus(int id, bool status, bool isNotify);
        void Delete(int id);

        Task<List<DocumentListModel>> GetAllPrefixDocNumber();
        Task<List<DocumentListModel>> GetAllDocumentList();
        bool InsertDocumentEmailData(DocumentEmailDataModel Model, int userId);

        IQueryable<DocumentEmailDataModel> GetPendingEmailDocuments();
        bool SendPendingEmail(string ids, string title, int userId);
        Task<List<DocumentListModel>> GetFilteredDocumentsForExportAsync(int statusId, string docTypePrefix, long? employeeId);
        Task<List<DocumentListModel>> GetAllDocumentViewScreenAsync(int employeeId);
        IQueryable<DocumentEmailDataModel> GetPendingEmailDocumentsBySP(int employeeid, DateTime? date);
        List<EmailPreviewModel> GetEmailPreview(string ids, string title, int userId);
    }
}
