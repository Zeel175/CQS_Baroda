using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Utils
{
    public interface ISessionManager
    {
        List<T> Get<T>(string key)
            where T : class;
        void Set<T>(string key, T model)
            where T : class;
        void AddToCollection<T>(string key, T model)
            where T : class;
        string GetToken();

        string GetClaim(string claimname);
    }
}
