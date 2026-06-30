using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.MvcCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CQSAirborne.Web.Mvc.Identity;
using Microsoft.Extensions.Options;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Specialized;
using CQSAirborne.Web.Mvc.Services;
using CQSAirborne.Model;
using CQSAirborne.Web.Mvc.Utils;
using System.Security.Claims;
using static CQSAirborne.Model.Core.Constants;
using Microsoft.AspNetCore.Authentication;

namespace CQSAirborne.Web.Mvc.Controllers
{
    [AllowAnonymous]
    [Route("Auth")]
    public class AuthController : Controller
    {
        const string relayStateReturnUrl = "ReturnUrl";
        private readonly Saml2Configuration config;
        private readonly ILogger _logger;
        private readonly AuthenticateService _authenticateService;
        private readonly ISessionManager _sessionManager;

        public AuthController(IOptions<Saml2Configuration> configAccessor, ILogger<AuthController> logger,
            AuthenticateService authenticateService,
            ISessionManager sessionManager)
        {
            config = configAccessor.Value;
            _logger = logger;
            _authenticateService = authenticateService;
            _sessionManager = sessionManager;
        }

        [Route("Login")]
        public IActionResult Login(string returnUrl = null)
        {

            _logger.LogInformation("Login");
            _logger.LogError("LoginError");

            returnUrl = returnUrl ?? ($"{Request.Scheme}://{Request.Host}/home/index");
            var binding = new Saml2RedirectBinding();
            binding.SetRelayStateQuery(new Dictionary<string, string> { { relayStateReturnUrl, returnUrl ?? Url.Content("~/") } });

            return binding.Bind(new Saml2AuthnRequest(config)
            {
                //ForceAuthn = true,
                Subject = new Subject { NameID = new NameID { ID = "Role", Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName" } },
                NameIdPolicy = new NameIdPolicy { AllowCreate = true, Format = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress" },
                RequestedAuthnContext = new RequestedAuthnContext
                {
                    Comparison = AuthnContextComparisonTypes.Minimum,
                    AuthnContextClassRef = new string[] { AuthnContextClassTypes.PasswordProtectedTransport.OriginalString },
                },
            }).ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService()
        {


            try
            {
                var newReq = Request;
                string SAMLResponse = "";
                try
                {
                    using (var reader = new StreamReader(Request.Body))
                    {
                        var body = reader.ReadToEnd();

                        var strArray = body.Split('&');
                        var strArray1 = strArray[0];
                        var xval = strArray1.Replace("SAMLResponse=", "");
                        SAMLResponse = xval.Replace("%2B", "+").Replace("%3D", "=");

                    }
                }
                catch { }

                var binding = new Saml2PostBinding();
                var saml2AuthnResponse = new Saml2AuthnResponse(config);

                try
                {

                    binding.ReadSamlResponse(newReq.ToGenericHttpRequest(SAMLResponse), saml2AuthnResponse);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
                {
                    throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
                }
                binding.Unbind(newReq.ToGenericHttpRequest(SAMLResponse), saml2AuthnResponse);

                var principal = new ClaimsPrincipal(saml2AuthnResponse.ClaimsIdentity);

                if (principal.Identity == null || !principal.Identity.IsAuthenticated)
                {
                    throw new InvalidOperationException("No Claims Identity created from SAML2 Response.");
                }

                var nameidVlaue = principal.FindFirstValue("EmployeeName");

                _logger.LogError("nameValue " + nameidVlaue.ToString());

                var claimTranform = ClaimsTransform.Transform(principal);


                if (claimTranform.Identity.IsAuthenticated)
                {
                    var result = await loginHandler(nameidVlaue);
                    if (result)
                    {
                        return RedirectToAction("index", "home");
                    }
                }

                return RedirectToAction("login", "account");

            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("login", "account");

        }

        protected async Task<bool> loginHandler(string ADID)
        {
            var model = new SSORequest();
            model.secretKey = SSOSecretKey.key;

            model.authenticationId = ADID;

            model.userName = "xyx";
            model.name = "test";
            model.OrgRole = "Admin";
            model.IsDirect = false;

            var result = await _authenticateService.SSOAuthenticationAsync(model);
            if (result.IsSuccess && result.Data != null)
            {
                //await HttpContext.SignInAsync("saml2.cookies", info.Principal);

                var loginResult = result.Data;
                var claims = new List<Claim>
                {
                    new Claim("AuthToken", loginResult.Token),
                    new Claim("AuthExpiry", loginResult.ExpireAt.ToString()),
                    new Claim("OrgRole", loginResult.OrgRole.ToString()),
                    new Claim("Permission" ,  JsonConvert.SerializeObject(loginResult.Permissions)),
                    new Claim("IsExternal", "1")
                };

                //claims.AddRange(info.Principal.Claims);

                var claimsIdentity = new ClaimsIdentity(claims, LoginScheme.cookies);

                await HttpContext.SignInAsync(LoginScheme.cookies, new ClaimsPrincipal(claimsIdentity));

                return true;
            }

            return false;
        }

        [AllowAnonymous]
        [Route("AssertionConsumerService")]
        public async Task<IActionResult> AssertionConsumerService(string x = "", string b = "")
        {

            try
            {
                try
                {
                    _logger.LogError("SAML2 Response");
                    _logger.LogError("SAML2 Response 2", config.ToString());

                    _logger.LogError("SAML Response", Request.ToGenericHttpRequest().Form);
                }
                catch { }


                var binding = new Saml2PostBinding();
                var saml2AuthnResponse = new Saml2AuthnResponse(config);

                _logger.LogError(saml2AuthnResponse.Status.ToString(), saml2AuthnResponse);

                binding.ReadSamlResponse(Request.ToGenericHttpRequest(), saml2AuthnResponse);

                if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
                {
                    throw new AuthenticationException($"SAML Response status: {saml2AuthnResponse.Status}");
                }
                binding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnResponse);
                await saml2AuthnResponse.CreateSession(HttpContext, claimsTransform: (claimsPrincipal) => ClaimsTransform.Transform(claimsPrincipal));

                var relayStateQuery = binding.GetRelayStateQuery();
                var returnUrl = relayStateQuery.ContainsKey(relayStateReturnUrl) ? relayStateQuery[relayStateReturnUrl] : Url.Content("~/");
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect(Url.Content("~/"));
            }

            var binding = new Saml2PostBinding();
            var saml2LogoutRequest = await new Saml2LogoutRequest(config, User).DeleteSession(HttpContext);
            return binding.Bind(saml2LogoutRequest).ToActionResult();
        }

        [Route("LoggedOut")]
        public IActionResult LoggedOut()
        {
            var binding = new Saml2PostBinding();
            binding.Unbind(Request.ToGenericHttpRequest(), new Saml2LogoutResponse(config));

            return Redirect(Url.Content("~/"));
        }

        [Route("SingleLogout")]
        public async Task<IActionResult> SingleLogout()
        {
            Saml2StatusCodes status;
            var requestBinding = new Saml2PostBinding();
            var logoutRequest = new Saml2LogoutRequest(config, User);
            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), logoutRequest);
                status = Saml2StatusCodes.Success;
                await logoutRequest.DeleteSession(HttpContext);
            }
            catch (Exception exc)
            {
                // log exception
                Debug.WriteLine("SingleLogout error: " + exc.ToString());
                status = Saml2StatusCodes.RequestDenied;
            }

            var responsebinding = new Saml2PostBinding();
            responsebinding.RelayState = requestBinding.RelayState;
            var saml2LogoutResponse = new Saml2LogoutResponse(config)
            {
                InResponseToAsString = logoutRequest.IdAsString,
                Status = status,
            };
            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }


    }
}
