using CQSAirborne.Model.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CQSAirborne.Web.Infrastructure.Models
{
    public class RestReponse
    {
        public RestReponse()
        {
            Errors = new List<ValidationResultModel>();
        }
        public bool IsSuccess { get; set; }
        public HttpStatusCode Status { get; set; }
        public string Data { get; set; }
        public List<ValidationResultModel> Errors { get; set; }

    }

    public class RestReponse<T>
        where T : class
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode Status { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
    }

    public class RestFileReponse : RestReponse
    {
        public string FileName { get; set; }
        public byte[] File { get; set; }
        public string ContentType { get; set; }
    }

}
