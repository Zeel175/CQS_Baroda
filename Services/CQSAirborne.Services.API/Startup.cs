using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CQSAirborne.Dependencies;
using CQSAirborne.Model;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.Plant;
using CQSAirborne.Model.Validator;
using CQSAirborne.Services.API.Models;
using CQSAirborne.Services.API.Utils;
using CQSAirborne.Services.API.Validators;
using CQSAirborne.Services.Contract;
using CQSAirborne.Services.Contract.ADSync;
using CQSAirborne.Services.Implementation.ADSync;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Implementation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace CQSAirborne.Services.API
{
    public class Startup
    {
        private readonly ILogger _logger;
        public Startup(IConfiguration configuration, ILoggerFactory logFactory)
        {
            Configuration = configuration;
            _logger = logFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                _logger.LogInformation("........Configure Services start.........");
                services.AddCors();
                //services.AddMvc()
                //    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                //    .AddFluentValidation()
                //    ;

                _logger.LogInformation("........Configure Services start --- 1 .........");
                services.Configure<ApiBehaviorOptions>(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });
                services.Configure<IISOptions>(options =>
                {
                    options.ForwardClientCertificate = false;
                });
                _logger.LogInformation("........Configure Services start --- 2 .........");
                
                services.AddTransient<IValidator<LoginViewModel>, LoginViewModelValidator>();
                services.AddTransient<IValidator<AddEditPlantModel>, AddEditPlantModelExtendValidator>();
                services.AddTransient<IValidator<AddEditDocumentModel>, AddEditDocumentModelExtendValidator>();
                services.AddTransient<IValidator<AddEditCategoryModel>, AddEditCategoryModelValidatorExtend>();

              
                _logger.LogInformation("........Configure Services start --- 3 .........");
                try
                {
                    string webPortalPath = Configuration.GetValue<string>("CustomerWebPath");
                    services.AddTransient<IRestClient, CQSRestClient>((s) => new CQSRestClient(webPortalPath));
                }
                catch { }


                services.AddTransient<IIdentityHelper, IdentityHelper>();

                services.AddControllers().AddNewtonsoftJson();
                RepositoryModule.Register(services);
                ServiceModule.Register(services);
                DataModule.Register(Configuration, services);
                services.AddTransient<IADUserSyncService, ADUserSyncService>();

                services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection")
                        ?? Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                    {
                        PrepareSchemaIfNecessary = true,
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        UsePageLocksOnDequeue = true,
                        DisableGlobalLocks = true,
                    }));
                services.AddHangfireServer();

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QMS Web API", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header using the Bearer scheme."
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
                });

                _logger.LogInformation("........Configure Services start --- 4 .........");

                //services.ConfigureSwaggerGen(options => { options.OperationFilter<AuthorizationHeaderParameterOperationFilter>(); });
               
                _logger.LogInformation("........Configure Services start --- 6 .........");

                // configure strongly typed settings objects
                var appSettingsSection = Configuration.GetSection("AppSettings");
                services.Configure<AppSettings>(appSettingsSection);

                // configure jwt authentication
                var appSettings = appSettingsSection.Get<AppSettings>();
                // ✅ Add this line just below AppSettings
                services.Configure<SpecialPlantEmailConfig>(
                    Configuration.GetSection("SpecialPlantEmailConfig"));

                services.AddTransient<AppSettings>(w => appSettings);
                

                var key = Encoding.ASCII.GetBytes(appSettings.Secret);
                services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(0)
                    };
                });
                _logger.LogInformation("........Configure Services start --- 8 .........");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex + "........Configure Services End.........");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.CreateLogger("Logs/mylog-{Date}.txt");
            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseStaticFiles();


            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            app.UseHangfireDashboard("/scheduler", new DashboardOptions
            {
                AppPath = "/swagger"
            });

            RecurringJob.AddOrUpdate<IADUserSyncService>(
                "ad-user-sync-job",
                service => service.SyncNewUsersAsync(),
                Configuration.GetValue<string>("ADUserSync:CronExpression") ?? "*/5 * * * *");

            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var errorLogService = context.RequestServices.GetService<IErrorLogService>();
                        errorLogService.Insert(new Model.ErrorLog.AddEditErrorLogModel
                        {
                            ApplicationName = "WEB API",
                            ErrorMessage = contextFeature.Error.Message,
                            StackTrace = contextFeature.Error.StackTrace
                        });
                    }
                });
            });


            app.UseRouting();

            app.UseAuthentication();  // ✅ must come before authorization
            app.UseAuthorization();   // ✅ add this line

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        public class AuthorizationHeaderParameterOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var hasAuthorize = context.MethodInfo
                    .DeclaringType
                    ?.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any() == true
                    || context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any();

                if (!hasAuthorize)
                    return;

                operation.Parameters ??= new List<OpenApiParameter>();

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Access token (Bearer {token})",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}
