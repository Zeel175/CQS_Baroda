using AutoMapper;
using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Dashboard;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.Employee;
using CQSAirborne.Model.Customer;
using CQSAirborne.Model.ErrorLog;
using CQSAirborne.Model.ExternalLink;
using CQSAirborne.Model.Plant;
using CQSAirborne.Model.QuickSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQSAirborne.Model.Role;
using CQSAirborne.Model.CPRMaster;

namespace CQSAirborne.Services.Implementation.Utils
{
    public static class MappingConfig
    {
        public static MapperConfiguration Register()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PlantEntity, PlantListModel>();
                cfg.CreateMap<PlantEntity, AddEditPlantModel>();
                cfg.CreateMap<AddEditPlantModel, PlantEntity>();

                cfg.CreateMap<CategoryEntity, CategoryListModel>()
                    .ForMember(dest => dest.CategoryType, opt => opt.MapFrom(src => src.CategoryType.Name))
                    .ForMember(dest => dest.PrimaryCategory, opt => opt.MapFrom(src => src.PrimaryCategory != null ? src.PrimaryCategory.Name : ""))
                ;


                cfg.CreateMap<GroupCodeEntity, SelectListModel>();
                cfg.CreateMap<AddEditCategoryModel, CategoryEntity>();
                cfg.CreateMap<CategoryEntity, SelectListModel>();
                cfg.CreateMap<CategoryEntity, AddEditCategoryModel>();

                cfg.CreateMap<CustomerEntity, AddEditCustomerModel>()
                .ForMember(dest => dest.CustomerDocumentDetail, opt => opt.MapFrom(src => src.CustomerDocumentMappings));
                cfg.CreateMap<AddEditCustomerModel, CustomerEntity>();


                cfg.CreateMap<CategoryEntity, CategorySelectListModel>()
                    .ForMember(dest => dest.PrimaryCategoryName, opt => opt.MapFrom(src => src.PrimaryCategory != null ? src.PrimaryCategory.Name : ""))
                ;

