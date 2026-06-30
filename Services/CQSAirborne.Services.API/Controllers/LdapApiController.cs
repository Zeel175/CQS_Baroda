using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Mvc;
namespace CQSAirborne.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LdapApiController : BaseController
    {
        public readonly ILdapService _ldapService;
        public LdapApiController(ILdapService ldapService)
        {
            _ldapService = ldapService;
        }
        /// <summary>
        /// API to login throgh LDAP and AD
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Authorized or Not</returns>
        [HttpPost("[action]")]
        public IActionResult Login(string username, string password)
        {
            //var isValid = _ldapService.GetADSILogin(username, password);
            //if (isValid)
            //    return Ok();
            //else
                return Unauthorized();
        }

        
    }
}