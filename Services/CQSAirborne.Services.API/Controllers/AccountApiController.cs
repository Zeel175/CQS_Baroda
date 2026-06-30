using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CQSAirborne.Model;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Employee;
using CQSAirborne.Services.API.Models;
using CQSAirborne.Services.Contract;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static CQSAirborne.Model.Core.Constants;

namespace CQSAirborne.Services.API.Controllers
{
    [Route("api/Account")]
    [ApiController]
    public class AccountApiController : BaseController
    {
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;
        private readonly IConfiguration _configuration;
        public readonly ILdapService _ldapService;
        public readonly IEmployeeService _employeeService;
        public readonly IRoleService _roleService;
        public AccountApiController(IUserService userService
            , AppSettings appSettings
            , IConfiguration configuration
            , ILdapService ldapService
            , IEmployeeService employeeService,
            IRoleService roleService)
        {
            _userService = userService;
            _appSettings = appSettings;
            _configuration = configuration;
            _ldapService = ldapService;
            _employeeService = employeeService;
            _roleService = roleService;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            var userResponse = _userService.LoginUserAsync(loginViewModel);
            if (!userResponse.IsSuccess)
            {
                return BadRequest(AddValidation("", userResponse.Error));
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var expiresAt = DateTime.UtcNow.AddDays(7);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userResponse.Claims.Select(w => new Claim(w.Key, w.Value)).ToArray()),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenResponse = tokenHandler.WriteToken(token);


            var permissionData = await _roleService.GetPermissionByType(PermissionScreenType.Role, userResponse.OrgRole, 0);


            return Ok(new LoginResponse
            {
                Token = tokenResponse,
                ExpireAt = expiresAt,
                Permissions = permissionData
            });
        }

        [HttpPost("[action]")]
        public IActionResult ActiveDirectoryLogin(LoginViewModel loginViewModel)
        {
            using (var context = new PrincipalContext(ContextType.Domain, _configuration.GetValue<string>("LDAPDomain")))
            {
                if (context.ValidateCredentials(loginViewModel.UserName, loginViewModel.Password))
                {
                    using (var de = new DirectoryEntry(_configuration.GetValue<string>("LDAPPath")))
                    using (var ds = new DirectorySearcher(de))
                    {
                        // other logic to verify user has correct permissions
                        return Ok(new { Message = "User logged in successfully" });
                    }
                }
            }
            return BadRequest(new { Message = "Unable to login the user.." });

        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> LoginLdap([FromBody] LoginViewModel loginViewModel)
        {
            string type = "LDAPSSL";
            var userResponse = _ldapService.LoginLdap(loginViewModel, type);
            if (!userResponse.IsSuccess)
            {
                type = "LDAP";
                userResponse = _ldapService.LoginLdap(loginViewModel, type);
                if (!userResponse.IsSuccess)
                {
                    type = "TADLMain";
                    userResponse = _ldapService.LoginLdap(loginViewModel, type);
                    if (!userResponse.IsSuccess)
                    {

                        userResponse = _userService.LoginUserAsync(loginViewModel);
                        if (!userResponse.IsSuccess)
                        {
                            return BadRequest(AddValidation("", userResponse.Error));
                        }
                    }

                }
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var expiresAt = DateTime.UtcNow.AddDays(7);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userResponse.Claims.Select(w => new Claim(w.Key, w.Value)).ToArray()),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenResponse = tokenHandler.WriteToken(token);

            var permissionData = await _roleService.GetPermissionByType(PermissionScreenType.Role, userResponse.OrgRole, 0);

            return Ok(new LoginResponse
            {
                Id = userResponse.Id,
                Token = tokenResponse,
                ExpireAt = expiresAt,
                OrgRole = userResponse.OrgRole,
                UserName = loginViewModel.UserName,
                EmpName = userResponse.EmpName,
                Permissions = permissionData
            });
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var vvm = new LoginViewModel()
            {
                UserName = model.UserName,
                Password = model.OldPassword
            };
            var message = new AccountResponseViewModel() { };
            var isCurrentPasswordTrue = _userService.LoginUserAsync(vvm);
            if (isCurrentPasswordTrue.IsSuccess)
            {
                var response = await _employeeService.ChangePassword(model.Password, model.UserName);
                message.Message = "Your password is changed.";
                message.IsFailed = false;
            }
            else
            {
                message.Message = "Your Old Password is invalid. Please try with currect old password.";
                message.IsFailed = false;
            }
            return Ok(message);
        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var response = await _employeeService.ResetPassword(model);
            return Ok(response);
        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult ForgotPassword(string Email)
        {
            var response = _userService.ForgotPassword(Email);
            return Ok(response);
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult IsUserEmailAvailable(string UserName, long? Id)
        {
            return new JsonResult(_userService.IsUserEmailAvailable(UserName, Id));
        }


        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateSSOAuthentication(SSORequest model)
        {
            if (model.secretKey != SSOSecretKey.key)
            {
                return BadRequest();
            }

            if (model.IsDirect)
            {
                var tokenHandler1 = new JwtSecurityTokenHandler();
                var key1 = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var expiresAt1 = DateTime.UtcNow.AddDays(7);

                var logResponse = new LoginResult();
                logResponse.Claims.Add("userName", model.userName);
                logResponse.Claims.Add("name", model.name);

                var tokenDescriptor1 = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(logResponse.Claims.Select(w => new Claim(w.Key, w.Value)).ToArray()),
                    Expires = expiresAt1,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key1), SecurityAlgorithms.HmacSha256Signature)
                };
                var token1 = tokenHandler1.CreateToken(tokenDescriptor1);
                string tokenResponse1 = tokenHandler1.WriteToken(token1);

                var permissionData1 = await _roleService.GetPermissionByType(PermissionScreenType.Role, model.OrgRole, 0);

                return Ok(new LoginResponse
                {
                    Token = tokenResponse1,
                    ExpireAt = expiresAt1,
                    OrgRole = model.OrgRole,
                    UserName = "XYZ",
                    Permissions = permissionData1
                });
            }

            var userResponse = _ldapService.CheckSSOAuthentication(model.authenticationId);
            if (!userResponse.IsSuccess)
            {
                return BadRequest(AddValidation("", userResponse.Error));
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var expiresAt = DateTime.UtcNow.AddDays(7);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userResponse.Claims.Select(w => new Claim(w.Key, w.Value)).ToArray()),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenResponse = tokenHandler.WriteToken(token);

            var permissionData = await _roleService.GetPermissionByType(PermissionScreenType.Role, userResponse.OrgRole, 0);

            return Ok(new LoginResponse
            {
                Token = tokenResponse,
                ExpireAt = expiresAt,
                OrgRole = userResponse.OrgRole,
                UserName = "XYZ",
                Permissions = permissionData
            });
        }

    }
}