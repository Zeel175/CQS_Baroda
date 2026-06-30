using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CQSAirborne.Services.API
{
    public class Program
    {
        //   public static void Main(string[] args)
        //   {
        //       CreateWebHostBuilder(args).Build().Run();
        //   }

        //   //public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        //   //{
        //   //    return WebHost.CreateDefaultBuilder(args)
        //   //        //.UseKestrel()
        //   //        //.UseContentRoot(Directory.GetCurrentDirectory())
        //   //        //.UseIISIntegration()

        //   //        .UseStartup<Startup>();
        //   //}
        //   public static IWebHostBuilder CreateHostBuilder(string[] args) =>
        //WebHost.CreateDefaultBuilder(args)
        //.ConfigureLogging(logBuilder =>
        //{ DivideByZeroException.
        //     logBuilder.ClearProviders(); // removes all providers from LoggerFactory
        //    logBuilder.AddConsole();
        //    logBuilder.AddTraceSource("Information, ActivityTracing"); // Add Trace listener provider
        //})
        //.UseStartup<Startup>();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
     WebHost.CreateDefaultBuilder(args)
     .ConfigureLogging(logBuilder =>
     {
         logBuilder.ClearProviders(); // removes all providers from LoggerFactory
         logBuilder.AddConsole();
         //logBuilder.AddTraceSource("Information, ActivityTracing"); // Add Trace listener provider
     })
     .UseStartup<Startup>();

    }
}
