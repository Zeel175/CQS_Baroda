using CQSAirborne.Domain;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.QuickSearch;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDataMapper _dataMapper;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentPlantRepository _documentPlantRepository;
        private readonly IPictureRepository _pictureRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DashboardService(ICategoryRepository categoryRepository
            , IDataMapper dataMapper
            , IDocumentRepository documentRepository
            , IDocumentPlantRepository documentPlantRepository
            , IPictureRepository pictureRepository,
IEmployeeRepository employeeRepository)
        {
            _categoryRepository = categoryRepository;
            _dataMapper = dataMapper;
            _documentRepository = documentRepository;
            _documentPlantRepository = documentPlantRepository;
            _pictureRepository = pictureRepository;
            _employeeRepository = employeeRepository;
        }

        public List<DashboardCategoryModel> GetDashboardCategory(int id, int parentCategoryId)
        {
            if (parentCategoryId > 0)
            {
                var result = _dataMapper.Project<CategoryEntity, DashboardCategoryModel>(_categoryRepository.GetByParentCategory(parentCategoryId))
                   .ToList().Select(s =>
                   {
                       s.HasChildCategory = false;

                       return s;
                   }).OrderBy(m=>m.Name).ToList();
                return result;
            }
            else
            {
                var data = _categoryRepository.GetByParentCategory(id);
                if (id == 0)
                {
                    data = data.Where(a => !a.Name.ToLower().Contains("quality alerts and guidelines") && !a.Name.ToLower().Contains("company credentials") && !a.Name.ToLower().Contains("standards") && !a.Name.ToLower().Contains(("training and support material").ToLower()) && !a.Name.ToLower().Contains(("audit management").ToLower()));
                }

                var result = _dataMapper.Project<CategoryEntity, DashboardCategoryModel>(data)
                    .ToList();
                return result;
            }
        }


        public DocumentDetailModel GetDocumentById(int id, DocumentOperationType documentOperationType)
        {
            switch (documentOperationType)
            {
                case DocumentOperationType.Document:
                    return _documentPlantRepository.GetActiveByPredicate(w => w.Id == id)
                        .Select(w => new DocumentDetailModel
                        {
                            Id = w.Id,
                            Path = w.Picture.Path,
                            CanDownload = w.Document.Category.IsAvailableForDownload,
                            Extension = w.Picture.Extension,
                            DisplayFileName = w.Picture.DisplayName
                        }).FirstOrDefault();
                case DocumentOperationType.History:
                    return _documentPlantRepository.GetPlantMapHistoryById(id)
                        .Select(w => new DocumentDetailModel
                        {
                            Id = w.Id,
                            Path = w.Picture.Path,
                            CanDownload = w.Document.Category.IsAvailableForDownload,
                            Extension = w.Picture.Extension,
                            DisplayFileName = w.Picture.DisplayName
                        }).FirstOrDefault();
                case DocumentOperationType.Common:
                    return _documentRepository.GetActiveById(id)
                        .Select(documentEntity => new DocumentDetailModel
                        {
                            Id = documentEntity.Id,
                            Path = documentEntity.CommonPicture.Path,
                            CanDownload = documentEntity.Category.IsAvailableForDownload,
                            Extension = documentEntity.CommonPicture.Extension,
                            DisplayFileName = documentEntity.CommonPicture.DisplayName
                        }).FirstOrDefault();
            }
            return null;
        }

        public DocumentDetailModel GetPictureDetails(long id)
        {
            var pictureEntity = _pictureRepository.GetById(id);
            if (pictureEntity == null)
                return null;
            return new DocumentDetailModel
            {
                Id = Convert.ToInt32(pictureEntity.Id),
                Path = pictureEntity.Path,
                DisplayFileName = pictureEntity.DisplayName,
                Extension = pictureEntity.Extension
            };

        }

        public IQueryable<DocumentListModel> GetDocuments(int id)
        {
            return _dataMapper.Project<DocumentEntity, DocumentListModel>
                (_documentRepository.GetByCategory(id));
        }

        //public DataSourceResult GetQuickSearchData(QuickSearchRequestModel quickSearchRequestModel)
        //{
        //    IQueryable<QuickSearchListModel> query = null;

        //    switch (quickSearchRequestModel.DbColumn)
        //    {
        //        case QuickSearchDbColumn.CategoryName:
        //            query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
        //                _categoryRepository.GetQuickSearches().Where(w => w.DbName == QuickSearchDbColumn.CategoryName));
        //            break;
        //        case QuickSearchDbColumn.DocumentCode:
        //            query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
        //                _categoryRepository.GetQuickSearches().Where(w => w.DbName == QuickSearchDbColumn.DocumentCode));
        //            break;
        //        case QuickSearchDbColumn.DocumentName:
        //            query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
        //                _categoryRepository.GetQuickSearches().Where(w => w.DbName == QuickSearchDbColumn.DocumentName));
        //            break;
        //        case QuickSearchDbColumn.DocumentUniqueCode:
        //            query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
        //                _categoryRepository.GetQuickSearches().Where(w => w.DbName == QuickSearchDbColumn.DocumentUniqueCode));
        //            break;
        //        case QuickSearchDbColumn.RevisionNumber:
        //            query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
        //                _categoryRepository.GetQuickSearches().Where(w => w.DbName == QuickSearchDbColumn.RevisionNumber));
        //            break;
        //        case QuickSearchDbColumn.ALL:
        //            query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
        //                _categoryRepository.GetQuickSearches());
        //            break;
        //    }

        //    if (!string.IsNullOrEmpty(quickSearchRequestModel.SearchValue))
        //    {
        //        query = query.Where(w => w.CompareText.Contains(quickSearchRequestModel.SearchValue));
        //    }

        //    if (quickSearchRequestModel.Order[0].Dir == "asc")
        //        query = query.OrderBy(w => w.CompareText);
        //    else
        //        query = query.OrderByDescending(w => w.CompareText);

        //    int totalRecord = query.Count();
        //    query = query.Skip(quickSearchRequestModel.Start).Take(quickSearchRequestModel.Length);


        //    var data = query.ToList();
        //    foreach (var entry in data)
        //    {
        //        if (entry.Type == QuickSearchDbColumn.CategoryName)
        //        {
        //            entry.ClickablePath = GetClickablePathForCategory(entry.Id);
        //        }
        //        else
        //        {
        //            entry.ClickablePath = GetClickableLinkForDocument(entry.Id, (DocumentOperationType)entry.DocumentTypeId);
        //        }
        //    }


        //    return new DataSourceResult
        //    {
        //        Data = data,
        //        Draw = quickSearchRequestModel.Draw,
        //        RecordsFiltered = totalRecord,
        //        RecordsTotal = totalRecord
        //    };
        //}

        public DataSourceResult GetQuickSearchData(QuickSearchRequestModel quickSearchRequestModel, int userId)
        {
            var employee = _employeeRepository.GetAll()
                    .Where(x => x.Id == userId)
                    .Select(x => new
                    {
                        x.OrgRole,
                        x.PlantIds,
                        x.PlantId
                    })
                    .FirstOrDefault();

            bool isAdmin = employee != null &&
                           !string.IsNullOrEmpty(employee.OrgRole) &&
                           employee.OrgRole.ToLower() == "admin";

            List<int> plantIdList = new List<int>();

            // Case 1: Multi Plant
            if (!string.IsNullOrEmpty(employee?.PlantIds))
            {
                plantIdList = employee.PlantIds
                                .Split(',')
                                .Select(p => int.TryParse(p, out int val) ? val : 0)
                                .Where(p => p > 0)
                                .ToList();
            }
            // Case 2: Single Plant
            if (employee?.PlantId != null && employee.PlantId > 0)
            {
                plantIdList.Add(employee.PlantId.Value);
            }

            IQueryable<ViewQuickSearch> baseQuery = _categoryRepository.GetQuickSearches();

            // 2️⃣ Apply Plant Filtering (Same Logic as SP)
            if (plantIdList.Any())
            {
                baseQuery = baseQuery.Where(x => plantIdList.Contains(x.PlantId ?? 0));

    //            baseQuery = baseQuery.Where(x =>
    //x.PlantId == null ||
    //plantIdList.Contains(x.PlantId.Value));
            }
            else
            {
                if (!isAdmin)
                {
                    // Non-admin with no plants → return empty
                    baseQuery = baseQuery.Where(x => false);
                }
                // Admin with no plants → no filter (see all)
            }

            IQueryable<QuickSearchListModel> query = null;

            switch (quickSearchRequestModel.DbColumn)
            {
                case QuickSearchDbColumn.CategoryName:
                    query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
                        baseQuery.Where(w => w.DbName == QuickSearchDbColumn.CategoryName));
                    break;

                case QuickSearchDbColumn.DocumentCode:
                    query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
                        baseQuery.Where(w => w.DbName == QuickSearchDbColumn.DocumentCode));
                    break;

                case QuickSearchDbColumn.DocumentName:
                    query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
                        baseQuery.Where(w => w.DbName == QuickSearchDbColumn.DocumentName));
                    break;

                case QuickSearchDbColumn.DocumentUniqueCode:
                    query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
                        baseQuery.Where(w => w.DbName == QuickSearchDbColumn.DocumentUniqueCode));
                    break;

                case QuickSearchDbColumn.RevisionNumber:
                    query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(
                        baseQuery.Where(w => w.DbName == QuickSearchDbColumn.RevisionNumber));
                    break;

                case QuickSearchDbColumn.ALL:
                    query = _dataMapper.Project<ViewQuickSearch, QuickSearchListModel>(baseQuery);
                    break;
            }

            if (!string.IsNullOrEmpty(quickSearchRequestModel.SearchValue))
            {
                query = query.Where(w => w.CompareText.Contains(quickSearchRequestModel.SearchValue));
            }

            if (quickSearchRequestModel.Order[0].Dir == "asc")
                query = query.OrderBy(w => w.CompareText);
            else
                query = query.OrderByDescending(w => w.CompareText);

            int totalRecord = query.Count();

            query = query.Skip(quickSearchRequestModel.Start)
                         .Take(quickSearchRequestModel.Length);

            var data = query.ToList();

            foreach (var entry in data)
            {
                if (entry.Type == QuickSearchDbColumn.CategoryName)
                {
                    entry.ClickablePath = GetClickablePathForCategory(entry.Id);
                }
                else
                {
                    entry.ClickablePath = GetClickableLinkForDocument(entry.Id, (DocumentOperationType)entry.DocumentTypeId);
                }
            }

            return new DataSourceResult
            {
                Data = data,
                Draw = quickSearchRequestModel.Draw,
                RecordsFiltered = totalRecord,
                RecordsTotal = totalRecord
            };
        }


        //private string GetNavigationLinkForDocument(int id)
        //{
        //    var documentPlant = _documentPlantRepository.GetById(id);
        //    return GetNavigationByCategory(documentPlant.Document.CategoryId, documentPlant.Document.Name);
        //}

        private List<ClickablePathModel> GetClickableLinkForDocument(int id, DocumentOperationType documentOperationType)
        {
            switch (documentOperationType)
            {
                case DocumentOperationType.Document:
                    //var documentPlant = _documentPlantRepository.GetById(id);
                    var documentPlant = _documentPlantRepository.GetByIdWithIncludes(id);
                    List<ClickablePathModel> list = new List<ClickablePathModel>
                    {
                        new ClickablePathModel
                        {
                            Id = id,
                            Name = documentPlant.Document.Name,
                            ParentId = documentPlant.Document.CategoryId,
                            IsLastElement = true,
                            DocFullName = ((documentPlant.Document.DocumentNumber??"") + " - " + (documentPlant.Document.Name ?? ""))
                        },
                        new ClickablePathModel
                        {
                            Id = documentPlant.Document.CategoryId,
                            Name = documentPlant.Document.Category.Name,
                            ParentId = documentPlant.Document.Category.PrimaryCategoryId,
                        }
                    };
                    return GetClickablePathChildForCategory(documentPlant.Document.Category, list);
                case DocumentOperationType.History:
                    //var documentPlantHistory = _documentPlantRepository.GetPlantMapHistoryById(id).ToList().FirstOrDefault();
                    var documentPlantHistory = _documentPlantRepository.GetPlantMapHistoryByIdNewWithIncludes(id).FirstOrDefault();
                    List<ClickablePathModel> resultList = new List<ClickablePathModel>
                    {
                        new ClickablePathModel
                        {
                            Id = id,
                            Name = documentPlantHistory.Document.Name,
                            ParentId = documentPlantHistory.Document.CategoryId,
                            IsLastElement = true,
                            DocFullName = ((documentPlantHistory.Document.DocumentNumber??"") + " - " + (documentPlantHistory.Document.Name ?? ""))
                        },
                        new ClickablePathModel
                        {
                            Id = documentPlantHistory.Document.CategoryId,
                            Name = documentPlantHistory.Document.Category.Name,
                            ParentId = documentPlantHistory.Document.Category.PrimaryCategoryId,
                        }
                    };
                    return GetClickablePathChildForCategory(documentPlantHistory.Document.Category, resultList);
                case DocumentOperationType.Common:
                    var documentEntity = _documentRepository.GetByIdWithIncludes(id);

                    List<ClickablePathModel> documentResultList = new List<ClickablePathModel>
                    {
                        new ClickablePathModel
                        {
                            Id = id,
                            Name = documentEntity.Name,
                            ParentId = documentEntity.CategoryId,
                            IsLastElement = true,
                            DocFullName = ((documentEntity.DocumentNumber??"") + " - " + (documentEntity.Name ?? ""))
                        },
                        new ClickablePathModel
                        {
                            Id = documentEntity.CategoryId,
                            Name = documentEntity.Category.Name,
                            ParentId = documentEntity.Category.PrimaryCategoryId,
                        }
                    };
                    return GetClickablePathChildForCategory(documentEntity.Category, documentResultList);
            }
            return new List<ClickablePathModel>();
        }

        public List<ClickablePathModel> GetClickablePathForCategory(int id)
        {
            var category = _categoryRepository.GetById(id);
            if (category == null)
                return new List<ClickablePathModel>();

            return GetClickablePathChildForCategory(category, new List<ClickablePathModel>
            {
                new ClickablePathModel
                {
                    Id = id,
                    IsLastElement = true,
                    Name =category.Name,
                    ParentId = category.PrimaryCategoryId
                }
            });
        }

        private List<ClickablePathModel> GetClickablePathChildForCategory(CategoryEntity category, List<ClickablePathModel> list)
        {

            if (category.PrimaryCategory != null)
            {
                list.Add(new ClickablePathModel
                {
                    Id = category.PrimaryCategory.Id,
                    ParentId = category.PrimaryCategory.PrimaryCategoryId,
                    Name = category.PrimaryCategory.Name
                });
                GetClickablePathChildForCategory(category.PrimaryCategory, list);
            }

            return list;
        }

        public IQueryable<CategoryListModel> GetSubCategories(int id)
        {
            IQueryable<CategoryEntity> data = _categoryRepository.GetSubCategories(id);
            return _dataMapper.Project<CategoryEntity, CategoryListModel>(data);
        }

        public DashboardModel GetCreateModel(int id)
        {
            return new DashboardModel
            {
                CategoryId = id,
                //NavigationLink = GetNavigationLinkForCategory(id),
                ClickablePath = GetClickablePathForCategory(id)
            };
        }

        public ViewDocumentModel GetViewDocumentModel(int id, DocumentOperationType documentOperationType)
        {
            return new ViewDocumentModel
            {
                DocumentId = id,
                DocumentOperationType = documentOperationType,
                ClickablePath = GetClickableLinkForDocument(id, documentOperationType)
            };
        }


    }
}
