using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CQSAirborne.Model;
using CQSAirborne.Model.Employee;
using CQSAirborne.Web.Mvc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CQSAirborne.Web.Mvc.Utils;
using Newtonsoft.Json;
using BotDetect.Web.Mvc;
using Microsoft.AspNetCore.Identity;
using static CQSAirborne.Model.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Saml2.Authentication.Core.Authentication;

namespace CQSAirborne.Web.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthenticateService _authenticateService;
        private readonly ISessionManager _sessionManager;

        public AccountController(AuthenticateService authenticateService, ISessionManager sessionManager)
        {
            _authenticateService = authenticateService;
            _sessionManager = sessionManager;
        }

        public async Task<IActionResult> Login(Guid? Id, string ReturnUrl = "")
        {
            ViewData["ReturnUrl"] = ReturnUrl;

            if (ReturnUrl != "")
            {
                if (User.Identity.IsAuthenticated)
                {
                    return Redirect(ReturnUrl);
                }
            }
            ViewBag.Message = Id != null ? "Your Password is changed. Please Login to continue." : "";

            try
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            }
            catch { }

            return View("Login", new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl = "")
        {
            var response = await _authenticateService.LoginAsync(loginViewModel);
            if (response.IsSuccess)
            {
                var loginResult = response.Data;
                var claims = new List<Claim>
                {
                    new Claim("AuthToken", loginResult.Token),
                    new Claim("AuthExpiry", loginResult.ExpireAt.ToString()),
                    new Claim("OrgRole", loginResult.OrgRole.ToString()),
                    new Claim("UserId", loginResult.Id.ToString()),
                    new Claim("EmpName", loginResult.EmpName.ToString()),
                    new Claim("Permission" ,  JsonConvert.SerializeObject(loginResult.Permissions)),
                    new Claim("IsExternal", "0")
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, LoginScheme.cookies);

                await HttpContext.SignInAsync(
                    LoginScheme.cookies,
                    new ClaimsPrincipal(claimsIdentity));

                if (returnUrl != "")
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Message = "Your Username and Password is Invalid. Please try again.";
            return View("Login", loginViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[CaptchaValidationActionFilter("CaptchaCode", "ExampleCaptcha", "Wrong Captcha!")]
        public async Task<IActionResult> LoginLdap(LoginViewModel loginViewModel, string returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var response = await _authenticateService.LoginLdapAsync(loginViewModel);
                if (response.IsSuccess)
                {
                    var loginResult = response.Data;
                    var claims = new List<Claim>
                {
                    new Claim("AuthToken", loginResult.Token),
                    new Claim("AuthExpiry", loginResult.ExpireAt.ToString()),
                    new Claim("OrgRole", loginResult.OrgRole??""),
                    new Claim("UserName", loginResult.UserName??""),
                    new Claim("EmpName", loginResult.EmpName.ToString()),
                    new Claim("UserId", loginResult.Id.ToString()),
                    new Claim("Permission" ,  JsonConvert.SerializeObject(loginResult.Permissions))
                };

                    var claimsIdentity = new ClaimsIdentity(claims, LoginScheme.cookies);

                    await HttpContext.SignInAsync(
                        LoginScheme.cookies,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)
                        });

                    if (returnUrl != "")
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                if (!response.IsSuccess && response.Error != null && response.Error != "")
                {
                    ViewBag.Message = response.Error;
                }
                else
                {
                    ViewBag.Message = "Your Username and Password is Invalid. Please try again.";
                }
            }
            else
            {
                //MvcCaptcha.ResetCaptcha("ExampleCaptcha");
            }

            return View("Login", loginViewModel);
        }
        //public async Task<IActionResult> LogOut()
        //{
        //    await HttpContext.SignOutAsync();
        //    try
        //    {

        //        var IsExternal = _sessionManager.GetClaim("IsExternal");
        //        if (IsExternal == "1")
        //        {
        //            //return Redirect("https://stubidp.sustainsys.com/Logout");
        //        }

        //        foreach (var cookie in Request.Cookies.Keys)
        //        {
        //            Response.Cookies.Delete(cookie);
        //        }

        //        await HttpContext.SignOutAsync(LoginScheme.scheme);
        //    }
        //    catch { }
        //    return RedirectToAction("Index", "Home");
        //}

        public async Task<IActionResult> LogOut()
        {
            try
            {
                // 1️⃣ Logout your custom login cookie
                await HttpContext.SignOutAsync(LoginScheme.cookies);

                // 2️⃣ Logout the default cookie auth scheme
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // 3️⃣ Logout SAML2 authentication cookie
                await HttpContext.SignOutAsync(Saml2Defaults.AuthenticationScheme);

                // 4️⃣ Delete ALL cookies manually
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie, new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(-1),
                        Path = "/",
                        Secure = true,
                        HttpOnly = false,
                        SameSite = SameSiteMode.None
                    });
                }

                // 5️⃣ Redirect after logout
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                // Log if needed
                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult ChangePassword(Guid? Id)
        {
            ViewBag.Message = Id != null ? "Your Password is changed." : "";
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            model.UserName = ((ClaimsIdentity)User.Identity).GetSpecificClaim("UserName");
            var Message = await _authenticateService.ChangePasswordAsync(model);
            ViewBag.Message = Message.Message;
            return View();
        }
        public ActionResult ForgotPassword(Guid? Id)
        {
            ViewBag.Message = Id != null ? "Please check your Mail box for the reset password link." : "";
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string Email)
        {
            await _authenticateService.ForgotPasswordAsync(Email);
            return RedirectToAction("ForgotPassword", new { Id = Guid.NewGuid() });
        }
        public ActionResult ResetPassword(string Token, string Message)
        {
            ViewBag.Token = Token;
            ViewBag.Message = Message;
            return View();
        }

        public ActionResult UnAuthorize()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            await _authenticateService.ResetPasswordAsync(model);
            return RedirectToAction("Login", "Account", new { Id = Guid.NewGuid() });
        }
        public async Task<IActionResult> IsUserEmailAvailable(string username, long? Id)
        {
            var response = await _authenticateService.IsUserEmailAvailable(username, Id);
            return new JsonResult(response.Data);
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLogoutCallback()
        {
            //await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { provider, returnUrl });

            var prop = new ChallengeResult(provider,
            new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            });
            //var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            //var xval = Challenge(prop, provider);

            return prop;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string provider, string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                //ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await GetExternalLoginInfoAsync(provider);
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new SSORequest();
            model.secretKey = SSOSecretKey.key;
            model.authenticationId = info.Principal.FindFirstValue("subject");
            if (model.authenticationId == null)
            {
                model.authenticationId = "";
            }

            model.userName = "xyx";
            model.name = "test";
            model.OrgRole = "Admin";
            model.IsDirect = true;

            var result = await _authenticateService.SSOAuthenticationAsync(model);
            if (result.IsSuccess)
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

                claims.AddRange(info.Principal.Claims);

                var claimsIdentity = new ClaimsIdentity(claims, LoginScheme.cookies);

                await HttpContext.SignInAsync(LoginScheme.cookies, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("index", "home");
            }

            return RedirectToAction(nameof(Login));

        }

        private async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string scheme)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(scheme);
            if (!authenticateResult.Succeeded)
            {
                return null;
            }

            var properties = authenticateResult.Properties ?? new AuthenticationProperties();
            var providerKey = authenticateResult.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            var loginProvider = "saml2";
            //var authenticationScheme = (await _signInManager.GetExternalAuthenticationSchemesAsync()).FirstOrDefault(s => s.Name == scheme);
            try
            {
                var xinfo = new ExternalLoginInfo(authenticateResult.Principal, loginProvider, providerKey, LoginScheme.scheme);
                return xinfo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}