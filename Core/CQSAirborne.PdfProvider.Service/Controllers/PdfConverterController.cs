using GleamTech.DocumentUltimate;
using GleamTech.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Filters;


namespace CQS.PdfProvider.Service.Controllers
{
    [RoutePrefix("api/PdfConverter")]
    [CustomExceptionHandler]
    public class PdfConverterController : ApiController
    {
        private readonly Dictionary<string, Func<string, Task<byte[]>>> _fileDownloadStrategies;
        public PdfConverterController()
        {
            _fileDownloadStrategies = new Dictionary<string, Func<string, Task<byte[]>>>
            {
                { ".xls", GetExcelPdfFileStream },
                { ".xlsx", GetExcelPdfFileStream },
                { ".doc", GetDocumentPdfFileStream },
                { ".docx", GetDocumentPdfFileStream },
                { ".jpg", GetImagePdfFileStream },
                { ".png", GetImagePdfFileStream },
                { ".jpeg", GetImagePdfFileStream },
                { ".bmp", GetImagePdfFileStream },
                { ".ppt", GetSlidePdfFileStream },
                { ".pptx", GetSlidePdfFileStream },
            };
        }

        private async Task<byte[]> GetSlidePdfFileStream(string path)
        {
            string inputFile = path;
            string outputFile = path.Replace(".pptx", ".pdf").Replace(".ppt", ".pdf");

            if (!File.Exists(outputFile))
            {
                var result = DocumentConverter.Convert(new GleamTech.IO.BackSlashPath(inputFile), DocumentFormat.Pdf);

                //using (var converter = new OfficeConverter.Converter())
                //{
                //    converter.Convert(inputFile, outputFile);
                //}
            }
            return await GetPdfFileStream(outputFile);
        }

        private async Task<byte[]> GetPdfFileStream(string path)
        {
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            return memory.ToArray();
        }

        private async Task<byte[]> GetDocumentPdfFileStream(string path)
        {
            string inputFile = path;
            string outputFile = path.Replace(".docx", ".pdf").Replace(".doc", ".pdf");

            if (!File.Exists(outputFile))
            {
                DocumentConverter.Convert(new BackSlashPath(inputFile), DocumentFormat.Pdf);
                //using (var converter = new OfficeConverter.Converter())
                //{
                //    converter.Convert(inputFile, outputFile);
                //}
            }
            return await GetPdfFileStream(outputFile);
        }

        private async Task<byte[]> GetExcelPdfFileStream(string path)
        {
            string inputFile = path;
            string outputFile = path.Replace(".xlsx", ".pdf").Replace(".xls", ".pdf");

            if (!File.Exists(outputFile))
            {
                //using (var converter = new OfficeConverter.Converter())
                //{
                //    converter.Convert(inputFile, outputFile);
                //}
            }
            return await GetPdfFileStream(outputFile);
        }

        private async Task<byte[]> GetImagePdfFileStream(string path)
        {
            string inputFile = path;
            string outputFile = path.Replace(".jpg", ".pdf")
                .Replace(".png", ".pdf")
                .Replace(".bmp", ".pdf")
                .Replace(".jpeg", ".pdf")
                ;

            //if (!System.IO.File.Exists(outputFile))
            //{
            //    Document doc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
            //    Image jpg = Image.GetInstance(inputFile);
            //    //Resize image depend upon your need

            //    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outputFile, FileMode.Create));

            //    doc.Open();
            //    jpg.ScaleToFit(300f, 300f);
            //    jpg.SpacingBefore = 10f;
            //    jpg.SpacingAfter = 1f;
            //    jpg.Alignment = Element.ALIGN_LEFT;
            //    doc.Add(jpg);
            //    doc.Close();
            //}
            return await GetPdfFileStream(outputFile);
        }



        [HttpPost]
        [Route("Convert")]
        public async Task<HttpResponseMessage> Convert()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string basePath = HostingEnvironment.MapPath("~/Uploads");

            var httpRequest = HttpContext.Current.Request;
            var postedFile = httpRequest.Files[0];


            string filePath = Path.Combine(basePath, Guid.NewGuid().ToString() + Path.GetExtension(postedFile.FileName));
            postedFile.SaveAs(filePath);

            string pdfPath = Path.Combine(basePath, Path.GetFileNameWithoutExtension(filePath)) + ".pdf";
            DocumentConverter.Convert(new BackSlashPath(filePath), new BackSlashPath(pdfPath), DocumentFormat.Pdf);
            var bytes = await GetPdfFileStream(pdfPath);

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            result.Content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            return result;

        }

        [HttpGet]
        [Route("TestMethod")]
        public IHttpActionResult TestMethod()
        {
            return Ok(new List<string> { "asafsd", "asdfasdf" });
        }


    }

    public class CustomExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            string message = actionExecutedContext.Exception.Message;
            base.OnException(actionExecutedContext);
        }
    }

}
