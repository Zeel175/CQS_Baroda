using CQSAirborne.Domain;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using CQSAirborne.Services.Implementation.Utils;
using CQSAirborne.Services.Implementation.ExtensionMethods;
using CQSAirborne.Data.Contract;
using Serilog;
using Microsoft.Extensions.Options;
namespace CQSAirborne.Services.Implementation
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDataMapper _dataMapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICodeMaintainRepository _codeMaintainRepository;
        private readonly IPictureRepository _pictureRepository;
        private readonly IDocumentPlantRepository _documentPlantRepository;
        private readonly IGroupCodesRepository _groupCodesRepository;
        private readonly IPlantRepository _plantRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDashboardService _dashboardService;
        private readonly EmailHelper _emailHelper;
        private readonly IDocumentTagsRepository _documentTagsRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDocumentEmailDataRepository _documentEmailDataRepository;
        private readonly IStoredProcedureContext _storedProcedureContext;
        private readonly SpecialPlantEmailConfig _specialPlantConfig;

        public DocumentService(IDocumentRepository documentRepository
            , IDataMapper dataMapper
            , IUnitOfWork unitOfWork
            , ICodeMaintainRepository codeMaintainRepository
            , IPictureRepository pictureRepository
            , IDocumentPlantRepository documentPlantRepository
             , IDocumentTagsRepository documentTagsRepository
            , IGroupCodesRepository groupCodesRepository
            , IPlantRepository plantRepository
            , IEmployeeRepository employeeRepository
            , IHostingEnvironment hostingEnvironment
            , IDashboardService dashboardService
            , EmailHelper emailHelper,
            ICategoryRepository categoryRepository,
            IDocumentEmailDataRepository documentEmailDataRepository,
            IStoredProcedureContext storedProcedureContext,
            IOptions<SpecialPlantEmailConfig> config
            )
        {
            _documentRepository = documentRepository;
            _dataMapper = dataMapper;
            _unitOfWork = unitOfWork;
            _codeMaintainRepository = codeMaintainRepository;
            _pictureRepository = pictureRepository;
            _documentPlantRepository = documentPlantRepository;
            _groupCodesRepository = groupCodesRepository;
            _plantRepository = plantRepository;
            _employeeRepository = employeeRepository;
            _hostingEnvironment = hostingEnvironment;
            _dashboardService = dashboardService;
            _emailHelper = emailHelper;
            _documentTagsRepository = documentTagsRepository;
            _categoryRepository = categoryRepository;
            _documentEmailDataRepository = documentEmailDataRepository;
            _storedProcedureContext = storedProcedureContext;
            _specialPlantConfig = config.Value;
        }

        public IQueryable<DocumentListModel> GetAll()
        {
            return _dataMapper.Project<DocumentEntity, DocumentListModel>
                (_documentRepository.GetAllNoTracking());
        }


        public async Task<AddEditDocumentModel> GetCreateModelAsync()
        {
            return new AddEditDocumentModel
            {
                Code = await _codeMaintainRepository.GetNextNumberAsync(Constants.CodeModule.Document),
                UploadDate = DateTime.Now
            };
        }

        public async Task<string> GetNextDocumentCodeByPlantAsync(int plantId)
        {
            // Get Plant Code (C295, TASL, etc.)
            var plantCode = _plantRepository.GetAll()
                .Where(p => p.Id == plantId)
                .Select(p => p.Alias)   // adjust column name if needed
                .FirstOrDefault();

            if (string.IsNullOrEmpty(plantCode))
                throw new Exception("Plant code not configured");

            var prefix = $"DOC-{plantCode}-";

            // Get last document for this plant
            var lastCode = _documentRepository.GetAllNoTracking()
                .Where(d => d.Code.StartsWith(prefix))
                .OrderByDescending(d => d.Code)
                .Select(d => d.Code)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastCode))
            {
                var numericPart = lastCode.Replace(prefix, "");
                int.TryParse(numericPart, out nextNumber);
                nextNumber++;
            }

            return $"{prefix}{nextNumber:D5}";
        }
        public async Task<AddEditDocumentModel> Insert(AddEditDocumentModel addEditDocumentModel, int userId)
        {
            var entity = _dataMapper.Map<AddEditDocumentModel, DocumentEntity>(addEditDocumentModel);
            entity.CreatedBy = userId;
            entity.CreatedOn = DateTime.Now;
            entity.ModifiedOn = DateTime.Now;
            entity.IsActive = true;

            if (entity.ProcessOwner == null)
            {
                entity.ProcessOwner = "";
            }

            if (_groupCodesRepository.IsGlobalDocument(addEditDocumentModel.DocumentTypeId))
            {
                entity.CommonPictureId = addEditDocumentModel.Uploads.FirstOrDefault().PictureId;
            }
            else
            {
                foreach (var upload in addEditDocumentModel.Uploads)
                {
                    foreach (var plant in upload.Plants)
                    {
                        entity.DocumentPlantMaps.Add(new DocumentPlantMapEntity
                        {
                            PictureId = upload.PictureId,
                            PlantId = plant.Id
                        });
                    }
                }
            }

            bool NewCodeGenerated = false;
            if (entity.DocumentTypeId == 3 && entity.DocumentPlantMaps != null && entity.DocumentPlantMaps.Count() > 0 && entity.DocumentPlantMaps.Count() == 1)
            {
                int plantId = entity.DocumentPlantMaps?.FirstOrDefault()?.PlantId ?? 0;
                entity.Code = await GetNextDocumentCodeByPlantAsync(plantId);
                NewCodeGenerated = true;
            }

            if (addEditDocumentModel.Tags != null)
            {
                foreach (var tag in addEditDocumentModel.Tags.Split(","))
                {
                    entity.DocumentTags.Add(new DocumentTagsEntity
                    {
                        DocumentTag = tag
                    });
                }
            }
            _documentRepository.Insert(entity);
            if (NewCodeGenerated == false)
            {
                _codeMaintainRepository.UpdateLastNumer(Constants.CodeModule.Document);
            }
            _unitOfWork.Commit();
            addEditDocumentModel.Id = entity.Id;
            return addEditDocumentModel;
        }

        public AddEditDocumentModel GetById(int id)
        {
            //var documentEntity = _documentRepository.GetById(id);
            var documentEntity = _documentRepository.GetByIdWithIncludes(id);
            if (documentEntity == null)
                return null;

            var result = _dataMapper.Map<DocumentEntity, AddEditDocumentModel>(documentEntity);
            result.Tags = string.Join(",", documentEntity.DocumentTags.Select(s => s.DocumentTag).ToList());
            if (documentEntity.DocumentType.Code == Constants.DocumentType.Global)
            {
                result.Uploads = new List<AddEditDocumentPictureModel>
                {
                    new AddEditDocumentPictureModel
                    {
                        DisplayName = documentEntity.CommonPicture.DisplayName,
                        PictureId   = documentEntity.CommonPictureId ?? 0,
                        //Plants = _plantRepository.GetAllNoTracking().Select(w=>new SelectListModel{ Id = w.Id, Name = w.Name }).ToList()
                    }
                };
            }
            else
            {
                result.Uploads = _documentPlantRepository.GetByDocumentId(id).GroupBy(w => new { w.PictureId, w.Picture.DisplayName }).Select(s => new AddEditDocumentPictureModel
                {
                    PictureId = s.Key.PictureId,
                    DisplayName = s.Key.DisplayName,
                    Plants = s.Select(w => new SelectListModel { Id = w.Plant.Id, Name = w.Plant.Name }).ToList()
                }).ToList();
            }
            return result;
        }

        public AddEditDocumentModel GetDocumentHistoryById(int id)
        {
            var documentEntity = _documentRepository.GetHistoryByIdWithInclude(id);//GetHistoryById(id);
            if (documentEntity == null)
                return null;

            var result = _dataMapper.Map<DocumentHistoryEntity, AddEditDocumentModel>(documentEntity);
            result.Tags = "";//string.Join(",", documentEntity.DocumentTags.Select(s => s.DocumentTag).ToList());
            if (documentEntity.DocumentType.Code == Constants.DocumentType.Global)
            {
                result.Uploads = new List<AddEditDocumentPictureModel>
                {
                    new AddEditDocumentPictureModel
                    {
                        DisplayName = documentEntity.CommonPicture.DisplayName,
                        PictureId   = documentEntity.CommonPictureId ?? 0,
                        //Plants = _plantRepository.GetAllNoTracking().Select(w=>new SelectListModel{ Id = w.Id, Name = w.Name }).ToList()
                    }
                };
            }
            else
            {
                result.Uploads = _documentPlantRepository.GetPlantMapHistoryById(id).GroupBy(w => new { w.PictureId, w.Picture.DisplayName }).Select(s => new AddEditDocumentPictureModel
                {
                    PictureId = s.Key.PictureId,
                    DisplayName = s.Key.DisplayName,
                    Plants = s.Select(w => new SelectListModel { Id = w.Plant.Id, Name = w.Plant.Name }).ToList()
                }).ToList();
            }
            return result;
        }

        public async Task<AddEditDocumentModel> GetByIdAsync(int id)
        {
            var documentEntity = await _documentRepository.FirstOrDefaultAsync(w => w.Id == id);
            if (documentEntity == null)
                return null;

            return _dataMapper.Map<DocumentEntity, AddEditDocumentModel>(documentEntity);
        }

        public (bool, string) EmailDocument(SendEmailModel sem)
        {
            var entity = _documentRepository.GetById(sem.id);
            var employees = _employeeRepository.GetAllActive().Where(m => !string.IsNullOrEmpty(m.OfficalEmpEmailID));
            var location = _dashboardService.GetClickablePathForCategory(entity.CategoryId);
            var locationstring = string.Empty;
            if (location != null)
                locationstring = string.Join(" > ", location.Select(m => m.Name).ToArray());
            if (!_groupCodesRepository.IsGlobalDocument(entity.DocumentTypeId))
            {
                var plants = entity.DocumentPlantMaps.Select(m => m.Plant).Select(m => m.Name).ToList();
                employees = employees.Where(m => plants.Contains(m.Plant));
            }
            string exc = "";
            var employeesList = employees.Select(m => m.OfficalEmpEmailID).ToEmailList();
            if (!string.IsNullOrEmpty(sem.SpecificPersonEmail))
            {
                employeesList.AddRange(sem.SpecificPersonEmail.StringToEmailList(','));
            }

            string Subject = $"COSMOS || {entity.Name} is {sem.DocumentType}";

            string basePath = _hostingEnvironment.ContentRootPath;
            var filePath = "EmailTemplate/EmailDocument.template";
            string path = Path.Combine(basePath, filePath);
            var template = File.ReadAllText(path).ToString();
            template = template.Replace("#EmployeeName#", "Dear Employee")
                .Replace("#DocuNo#", entity.DocumentNumber)
                .Replace("#DocuName#", entity.Name)
                .Replace("#Status#", sem.DocumentType)
                .Replace("#RevisionNo#", entity.RevisionNumber)
                .Replace("#Location#", locationstring);
            var bccemails = new List<string>();
            if (employeesList.Count > 0)
            {
                bccemails = employeesList.ToList();
            }
            _emailHelper.SendEmail(string.Empty, Subject, template, bccemails);

            return (true, string.Join(",", bccemails));
        }

        public bool Update(AddEditDocumentModel addEditDocumentModel, int userId)
        {
            var entity = _documentRepository.GetByIdWithIncludesForUpdate(addEditDocumentModel.Id);
            //var entity = _documentRepository.GetById(addEditDocumentModel.Id);
            if (entity == null)
                return false;

            if (addEditDocumentModel.ProcessOwner == null)
            {
                addEditDocumentModel.ProcessOwner = "";
            }

            addEditDocumentModel.Code = entity.Code;
            if (addEditDocumentModel.OldRevId == null || addEditDocumentModel.OldRevId == 0)
            {
                string oldRevisionNumber = entity.RevisionNumber;
                if (oldRevisionNumber != addEditDocumentModel.RevisionNumber)
                {
                    var documentHistory = _dataMapper.Map<DocumentEntity, DocumentHistoryEntity>(entity);
                    documentHistory.InActiveDate = addEditDocumentModel.InActiveDate;
                    documentHistory.ActualInActiveDate = DateTime.Now;
                    _documentRepository.AddDocumentHistory(documentHistory);
                }

                _dataMapper.Map(addEditDocumentModel, entity);
                entity.ModifiedOn = DateTime.Now;

                // Remove plants that are already there in database
                var plantsToRemove = entity.DocumentPlantMaps.ToList();
                foreach (var plant in plantsToRemove)
                {
                    entity.DocumentPlantMaps.Remove(plant);
                    _documentPlantRepository.Delete(plant);
                }
                entity.CommonPictureId = null;

                // Remove tags that are already there in database
                var tagsToRemove = entity.DocumentTags.ToList();
                foreach (var tag in tagsToRemove)
                {
                    entity.DocumentTags.Remove(tag);
                    _documentTagsRepository.Delete(tag);
                }

                if (addEditDocumentModel.Tags != null)
                {
                    foreach (var tag in addEditDocumentModel.Tags.Split(","))
                    {
                        entity.DocumentTags.Add(new DocumentTagsEntity
                        {
                            DocumentTag = tag
                        });
                    }
                }

                // Add the new plants for the document
                if (_groupCodesRepository.IsGlobalDocument(addEditDocumentModel.DocumentTypeId))
                {
                    entity.CommonPictureId = addEditDocumentModel.Uploads.FirstOrDefault().PictureId;
                }
                else
                {
                    foreach (var upload in addEditDocumentModel.Uploads)
                    {
                        foreach (var plant in upload.Plants)
                        {
                            entity.DocumentPlantMaps.Add(new DocumentPlantMapEntity
                            {
                                PictureId = upload.PictureId,
                                PlantId = plant.Id
                            });
                        }
                    }
                }

                // Save document changes
                if (entity.ProcessOwner == null)
                {
                    entity.ProcessOwner = "";
                }
                entity.ModifiedBy = userId;
                entity.ModifiedOn = DateTime.Now;
                _documentRepository.Update(entity);
            }
            else
            {
                var historyEntity = _documentRepository.GetHistoryById(addEditDocumentModel.OldRevId);
                if (historyEntity == null)
                    return false;

                historyEntity.Name = addEditDocumentModel.Name;
                historyEntity.UploadDate = addEditDocumentModel.UploadDate;
                historyEntity.InActiveDate = addEditDocumentModel.InActiveDate;
                historyEntity.ActualInActiveDate = DateTime.Now;
                _documentRepository.UpdateDocumentHistory(historyEntity);//Update History Data
            }
            return _unitOfWork.Commit() > 0;
        }

        public UploadResponse SavePicture(AddEditPictureModel addEditPictureModel, int userId)
        {
            var entity = _dataMapper.Map<AddEditPictureModel, PictureEntity>(addEditPictureModel);
            entity.CreatedBy = userId;
            entity.CreatedOn = DateTime.Now;
            entity.ModifiedOn = DateTime.Now;

            _pictureRepository.Insert(entity);
            bool isSaved = _unitOfWork.Commit() > 0;
            if (isSaved)
                return new UploadResponse { Id = entity.Id };
            return null;
        }

        public bool IsDocumentNumberUnique(int id, string documentNumber)
        {
            return _documentRepository.IsDocumentNumberUnique(id, documentNumber);
        }

        public Task<List<SelectListModel>> GetAllDocumentTypeAsync()
        {
            return _groupCodesRepository.ConvertToListAsync(
                _dataMapper.Project<GroupCodeEntity, SelectListModel>(
            _groupCodesRepository.GetByModule(Constants.ModuleNames.DocumentType)));
        }

        public List<PlantSelectListModel> GetDocumentAllPlants()
        {
            return _dataMapper.Project<PlantEntity, PlantSelectListModel>(
                _plantRepository.GetAllNoTracking().Where(a => a.IsActive == true).OrderBy(w => w.DisplayOrder)
                ).ToList();
            //return _dataMapper.Project<PlantEntity, PlantSelectListModel>
            //    (_documentPlantRepository.GetDocumentAllPlants()).ToList();

        }

        public List<PlantSelectListModel> GetDocumentPlants(int id)
        {
            var query = _documentPlantRepository.GetPlantsForDocumentHistory(id).Where(a => a.IsActive == true);
            if (query == null)
                return new List<PlantSelectListModel>();
            return _dataMapper.Project<PlantEntity, PlantSelectListModel>
                (query).ToList();
        }

        public IQueryable<DocumentListModel> GetHistoryByDocumentId(int id)
        {
            //var entity = ;
            //Task.Run(() => _documentRepository.GetHistoryByDocumentId(id));
            var mappingmodel = _dataMapper.Project<DocumentHistoryEntity, DocumentListModel>(_documentRepository.GetHistoryByDocumentId(id));

            return mappingmodel;
        }

        public List<PlantDocumentListModel> AssignAllActivePlant(int id, bool isRestrected = false)
        {
            var documentEntity = _documentRepository.GetById(id);
            var plantData = _plantRepository.GetAllNoTracking()
                .Where(w => w.IsActive);//DTA Plant

            if (isRestrected)
            {
                plantData = plantData.Where(m => m.Id != 30);
            }

            return plantData.Select(s => new PlantDocumentListModel
            {
                Id = documentEntity.Id,
                CanDownload = documentEntity.Category.IsAvailableForDownload,
                DocumentDisplayName = documentEntity.Alias,
                PlantAlias = s.Alias,
                PlantId = s.Id,
                PlantName = s.Name,
                DisplayOrder = s.DisplayOrder
            }).ToList();

        }

        public Dictionary<int, List<PlantDocumentListModel>> AssignAllActivePlants(List<int> docIds, bool isRestricted = false)
        {
            // Step 1: Get all needed document entities at once
            var documents = _documentRepository.GetAll()
      .Where(d => docIds.Contains(d.Id))
      //.Include(d => d.Category) // 👈 explicitly include the navigation property
      .ToList();


            // Step 2: Get plant data (apply restriction if needed)
            var plantData = _plantRepository.GetAllNoTracking()
                .Where(w => w.IsActive);

            if (isRestricted)
            {
                plantData = plantData.Where(p => p.Id != 30);
            }

            var plants = plantData.ToList();

            // Step 3: Build dictionary to hold assignments
            var result = new Dictionary<int, List<PlantDocumentListModel>>();

            foreach (var doc in documents)
            {
                var mappedPlants = plants.Select(plant => new PlantDocumentListModel
                {
                    Id = doc.Id,
                    CanDownload = doc.Category?.IsAvailableForDownload ?? false,
                    DocumentDisplayName = doc.Alias,
                    PlantAlias = plant.Alias,
                    PlantId = plant.Id,
                    PlantName = plant.Name,
                    DisplayOrder = plant.DisplayOrder
                }).ToList();

                result[doc.Id] = mappedPlants;
            }

            return result;
        }


        public List<PlantDocumentListModel> AssignAllActivePlantForHistory(int id)
        {
            var documentEntity = _documentRepository.GetHistoryByIdWithInclude(id);//GetHistoryById(id);
            return _plantRepository.GetAllNoTracking()
                .Where(w => w.IsActive)
                .Select(s => new PlantDocumentListModel
                {
                    Id = documentEntity.Id,
                    CanDownload = documentEntity.Category.IsAvailableForDownload,
                    DocumentDisplayName = documentEntity.Alias,
                    PlantAlias = s.Alias,
                    PlantId = s.Id,
                    PlantName = s.Name,
                    DisplayOrder = s.DisplayOrder
                }).ToList();
        }

        public bool ChangeStatus(int id, bool status, bool isNotify = false)
        {
            var documentEntity = _documentRepository.GetByIdWithIncludes(id);//GetById(id);

            if (documentEntity == null)
                return false;

            documentEntity.IsActive = status;
            _documentRepository.Update(documentEntity);

            //var employees = _employeeRepository.GetAllActive().Where(m => !string.IsNullOrEmpty(m.OfficalEmpEmailID));
            //var employees = _employeeRepository.GetAllEmpNoTracking().Where(m => !string.IsNullOrEmpty(m.OfficalEmpEmailID));
            var employees = _employeeRepository
    .GetAllNoTracking()
    .Where(e => !string.IsNullOrEmpty(e.OfficalEmpEmailID) && e.IsActive == true)
    .Select(e => new
    {
        PlantId = e.PlantId,
        Email = e.OfficalEmpEmailID
    })
    .ToList(); // 🔥 SQL EXECUTES HERE (ONCE)

            var location = _dashboardService.GetClickablePathForCategory(documentEntity.CategoryId);
            var locationstring = string.Empty;
            if (location != null)
                locationstring = string.Join(" > ", location.Select(m => m.Name).ToArray());
            if (!_groupCodesRepository.IsGlobalDocument(documentEntity.DocumentTypeId))
            {
                var plants = documentEntity.DocumentPlantMaps.Select(m => m.Plant).Select(m => m.Id).Distinct().ToList();
                employees = employees.Where(a => plants.Any(b => b == a.PlantId)).ToList();
            }
            string exc = "";
            //var employeesList = employees.Select(m => m.OfficalEmpEmailID).ToEmailList();

            var employeesList = employees.Select(m => m.Email).ToList();
            if (isNotify)
            {
                if (status == false)
                {
                    string Subject = $"COSMOS || {documentEntity.Name} is Obsoleted";

                    string basePath = _hostingEnvironment.ContentRootPath;
                    var filePath = "EmailTemplate/EmailDocument.template";
                    string path = Path.Combine(basePath, filePath);
                    var template = File.ReadAllText(path).ToString();
                    template = template.Replace("#EmployeeName#", "Dear Employee")
                        .Replace("#DocuNo#", documentEntity.DocumentNumber)
                        .Replace("#DocuName#", documentEntity.Name)
                        .Replace("#Status#", "Obsoleted")
                        .Replace("#RevisionNo#", documentEntity.RevisionNumber)
                        .Replace("#Location#", locationstring);

                    var bccemails = new List<string>();
                    if (employeesList.Count > 0)
                    {
                        bccemails = employeesList.ToList();
                    }
                    _emailHelper.SendEmail(string.Empty, Subject, template, bccemails);
                }
            }
            //return (true, string.Join(",", bccemails));
            return _unitOfWork.Commit() > 0;
        }
        public void Delete(int id)
        {
            var docPlantMaps = _documentPlantRepository.GetAll().Where(m => m.DocumentId == id).ToList();
            foreach (var val in docPlantMaps)
            {
                _documentPlantRepository.Delete(val);
                _unitOfWork.Commit();
            }
            var docTagsMap = _documentTagsRepository.GetAll().Where(m => m.DocumentId == id).ToList();
            foreach (var val in docTagsMap)
            {
                _documentTagsRepository.Delete(val);
                _unitOfWork.Commit();
            }
            var doc = _documentRepository.GetById(id);
            _documentRepository.Delete(doc);
            _unitOfWork.Commit();
        }

        public async Task<List<DocumentListModel>> GetAllPrefixDocNumber()
        {
            var spData = await _categoryRepository.GetAllPrefixDocNumber();

            return spData;
            //var data = _dataMapper.Project<CategoryEntity, DocumentChartModel>(_categoryRepository.GetPrimaryCategories()).ToList();
            //return data;
        }

        public IQueryable<DocumentListModel> GetAllDocumentHistory()
        {

            return _dataMapper.Project<DocumentHistoryEntity, DocumentListModel>
                (_documentRepository.GetAllDocumentHistory());
        }

        public async Task<List<DocumentListModel>> GetAllDocumentList()
        {
            var spData = await _categoryRepository.GetAllDocumentList();
            return spData;
        }

        public bool InsertDocumentEmailData(DocumentEmailDataModel Model, int userId)
        {
            try
            {
                //var entity = _documentRepository.GetById(Model.DocumentId);
                var entity = _documentRepository.GetByIdWithIncludes(Model.DocumentId);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    var emailDataEntity = _dataMapper.Map<DocumentEmailDataModel, DocumentEmailDataEntity>(Model);
                    emailDataEntity.DocumentTypeId = entity.DocumentTypeId;
                    emailDataEntity.DocumentTypeName = entity.DocumentType.Name;
                    emailDataEntity.DocumentName = entity.Name;
                    emailDataEntity.DocumentNumber = entity.DocumentNumber;
                    emailDataEntity.RevisionNumber = entity.RevisionNumber;
                    emailDataEntity.ProcessOwner = entity.ProcessOwner;
                    emailDataEntity.IsActive = true;
                    emailDataEntity.IsMailSend = false;
                    emailDataEntity.CreatedBy = userId;
                    emailDataEntity.CreatedOn = DateTime.Now;
                    emailDataEntity.ModifiedBy = userId;
                    emailDataEntity.ModifiedOn = DateTime.Now;
                    _documentEmailDataRepository.Insert(emailDataEntity);
                }
                return _unitOfWork.Commit() > 0;
            }
            catch (Exception er)
            {
                return false;
            }
        }

        public async Task<List<DocumentListModel>> GetFilteredDocumentsForExportAsync(int statusId, string docTypePrefix, long? employeeId)
        {
            var spData = await _categoryRepository.GetFilteredDocumentsForExportAsync(statusId, docTypePrefix, employeeId);
            return spData;
        }
        public async Task<List<DocumentListModel>> GetAllDocumentViewScreenAsync(int employeeId)
        {
            var spData = await _categoryRepository.GetAllDocumentViewScreenAsync(employeeId);
            return spData;
        }

        public IQueryable<DocumentEmailDataModel> GetPendingEmailDocuments()
        {
            var entity = _documentEmailDataRepository.GetAllNoTracking().Where(a => a.IsActive == true && a.IsMailSend != true);
            return _dataMapper.Project<DocumentEmailDataEntity, DocumentEmailDataModel>
                (entity);
        }

        public IQueryable<DocumentEmailDataModel> GetPendingEmailDocumentsBySP(int employeeid, DateTime? date)
        {
            var entity = _storedProcedureContext.GetAllDocumentEmailDataAsync(employeeid, date);
            return entity.Result.AsQueryable();
        }
        public bool SendPendingEmail(string ids, string title, int userId)
        {
            bool result = false;
            if (ids != null && ids != string.Empty)
            {
                List<int> pendingIds = new List<int>();
                string[] str = ids.Split(',');
                foreach (var item in str)
                {
                    int id = item != null && item != string.Empty ? Convert.ToInt32(item) : 0;
                    pendingIds.Add(id);
                }

                string Subject = $"QMS || PCF Documents Changes / Updates";
                if (title != null && title != "")
                {
                    Subject = Subject + $" ({title})";
                }

                //Task.Run(async () =>
                //{
                #region Email
                List<string> emailList = new List<string>();
                List<EmployeewiseDocumentMailModel> model = new List<EmployeewiseDocumentMailModel>();

                var mailData = _documentEmailDataRepository.GetAllNoTracking().Where(a => pendingIds.Any(b => a.Id == b));
                if (mailData != null && mailData.Count() > 0)
                {
                    foreach (var docData in mailData.ToList())
                    {
                        var employees = _employeeRepository.GetAllActive().Where(m => !string.IsNullOrEmpty(m.OfficalEmpEmailID));
                        var entity = _documentRepository.GetByIdWithIncludes(docData.DocumentId);

                        var location = _dashboardService.GetClickablePathForCategory(entity.CategoryId);
                        var locationstring = string.Empty;
                        if (location != null)
                        {
                            locationstring = string.Join(" > ", location.Select(m => m.Name).ToArray());
                        }
                        if (!_groupCodesRepository.IsGlobalDocument(entity.DocumentTypeId))
                        {
                            //var plants = entity.DocumentPlantMaps.Select(m => m.Plant).Select(m => m.Name).ToList();
                            //employees = employees.Where(m => plants.Contains(m.Plant));
                            var specialAliases = _specialPlantConfig?.PlantAliases ?? new List<string>();
                            var plantIds = entity.DocumentPlantMaps
              .Select(m => m.PlantId)
              .ToList();

                            var documentPlants = entity.DocumentPlantMaps
        .Where(m => m.Plant != null)
        .Select(m => new { m.PlantId, Alias = m.Plant.Alias })
        .ToList();

                            var specialPlants = documentPlants
                                .Where(p => !string.IsNullOrWhiteSpace(p.Alias) &&
                                            specialAliases.Any(a => a.Equals(p.Alias, StringComparison.OrdinalIgnoreCase)))
                                .ToList();

                            var normalPlants = documentPlants
                                .Where(p => string.IsNullOrWhiteSpace(p.Alias) ||
                                            !specialAliases.Any(a => a.Equals(p.Alias, StringComparison.OrdinalIgnoreCase)))
                                .ToList();

                            // ✅ Case 1 & 3 → add fixed emails if ANY special plant exists
                            if (specialPlants.Any())
                            {
                                var fixedEmails = _specialPlantConfig.Emails
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim());

                                emailList.AddRange(fixedEmails);
                            }

                            // ✅ Case 2 & 3 → add employees for normal plants
                            if (normalPlants.Any())
                            {
                                var normalPlantIds = normalPlants.Select(p => p.PlantId).ToList();

                                var employeesBase = _employeeRepository.GetAllActive()
                                    .Where(e => e.IsActive && !string.IsNullOrEmpty(e.OfficalEmpEmailID));

                                var employeesSinglePlant = employeesBase
                                    .Where(e => e.PlantId != null && normalPlantIds.Contains(e.PlantId.Value))
                                    .ToList();

                                var employeesMultiPlant = employeesBase
                                    .Where(e => !string.IsNullOrEmpty(e.PlantIds))
                                    .AsEnumerable()
                                    .Where(e =>
                                        e.PlantIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Any(pid => normalPlantIds.Contains(Convert.ToInt32(pid)))
                                    )
                                    .ToList();

                                employees = employeesSinglePlant
                                    .Union(employeesMultiPlant)
                                    .Distinct()
                                    .ToList().AsQueryable();

                                emailList.AddRange(employees.Select(e => e.OfficalEmpEmailID));
                            }

                            // ✅ Final cleanup
                            emailList = emailList.Distinct().ToList();
                        }
                        else
                        {
                            emailList = employees.Select(m => m.OfficalEmpEmailID).ToList();
                        }
                        //var employeesList = employees.Select(m => m.OfficalEmpEmailID).ToEmailList();
                        if (!string.IsNullOrEmpty(docData.SpecificPersonEmail))
                        {
                            emailList.AddRange(docData.SpecificPersonEmail.StringToEmailList(','));
                        }

                        foreach (var emp in emailList)
                        {
                            EmployeewiseDocumentMailModel empModel = new EmployeewiseDocumentMailModel();
                            empModel.EmployeeEmail = emp;
                            DocumentMailModel documentMail = new DocumentMailModel();
                            documentMail.DocumentName = docData.DocumentName;
                            documentMail.DocumentNumber = docData.DocumentNumber;
                            documentMail.DocumentType = docData.DocumentType;
                            documentMail.RevisionNo = docData.RevisionNumber;
                            documentMail.Location = locationstring;
                            empModel.DocumentModel.Add(documentMail);

                            var dt = model.Where(a => a.EmployeeEmail == emp).FirstOrDefault();
                            if (dt == null)
                            {
                                empModel.DocIds = docData.DocumentId.ToString();
                                model.Add(empModel);
                            }
                            else
                            {
                                if (!dt.DocIds.Contains(docData.DocumentId.ToString()))
                                {
                                    dt.DocIds = dt.DocIds + "," + docData.DocumentId.ToString();
                                    dt.DocumentModel.Add(documentMail);
                                }
                            }
                        }
                    }
                }

                //Email Template Related Code Here...
                try
                {
                    if (model != null && model.Count() > 0)
                    {
                        List<EmployeewiseDocumentMailModel> finalModel = new List<EmployeewiseDocumentMailModel>();
                        var distData = model.Select(a => a.DocIds).Distinct();
                        foreach (var item in distData)
                        {
                            EmployeewiseDocumentMailModel empModelData = new EmployeewiseDocumentMailModel();
                            var dt = model.Where(a => a.DocIds == item).FirstOrDefault();
                            empModelData.DocIds = item;
                            empModelData.DocumentModel.AddRange(dt.DocumentModel);
                            var emailList1 = model.Where(a => a.DocIds == item).Select(b => b.EmployeeEmail).ToList();
                            empModelData.EmailIds = emailList1;
                            finalModel.Add(empModelData);
                        }

                        foreach (var emailData in finalModel)
                        {
                            string basePath = _hostingEnvironment.ContentRootPath;
                            var filePath = "EmailTemplate/DocmentCommonEmail.template";
                            string path = Path.Combine(basePath, filePath);
                            var template = File.ReadAllText(path).ToString();

                            string tableBody = "";
                            int count = 1;
                            foreach (var item in emailData.DocumentModel)
                            {
                                string mailStr = "";
                                mailStr = "<tr>";
                                mailStr += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + item.DocumentNumber.ToString() + "</td>";
                                mailStr += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + item.DocumentName.ToString() + "</td>";
                                mailStr += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + item.RevisionNo.ToString() + "</td>";
                                mailStr += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + item.DocumentType?.ToString() + "</td>";
                                mailStr += "</tr>";
                                tableBody = tableBody + mailStr;
                                count++;
                            }

                            template = template.Replace("###TableBody", tableBody);
                            try
                            {
                                if (emailData.EmailIds != null && emailData.EmailIds.Count() > 0)
                                {
                                    _emailHelper.SendEmail(string.Empty, Subject, template, emailData.EmailIds);
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error while sending document email", e);
                }

                //});
                #endregion

                //Update Document Email Data Table
                foreach (var eDataId in pendingIds)
                {
                    var updEntity = _documentEmailDataRepository.GetById(eDataId);
                    updEntity.IsMailSend = true;
                    updEntity.ModifiedBy = userId;
                    updEntity.ModifiedOn = DateTime.Now;
                    _documentEmailDataRepository.Update(updEntity);
                }
                _unitOfWork.Commit();
                result = true;
            }
            return result;
        }

        public List<EmailPreviewModel> GetEmailPreview(string ids, string title, int userId)
        {
            try
            {

                List<EmailPreviewModel> previewList = new List<EmailPreviewModel>();

                if (string.IsNullOrEmpty(ids))
                    return previewList;

                List<int> pendingIds = ids.Split(',')
                                          .Select(x => Convert.ToInt32(x))
                                          .ToList();

                string Subject = $"QMS || PCF Documents Changes / Updates";
                if (!string.IsNullOrEmpty(title))
                {
                    Subject += $" ({title})";
                }

                var mailData = _documentEmailDataRepository
                                .GetAllNoTracking()
                                .Where(a => pendingIds.Contains(a.Id))
                                .ToList();
                List<string> emailList = new List<string>();
                Dictionary<string, EmailPreviewModel> consolidated = new Dictionary<string, EmailPreviewModel>();

                foreach (var docData in mailData)
                {
                    emailList = new List<string>();
                    var employees = _employeeRepository.GetAllActive()
                                    .Where(m => !string.IsNullOrEmpty(m.OfficalEmpEmailID));

                    var entity = _documentRepository.GetByIdWithIncludes(docData.DocumentId);

                    var location = _dashboardService.GetClickablePathForCategory(entity.CategoryId);
                    string locationstring = location != null
                        ? string.Join(" > ", location.Select(m => m.Name))
                        : "";

                    if (!_groupCodesRepository.IsGlobalDocument(entity.DocumentTypeId))
                    {
                        //var plants = entity.DocumentPlantMaps.Select(m => m.Plant.Name).ToList();
                        //employees = employees.Where(m => plants.Contains(m.Plant));
                        var specialAliases = _specialPlantConfig?.PlantAliases ?? new List<string>();
                        var plantIds = entity.DocumentPlantMaps
          .Select(m => m.PlantId)
          .ToList();

                        var documentPlants = entity.DocumentPlantMaps
    .Where(m => m.Plant != null)
    .Select(m => new { m.PlantId, Alias = m.Plant.Alias })
    .ToList();

                        var specialPlants = documentPlants
                            .Where(p => !string.IsNullOrWhiteSpace(p.Alias) &&
                                        specialAliases.Any(a => a.Equals(p.Alias, StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                        var normalPlants = documentPlants
                            .Where(p => string.IsNullOrWhiteSpace(p.Alias) ||
                                        !specialAliases.Any(a => a.Equals(p.Alias, StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                        // ✅ Case 1 & 3 → add fixed emails if ANY special plant exists
                        if (specialPlants.Any())
                        {
                            var fixedEmails = _specialPlantConfig.Emails
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim());

                            emailList.AddRange(fixedEmails);
                        }

                        // ✅ Case 2 & 3 → add employees for normal plants
                        if (normalPlants.Any())
                        {
                            var normalPlantIds = normalPlants.Select(p => p.PlantId).ToList();

                            var employeesBase = _employeeRepository.GetAllActive()
                                .Where(e => e.IsActive && !string.IsNullOrEmpty(e.OfficalEmpEmailID));

                            var employeesSinglePlant = employeesBase
                                .Where(e => e.PlantId != null && normalPlantIds.Contains(e.PlantId.Value))
                                .ToList();

                            var employeesMultiPlant = employeesBase
                                .Where(e => !string.IsNullOrEmpty(e.PlantIds))
                                .AsEnumerable()
                                .Where(e =>
                                    e.PlantIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Any(pid => normalPlantIds.Contains(Convert.ToInt32(pid)))
                                )
                                .ToList();

                            employees = employeesSinglePlant
                                .Union(employeesMultiPlant)
                                .Distinct()
                                .ToList().AsQueryable();

                            emailList.AddRange(employees.Select(e => e.OfficalEmpEmailID));
                        }

                        // ✅ Final cleanup
                        emailList = emailList.Distinct().ToList();
                    }
                    else
                    {
                        emailList = employees.Select(m => m.OfficalEmpEmailID).ToList();
                    }

                    if (!string.IsNullOrEmpty(docData.SpecificPersonEmail))
                    {
                        emailList.AddRange(docData.SpecificPersonEmail.Split(','));
                    }

                    emailList = emailList.Distinct().OrderBy(x => x).ToList();

                    string emailKey = string.Join(",", emailList);

                    DocumentMailModel documentMail = new DocumentMailModel
                    {
                        DocumentName = docData.DocumentName,
                        DocumentNumber = docData.DocumentNumber,
                        DocumentType = docData.DocumentType,
                        RevisionNo = docData.RevisionNumber,
                        Location = locationstring
                    };

                    if (!consolidated.ContainsKey(emailKey))
                    {
                        consolidated[emailKey] = new EmailPreviewModel
                        {
                            Subject = Subject,
                            Recipients = emailList,
                            Documents = new List<DocumentMailModel>()
                        };
                    }

                    consolidated[emailKey].Documents.Add(documentMail);
                }

                previewList = consolidated.Values.ToList();

                return previewList;
            }
            catch (Exception e)
            {
                Log.Error("Error while generating email preview", e);
                return new List<EmailPreviewModel>();
            }
        }

    }
}
