using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Threading.Tasks;
using CQSAirborne.Model;
using CQSAirborne.Services.Contract;
using System.Linq;
using CQSAirborne.Repository.Contract;

namespace CQSAirborne.Services.Implementation
{
    public class LdapService : ILdapService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeRepository _employeeRepository;
        public LdapService(IEmployeeService employeeService, IEmployeeRepository employeeRepository)
        {
            _employeeService = employeeService;
            _employeeRepository = employeeRepository;
        }
        /// <summary>
        /// Authenticating user with LDAP and Azre Directory
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>true if the credentials are valid, false otherwise</returns>
        public LoginResult LoginLdap(LoginViewModel loginViewModel, string type)

        {
            var loginresult = new LoginResult();
            try
            {
                #region OldPorts

                //string strServerName = type=="TASLMain"?"10.10.10.191": "10.10.10.192";
                //string strBaseDN = "DC=tadl,DC=com";
                //string strUserDN = "OU=Users";
                //string strGroupName = "CN=Operater, OU=Groups";
                //string strAccountFilter = "sAMAccountName";
                //string strPortNo = "5389";
                //Boolean blnGroupUser = false;

                //if(type == "TADLMain")
                //{
                //    strServerName = "10.10.10.223";
                //    strPortNo = "50001";
                //}


                //if (type == "TADLAirborneNewServer")
                //{
                //    strServerName = "10.60.106.77";
                //    strPortNo = "389";
                //}
                //if (type == "TADLAirborneTADLNewServer")
                //{
                //    strServerName = "10.60.106.77";
                //    strPortNo = "50001";
                //}
                //if (type == "TADLAirborneOtherNewServer")
                //{
                //    strServerName = "10.60.106.77";
                //    strPortNo = "5389";
                //}


                //if (type == "TADLAirborne")
                //{
                //    strServerName = "10.60.106.60";
                //    strPortNo = "389";
                //}
                //if (type == "TADLAirborneTADL")
                //{
                //    strServerName = "10.60.106.60";
                //    strPortNo = "50001";
                //}
                //if (type == "TADLAirborneOther")
                //{
                //    strServerName = "10.60.106.60";
                //    strPortNo = "5389";
                //}

                #endregion

                string strServerName = "10.120.20.10";
                string strBaseDN = "DC=tadl,DC=com";
                string strUserDN = "OU=Users";
                string strGroupName = "CN=Operater, OU=Groups";
                string strAccountFilter = "sAMAccountName";
                string strPortNo = "389";
                bool blnGroupUser = false;

                switch (type)
                {
                    case "LDAPSSL":
                        strServerName = "10.120.20.10";
                        strPortNo = "636";
                        break;

                    case "LDAP":
                        strServerName = "10.120.20.10";
                        strPortNo = "389";
                        break;

                    case "TADLMain":
                        strServerName = "10.10.10.223";
                        strPortNo = "50001";
                        break;

                    default:
                        throw new Exception($"Invalid LDAP type: {type}");
                }

                var hostname = $"{strServerName}:{strPortNo}";

                bool result = true;
                var exc = "";
                var credentials = new NetworkCredential(loginViewModel.UserName, loginViewModel.Password);
                var serverId = new LdapDirectoryIdentifier(hostname);

                var conn = new LdapConnection(serverId, credentials, AuthType.Negotiate);
                try
                {
                    conn.Bind();
                    var userData = _employeeService.GetAllActive().Where(m => m.ADID == loginViewModel.UserName).FirstOrDefault();
                    if (userData != null)
                    {
                        loginresult.IsSuccess = true;
                        loginresult.Claims.Add("userName", userData.ADID);
                        loginresult.Claims.Add("name", userData.EmployeeName);
                        loginresult.Claims.Add("UserId", userData.Id.ToString());
                        loginresult.OrgRole = userData.OrgRole;
                        loginresult.Id = userData.Id;
                        loginresult.EmpName = userData.EmployeeName;
                        return loginresult;
                    }
                    loginresult.Error = "Credentials are invalid";

                }
                catch (Exception ex)
                {
                    loginresult.Error = ex.Message;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                loginresult.Error = "Credentials are invalid";
            }
            return loginresult;
        }

        public LoginResult CheckSSOAuthentication(string authenticationId)

        {
            var loginresult = new LoginResult();
            try
            {

                var userData2 = _employeeRepository.GetAll().Where(m => m.ADID == authenticationId).FirstOrDefault();
                if (userData2 != null)
                {
                    loginresult.IsSuccess = true;
                    loginresult.Claims.Add("userName", userData2.ADID);
                    loginresult.Claims.Add("name", userData2.EmployeeName);
                    loginresult.OrgRole = userData2.OrgRole;
                    loginresult.EmpName = userData2.EmployeeName;
                    return loginresult;
                }
                else
                {
                    loginresult.IsSuccess = false;
                }

                loginresult.Error = "Credentials are invalid";

                if (authenticationId == "-1")
                {

                    string type = "TADLMain";
                    string strServerName = type == "TASLMain" ? "10.10.10.191" : "10.10.10.192";
                    //string strBaseDN = "DC=TASLAERO,DC=COM";
                    //string strUserDN = "OU=Users";
                    //string strGroupName = "CN=Operater, OU=Groups";
                    //string strAccountFilter = "sAMAccountName";
                    string strPortNo = "5389";
                    //Boolean blnGroupUser = false;
                    if (type == "TADLMain")
                    {
                        strServerName = "10.10.10.223";
                        strPortNo = "50001";
                    }

                    var hostname = $"{ strServerName }:{strPortNo}";
                    //Search for user
                    //DirectoryEntry deSystem = new DirectoryEntry($"LDAP://{ strServerName }:{strPortNo}/{strBaseDN}");
                    //deSystem.AuthenticationType = AuthenticationTypes.Secure;
                    //deSystem.Username = username;
                    //deSystem.Password = password;
                    ////Search for account name
                    //string strSearch = string.Format("(&(objectClass=user)(objectCategory=user) (sAMAccountName={0}))", username); ;
                    //DirectorySearcher dsSystem = new DirectorySearcher(deSystem,strSearch);

                    ////Search subtree of UserDN
                    //dsSystem.SearchScope = System.DirectoryServices.SearchScope.Subtree;
                    ////Find the user data
                    //SearchResult srSystem = dsSystem.FindOne();
                    ////Pick up the user group belong to
                    //ResultPropertyValueCollection valcol = srSystem.Properties["memberOf"];
                    //if (valcol.Count > 0)
                    //{
                    //    foreach (object o in valcol)
                    //    {
                    //        //check user exist in Group we are searching for
                    //        if (o.ToString().Equals(strGroupName + "," + strBaseDN))
                    //        {
                    //            blnGroupUser = true;
                    //            break;
                    //        }
                    //    }
                    //}
                    bool result = true;
                    var exc = "";
                    var credentials = new NetworkCredential();//No Parameters
                    var serverId = new LdapDirectoryIdentifier(hostname);

                    var conn = new LdapConnection(serverId, credentials, AuthType.Negotiate);
                    try
                    {
                        //conn.Bind();

                        var users = _employeeService.GetAllActive();
                        var userData = users.Where(m => m.ADID == authenticationId).FirstOrDefault();

                        if (userData != null)
                        {
                            loginresult.IsSuccess = true;
                            loginresult.Claims.Add("userName", userData.ADID);
                            loginresult.Claims.Add("name", userData.EmployeeName);
                            loginresult.OrgRole = userData.OrgRole;
                            return loginresult;
                        }
                        loginresult.Error = "Credentials are invalid";

                    }
                    catch (Exception ex)
                    {
                        loginresult.Error = ex.Message;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                loginresult.Error = "Credentials are invalid";
            }
            return loginresult;
        }

    }
}
