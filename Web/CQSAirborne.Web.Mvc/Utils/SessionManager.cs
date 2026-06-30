using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Utils
{
    public class SessionManager: ISessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<T> Get<T>(string key)
            where T : class
        {
            if (_httpContextAccessor.HttpContext.Session.Keys.Contains(key))
            {
                return JsonConvert.DeserializeObject<List<T>>(_httpContextAccessor.HttpContext.Session.GetString(key));
            }
            return new List<T>();
        }

        public void Set<T>(string key, T model)
            where T : class
        {
            _httpContextAccessor.HttpContext.Session.SetString(key, JsonConvert.SerializeObject(model));
        }

        public void AddToCollection<T>(string key, T model)
            where T : class
        {
            if (!_httpContextAccessor.HttpContext.Session.Keys.Contains(key))
            {
                throw new Exception("Collection not found in session");
            }
            var collection = Get<T>(key);
            collection.Add(model);
            Set(key, collection);
        }
        public string GetToken()
        {
            var token = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).GetSpecificClaim("AuthToken");
            return token;
        }

        public string GetClaim(string claimname)
        {
            var token = ((ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity).GetSpecificClaim(claimname);
            return token;
        }
    }
}
