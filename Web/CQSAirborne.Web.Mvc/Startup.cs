using BotDetect.Web;
using CQSAirborne.Model;
using CQSAirborne.Model.Category;
using CQSAirborne.Model.Document;
using CQSAirborne.Model.ErrorLog;
using CQSAirborne.Model.Plant;
using CQSAirborne.Model.Validator;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Implementation;
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Web.Mvc.Utils;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
//using Saml2.Authentication.Core;
//using Saml2.Authentication.Core.Configuration;
//using Microsoft.AspNetCore.Identity;
using CQSAirborne.Services.API.Models;
using Microsoft.EntityFrameworkCore;
using CQSAirborne.Web.Mvc.Data;
using static CQSAirborne.Model.Core.Constants;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Util;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace CQSAirborne.Web.Mvc
{
    public class Startup
    {
        private IHostingEnvironment _hostingEnvironment;

        private readonly string _myOrigin = "AllowMyOrigin";

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            string crossOrogin = Configuration.GetValue<string>("Cross-origin");

            //services.AddCors(options =>
            //{
            //    options.AddPolicy(_myOrigin,
            //    builder => builder.WithOrigins(crossOrogin, " https://6ae43d2556ee.ngrok.io"));
            //});
            services.AddCors(options =>
            {
                options.AddPolicy(_myOrigin, builder =>
                    builder.WithOrigins(crossOrogin, "https://6ae43d2556ee.ngrok.io")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials());
            });

            services.AddCors();


            services.AddControllersWithViews().AddRazorRuntimeCompilation().AddNewtonsoftJson();
            services.AddRazorPages().AddRazorRuntimeCompilation().AddNewtonsoftJson();

            string apiPath = Configuration.GetValue<string>("BaseApiPath");
            int cacheMechanism = Configuration.GetValue<int>("CacheMechanism");

            services.AddTransient<IRestClient, CQSRestClient>((s) => new CQSRestClient(apiPath));
            services.AddTransient<AuthenticateService>();
            services.AddTransient<PlantService>();
            services.AddTransient<CategoryService>();
            services.AddTransient<DocumentService>();
            services.AddTransient<DashboardService>();
            services.AddTransient<ExternalLinkService>();
            services.AddTransient<EmployeeService>();
            services.AddTransient<CustomerService>();
            services.AddTransient<RoleService>();
            services.AddTransient<CPRMasterService>();

            //services.AddSession();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            services.AddSingleton<IFileProvider>(_hostingEnvironment.ContentRootFileProvider);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });


            //services.Configure<MvcOptions>(options =>
            //{
            //    options.Filters.Add(new CorsAuthorizationFilterFactory(_myOrigin));
            //});

            services.AddSession(options =>
            {
                //options.IdleTimeout = TimeSpan.FromMinutes(120);
                options.Cookie.IsEssential = true;
            });



            services.Configure<Saml2Configuration>(Configuration.GetSection("Saml2"));
            services.Configure<Saml2Configuration>(saml2Configuration =>
            {
                ///saml2Configuration.SignAuthnRequest = true;
                saml2Configuration.SigningCertificate = CertificateUtil.Load(_hostingEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SigningCertificateFile"]), Configuration["Saml2:SigningCertificatePassword"], X509KeyStorageFlags.MachineKeySet);



                //saml2Configuration.SignatureValidationCertificates.Add(CertificateUtil.Load(AppEnvironment.MapToPhysicalFilePath(Configuration["Saml2:SignatureValidationCertificateFile"])));
                saml2Configuration.AllowedAudienceUris.Add(saml2Configuration.Issuer);


                var entityDescriptor = new EntityDescriptor();
                //entityDescriptor.ReadIdPSsoDescriptorFromUrl(new Uri(Configuration["Saml2:IdPMetadataFile"]));
                entityDescriptor.ReadIdPSsoDescriptorFromFile(_hostingEnvironment.MapToPhysicalFilePath(Configuration["Saml2:IdPMetadataFile"]));
                if (entityDescriptor.IdPSsoDescriptor != null)
                {
                    saml2Configuration.AllowedIssuer = entityDescriptor.EntityId;

                    saml2Configuration.IDPCertificate = entityDescriptor.IdPSsoDescriptor.SigningCertificates.FirstOrDefault();

                    saml2Configuration.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
                    //saml2Configuration.SingleLogoutDestination = entityDescriptor.IdPSsoDescriptor.SingleLogoutServices.First().Location;
                    saml2Configuration.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);
                    if (entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.HasValue)
                    {
                        saml2Configuration.SignAuthnRequest = entityDescriptor.IdPSsoDescriptor.WantAuthnRequestsSigned.Value;
                    }
                }
                else
                {
                    throw new Exception("IdPSsoDescriptor not loaded from metadata.");
                }
            });

            services.AddSaml2(slidingExpiration: true);


            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(cookie =>
            //    {
            //        cookie.LoginPath = "/Account/Login";
            //        //cookie.ExpireTimeSpan = TimeSpan.FromMinutes(120);
            //        //cookie.LogoutPath = "/Account/LogOut";
            //    });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = LoginScheme.cookies;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
             .AddCookie(options =>
             {
                 options.LoginPath = "/Account/Login";
                 options.Cookie.HttpOnly = true;
                 options.Cookie.SameSite = SameSiteMode.None; //set Lax for Testing link
                 options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // set None for Testing link
             })
              .AddCookie(LoginScheme.cookies, options =>
              {
                  options.LoginPath = "/Account/Login";
                  options.Cookie.HttpOnly = true;
                  options.Cookie.SameSite = SameSiteMode.None; //set Lax for Testing link
                  options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // set None for Testing link
              });



            services.AddTransient<IValidator<LoginViewModel>, LoginViewModelValidator>();
            services.AddTransient<IValidator<AddEditPlantModel>, AddEditPlantModelValidator>();
            services.AddTransient<IValidator<AddEditDocumentModel>, AddEditDocumentModelValidator>();
            services.AddTransient<IValidator<AddEditCategoryModel>, AddEditCategoryModelValidator>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            if (cacheMechanism == 1)
            {
                services.AddTransient<ISessionManager, SessionManager>();
            }
            else if (cacheMechanism == 2)
            {
                services.AddTransient<ISessionManager, MemorySessionManager>();
            }

            //        services
            //.AddMvc(options =>
            //{
            //    options.EnableEndpointRouting = false;
            //})
            //.SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            //.AddFluentValidation();


            try
            {
                var processSuffix = "32bit";
                if (Environment.Is64BitProcess && IntPtr.Size == 8)
                {
                    processSuffix = "64bit";
                }

                CustomAssemblyLoadContext context = new CustomAssemblyLoadContext();
                context.LoadUnmanagedLibrary(Path.Combine(_hostingEnvironment.WebRootPath, $"PDFNative\\{processSuffix}\\libwkhtmltox"));

            }
            catch { }

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            //app.UseCors(_myOrigin);

            app.UseDeveloperExceptionPage();

            //app.UseHttpsRedirection(); //Commented for test link //Date : 25 June 2026
            //app.UseStaticFiles();
            //app.UseCookiePolicy();
            //app.UseSession();

            //app.UseForwardedHeaders(); // 🔥 REQUIRED


            //app.UseRouting();

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseForwardedHeaders();

            app.UseCors(_myOrigin);

            // app.UseHttpsRedirection(); // only if SSL is configured
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCaptcha(Configuration);

            //configure BotDetectCaptcha
            //app.UseCaptcha(Configuration);

            //RotativaConfiguration.Setup(env);  // Rotativa will look in wwwroot/Rotativa/wkhtmltopdf.exe

            string hostValue = Configuration.GetValue<string>("X-Forwarded-Host");

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Forwarded-Host", hostValue);
                await next();
            });

            //app.Use(async (ctx, next) =>
            //{
            //    //ctx.Response.Headers.Add("Content-Security-Policy","default-src 'self'; report-uri /home/cspreport");
            //    //ctx.Response.Headers.Add("Content-Security-Policy1", "style-src 'unsafe-inline'");
            //    //ctx.Response.Headers.Add("Content-Security-Policy1", "style-src 'unsafe-inline'");
            //    ctx.Response.Headers.Add(
            //   "Content-Security-Policy",
            //   "default-src 'self'; " +
            //   "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            //  "object-src blob: ;" +
            //  // "img-src 'self' blob:;" +
            //  "worker-src blob:;" +
            //   //"child-src blob: gap:;" +
            //   "img-src 'self' blob: data:;" +
            //   "style-src 'self' 'unsafe-inline';" //+
            //  //"style-src 'sha256-ozBpjL6dxO8fsS4u6fwG1dFDACYvpNxYeBA6tzR+FY8='"
            //   );
            //    ctx.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            //    ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            //    ctx.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
            //    ctx.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            //    await next();
            //});
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add(
                    "Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' blob: data:; " +
                    "connect-src 'self' https://localhost:44325 http://localhost:44325 wss://localhost:44325 ws://localhost:44325; " +
                    "object-src blob:; " +
                    "worker-src blob:; " +
                    "frame-ancestors 'self';"
                );

                ctx.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                ctx.Response.Headers.Add("X-Xss-Protection", "1; mode=block");
                ctx.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

                await next();
            });

            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    try
                    {
                        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (contextFeature != null)
                        {
                            var client = context.RequestServices.GetService<IRestClient>();
                            var restRequest = new RestRequest<AddEditErrorLogModel>("ErrorLog/Post", RestMethodType.Post, new AddEditErrorLogModel
                            {
                                ApplicationName = "CQS WEB",
                                ErrorMessage = contextFeature.Error.Message,
                                StackTrace = contextFeature.Error.StackTrace
                            });
                            //await client.ExecutePostAsync(restRequest);
                        }
                    }
                    catch
                    {
                    }
                });
            });

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public class ExpectionModel
        {
            public string Message { get; set; }
            public string Application { get; set; }
            public string StackTrace { get; set; }
        }
    }
}
