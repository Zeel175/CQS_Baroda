using CQSAirborne.Web.Infrastructure.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Security.Claims;

namespace CQSAirborne.Web.Mvc.Utils
{
    public class MemorySessionManager : ISessionManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public MemorySessionManager(IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
        {
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddToCollection<T>(string key, T model)
            where T : class
        {
            var collection = Get<T>(key);
            collection.Add(model);
            Set(key, collection);
        }

        public List<T> Get<T>(string key)
            where T : class
        {
            return _memoryCache.Get<List<T>>(key);
        }

        public void Set<T>(string key, T model)
            where T : class
        {
            _memoryCache.Set<T>(key, model);
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
