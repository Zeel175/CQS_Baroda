using System.Linq;
using System.Security.Claims;

namespace CQSAirborne.Web.Mvc.Utils
{
    public static class ClamIdentityExtentions
    {   
        public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

            return (claim != null) ? claim.Value : string.Empty;
        }
        
    }
}