                cfg.CreateMap<DocumentEntity, DocumentListModel>()
                    .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
                    .ForMember(dest => dest.CanDownload, opt => opt.MapFrom(src => src.Category.IsAvailableForDownload))
                    .ForMember(dest => dest.DocumentTypeCode, opt => opt.MapFrom(src => src.DocumentType.Code))
                    .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src => src.DocumentType.Name))
                    .ForMember(dest => dest.Plants, opt => opt.MapFrom(src => src.DocumentPlantMaps.Where(w => w.Plant.IsActive).Select(s => new PlantDocumentListModel
                    {
                        Id = s.Id,
                        PlantId = s.PlantId,
                        PlantAlias = s.Plant.Alias,
                        PlantName = s.Plant.Name,
                        DocumentDisplayName = s.Document.Alias,
                        CanDownload = s.Document.Category.IsAvailableForDownload,
                        DisplayOrder = s.Plant.DisplayOrder
                    }).ToList()))
                    ;

                cfg.CreateMap<DocumentHistoryEntity, DocumentListModel>()
                    .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
                    .ForMember(dest => dest.CPRNumber, opt => opt.MapFrom(src => src.CPRMasterId != null && src.CPRMasterEntity != null ? src.CPRMasterEntity.CPRUniqueCode : ""))
                    .ForMember(dest => dest.CanDownload, opt => opt.MapFrom(src => src.Category.IsAvailableForDownload))
                    .ForMember(dest => dest.Plants, opt => opt.MapFrom(src => src.DocumentPlantMaps.Where(w => w.Plant.IsActive).Select(s => new PlantDocumentListModel
                    {
                        Id = s.Id,
                        PlantId = s.PlantId,
                        PlantAlias = s.Plant.Alias,
                        PlantName = s.Plant.Name,
                        DocumentDisplayName = s.Document.Alias,
                        CanDownload = s.Document.Category.IsAvailableForDownload
                    }).ToList()))
                    ;

                cfg.CreateMap<PlantEntity, SelectListModel>()
                    .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Alias))
                ;

                cfg.CreateMap<PlantEntity, PlantSelectListModel>()
                    .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Alias))
                ;

                cfg.CreateMap<AddEditPictureModel, PictureEntity>();
                cfg.CreateMap<AddEditDocumentModel, DocumentEntity>();
                cfg.CreateMap<DocumentEntity, AddEditDocumentModel>()
                    .ForMember(dest => dest.Plants, opt => opt.MapFrom(src => src.DocumentPlantMaps.Where(w => w.Plant.IsActive).Select(s => s.PlantId)))
                    .ForMember(dest => dest.Tags, opt => opt.Ignore())
                    .ForMember(dest => dest.PrimaryCategory, opt => opt.MapFrom(src => src.Category.PrimaryCategory != null ? src.Category.PrimaryCategory.Name : ""))
                ;

                cfg.CreateMap<ExternalLinkEntity, ExternalLinkListModel>()
                ;
                cfg.CreateMap<ExternalLinkEntity, AddEditExternalLinkModel>()
                ;
                cfg.CreateMap<AddEditExternalLinkModel, ExternalLinkEntity>()
                ;

                cfg.CreateMap<CategoryEntity, DashboardCategoryModel>()
                    .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                    .ForMember(dest => dest.HtmlTemplate, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.CategoryType, opt => opt.MapFrom(src => src.CategoryType.Name))
                    .ForMember(dest => dest.HasChildCategory, opt => opt.MapFrom(src => true))
                    .ForMember(dest => dest.HasDocument, opt => opt.MapFrom(src => src.Documents.Any(a => a.IsActive)))
                    .ForMember(dest => dest.HasSubCategory, opt => opt.MapFrom(src => src.SecondaryCategories.Any(a => a.IsActive)))
                ;

                cfg.CreateMap<AddEditErrorLogModel, ErrorLogEntity>();

                cfg.CreateMap<PlantEntity, DashboardDocumentPlantModel>();

                cfg.CreateMap<ViewQuickSearch, QuickSearchListModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.DbName))
                .ForMember(dest => dest.DocumentTypeId, opt => opt.MapFrom(src => src.DocumentTypeId == 5 ? (int)DocumentOperationType.Common : (int)DocumentOperationType.Document))
                ;

                cfg.CreateMap<DocumentEntity, DocumentHistoryEntity>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.Category, opt => opt.Ignore())
                    .ForMember(dest => dest.DocumentType, opt => opt.Ignore())
                ;
                cfg.CreateMap<DocumentHistoryEntity, AddEditDocumentModel>()
                    .ForMember(dest => dest.Plants, opt => opt.MapFrom(src => src.DocumentPlantMaps.Where(w => w.Plant.IsActive).Select(s => s.PlantId)))
                    //.ForMember(dest => dest.Tags, opt => opt.Ignore())
                    .ForMember(dest => dest.PrimaryCategory, opt => opt.MapFrom(src => src.Category.PrimaryCategory != null ? src.Category.PrimaryCategory.Name : ""))
                ;
                cfg.CreateMap<DocumentPlantMapEntity, DocumentPlantMapHistoryEntity>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.Document, opt => opt.Ignore())
                    .ForMember(dest => dest.Plant, opt => opt.Ignore())
                    .ForMember(dest => dest.Picture, opt => opt.Ignore())
                ;

                cfg.CreateMap<AddEditEmployeeViewModel, EmployeeEntity>();
                cfg.CreateMap<EmployeeEntity, EmployeeListViewModel>();
                cfg.CreateMap<AddEditEmployeeViewModel, AddEmployeeDataType>();
                cfg.CreateMap<AddEditUserViewModel, UserEntity>();
                cfg.CreateMap<EmployeeEntity, AddEditEmployeeViewModel>();

                cfg.CreateMap<CategoryEntity, DocumentChartModel>()
                   .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name != null ? src.Name : ""));

                cfg.CreateMap<CustomerDocumentMappingEntity, CustomerDocumentMappingModel>()
                .ForMember(dest => dest.AttachmentName, opt => opt.MapFrom(src => src.Picture != null ? src.Picture.DisplayName : ""))
                .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.DocumentNumber : ""));
                cfg.CreateMap<CustomerDocumentMappingModel, CustomerDocumentMappingEntity>();

                cfg.CreateMap<DocumentEmailDataEntity, DocumentEmailDataModel>();
                cfg.CreateMap<DocumentEmailDataModel, DocumentEmailDataEntity>();

                cfg.CreateMap<AddEditRoleModel, RoleEntity>();

                cfg.CreateMap<RoleEntity, AddEditRoleModel>();

                cfg.CreateMap<RoleEntity, SelectListModel>();
                cfg.CreateMap<RoleEntity, StructureRoleSelectListModel>();
                cfg.CreateMap<AddEditRolePermissionModel, RolePermissionEntity>();


                // Header
                cfg.CreateMap<CPRMasterEntity, CPRMasterModel>()
                 //.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryEntity != null ? src.CategoryEntity.Name : null))
                 //.ForMember(dest => dest.DocumentTitle, opt => opt.MapFrom(src => src.DocumentEntity != null ? src.DocumentEntity.Name : null))
                 //.ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedByEntity != null ? src.RequestedByEntity.EmployeeName : null))
                 .ForMember(dest => dest.ApprovalsList, opt => opt.MapFrom(src => src.ApprovalsEntity != null && src.ApprovalsEntity.Count() > 0 ? src.ApprovalsEntity : null));
                cfg.CreateMap<CPRMasterModel, CPRMasterEntity>();

                // Approvers
                cfg.CreateMap<CPRMasterApproverDetailEntity, CPRMasterApproverDetailModel>()
                    //.ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => src.User != null ? src.User.EmployeeName : null))
                    //.ForMember(dest => dest.ApprovalStatusName, opt => opt.MapFrom(src => src.CPRApprovalStatus != null ? src.CPRApprovalStatus.Name : null))
                    .ReverseMap();
            });

            return config;
        }


    }
}
