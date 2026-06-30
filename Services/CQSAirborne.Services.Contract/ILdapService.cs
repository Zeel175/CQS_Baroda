using CQSAirborne.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface ILdapService
    {
        LoginResult LoginLdap(LoginViewModel loginViewModel, string type);
        LoginResult CheckSSOAuthentication(string authenticationId);
    }
}
