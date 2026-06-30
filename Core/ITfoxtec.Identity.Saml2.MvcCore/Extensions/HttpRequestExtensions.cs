using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;

namespace ITfoxtec.Identity.Saml2.MvcCore
{
    /// <summary>
    /// Extension methods for HttpRequest
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Converts a Microsoft.AspNet.Http.HttpRequest to ITfoxtec.Identity.Saml2.Http.HttpRequest.
        /// </summary>
        public static Http.HttpRequest ToGenericHttpRequest(this HttpRequest request, string SAMLResposne = "")
        {
            return new Http.HttpRequest
            {
                Method = request.Method,
                QueryString = request.QueryString.Value,
                Query = ToNameValueCollection(request.Query, SAMLResposne),
                Form = "POST".Equals(request.Method, StringComparison.InvariantCultureIgnoreCase) ? ToNameValueCollection(request.Form, SAMLResposne) : null,
            };
        }

        private static NameValueCollection ToNameValueCollection(IEnumerable<KeyValuePair<string, StringValues>> items, string SAMLResposne)
        {
            var nv = new NameValueCollection();
            foreach (var item in items)
            {
                nv.Add(item.Key, item.Value.First());
            }

            if(SAMLResposne!="")
            {
                nv.Add("SAMLResponse", SAMLResposne);
            }

            return nv;
        }
    }
}
