using CQSAirborne.Repository.Contract;
using CQSAirborne.Repository.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Dependencies
{
    public class RepositoryModule
    {
        public static void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IUserRepository, UserRepository>();
            serviceCollection.AddTransient<IPlantRepository, PlantRepository>();
            serviceCollection.AddTransient<ICodeMaintainRepository, CodeMaintainRepository>();
            serviceCollection.AddTransient<IGroupCodesRepository, GroupCodesRepository>();
            serviceCollection.AddTransient<ICategoryRepository, CategoryRepository>();
            serviceCollection.AddTransient<IDocumentRepository, DocumentRepository>();
            serviceCollection.AddTransient<IExternalLinkRepository, ExternalLinkRepository>();
            serviceCollection.AddTransient<IPictureRepository, PictureRepository>();
            serviceCollection.AddTransient<IDocumentPlantRepository, DocumentPlantRepository>();
            serviceCollection.AddTransient<IErrorLogRepository, ErrorLogRepository>();
            serviceCollection.AddTransient<IEmployeeRepository, EmployeeRepository>();
            serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddTransient<IDocumentTagsRepository, DocumentTagsRepository>();
            serviceCollection.AddTransient<ICustomerRepository, CustomerRepository>();
            serviceCollection.AddTransient<ICustomerDocumentMappingRepository, CustomerDocumentMappingRepository>();
            serviceCollection.AddTransient<IDocumentEmailDataRepository, DocumentEmailDataRepository>();
            serviceCollection.AddTransient<IEmailHistoryRepository, EmailHistoryRepository>();
            serviceCollection.AddTransient<IRoleRepository, RoleRepository>();
            serviceCollection.AddTransient<IPermissionRepository, PermissionRepository>();
            serviceCollection.AddTransient<IRolePermissionRepository, RolePermissionRepository>();

            serviceCollection.AddTransient<ICPRMasterRepository, CPRMasterRepository>();
            serviceCollection.AddTransient<ICPRMasterApproverRepository, CPRMasterApproverRepository>();
        }
    }
}
