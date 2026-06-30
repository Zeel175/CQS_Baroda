using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CQSAirborne.Web.Infrastructure.Models
{
    public class RestRequest
    {
        public RestRequest(string resourceUrl, RestMethodType httpMethod = RestMethodType.Get, string token = "", bool isAnonymousRequest = false)

        {

            Parameters = new Dictionary<string, string>();

            QueryStrings = new Dictionary<string, string>();
            ResourceUrl = resourceUrl;
            Method = httpMethod;
            Token = token;
        }

        public RestMethodType Method { get; }
        public bool IsAnonymousRequest { get; }
        public string ResourceUrl { get; }
        public Dictionary<string, string> Parameters { get; }
        public Dictionary<string, string> QueryStrings { get; }
        public string Token { get; set; }
        public void AddParameter(string parameterName, string value)
        {
            Parameters.Add(parameterName, value);
        }

        public void AddQueryString(string parameterName, string value)
        {
            QueryStrings.Add(parameterName, value);
        }
    }

    public class RestRequest<T> : RestRequest
        where T : class
    {
        public RestRequest(string resourceUrl, RestMethodType httpMethod, T body, string token= "", bool isAnonymousRequest = false)
            : base(resourceUrl, httpMethod, token, isAnonymousRequest)
        {
            Body = body;
        }
        public T Body { get; }
    }
}
