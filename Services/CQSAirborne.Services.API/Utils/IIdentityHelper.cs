using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CQSAirborne.Services.API.Utils
{
    public interface IIdentityHelper
    {
        int UserId { get; }
    }

    public class IdentityHelper : IIdentityHelper
    {
        public int UserId
        {
            get
            {
                return 1;
            }
        }
    }

    public static class IdentityExtensions
    {
        public static int GetUserId(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.FindFirst("UserId");

            if (claim == null)
                return 0;

            return int.Parse(claim.Value);
        }
    }

}
