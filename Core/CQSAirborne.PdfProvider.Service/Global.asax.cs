using GleamTech.DocumentUltimate;
using GleamTech.DocumentUltimate.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CQS.PdfProvider.Service
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            //Set this property only if you have a valid license key, otherwise do not
            //set it so DocumentUltimate runs in trial mode.
            DocumentUltimateConfiguration.Current.LicenseKey = ConfigurationManager.AppSettings["DocumentUltimateLicenseKey"];

            //The default CacheLocation value is "~/App_Data/DocumentCache"
            //Both virtual and physical paths are allowed (or a Location instance for one of the supported 
            //file systems like Amazon S3 and Azure Blob).
            //DocumentUltimateWebConfiguration.Current.CachePath = ConfigurationManager.AppSettings["DocumentUltimateCacheLocation"];
        }
        
    }
}
