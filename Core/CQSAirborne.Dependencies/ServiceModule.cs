using AutoMapper;
using CQS.Services.Implementation;
using CQSAirborne.Services.Contract;
using CQSAirborne.Services.Implementation;
using CQSAirborne.Services.Implementation.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Dependencies
{
    public class ServiceModule
    {
        public static void Register(IServiceCollection serviceCollection)
        {
            //MappingConfig.Register();
            // ✅ AutoMapper 13+ registration — automatically loads MappingConfig : Profile
            IMapper mapper = MappingConfig.Register().CreateMapper();
            serviceCollection.AddSingleton(mapper);

            // ✅ Register application services
            serviceCollection.AddTransient<IDataMapper, AutomapperDataMapper>();

            serviceCollection.AddTransient<IUserService, UserService>();
            serviceCollection.AddTransient<IPlantService, PlantService>();
            serviceCollection.AddTransient<ICategoryService, CategoryService>();
            serviceCollection.AddTransient<IDocumentService, DocumentService>();
            serviceCollection.AddTransient<IDashboardService, DashboardService>();
            serviceCollection.AddTransient<IExternalLinkService, ExternalLinkService>();
            serviceCollection.AddTransient<IErrorLogService, ErrorLogService>();
            serviceCollection.AddTransient<IEmployeeService, EmployeeService>();
            serviceCollection.AddTransient<EmailHelper>();

            serviceCollection.AddTransient<IDataMapper, AutomapperDataMapper>();
            serviceCollection.AddTransient<ILdapService, LdapService>();
            serviceCollection.AddTransient<ICustomerPortalService, CustomerPortalService>();
            serviceCollection.AddTransient<ICustomerService, CustomerService>();
            serviceCollection.AddTransient<IRoleService, RoleService>();

            serviceCollection.AddTransient<ICPRMasterService, CPRMasterService>();
        }
    }
}
