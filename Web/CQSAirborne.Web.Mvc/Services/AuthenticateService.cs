using CQSAirborne.Model;
using CQSAirborne.Model.Employee;
using CQSAirborne.Services.API.Models;
using CQSAirborne.Web.Infrastructure.Contracts;
using CQSAirborne.Web.Infrastructure.Models;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Services
{
    public class AuthenticateService
    {
        private readonly IRestClient _restClient;

        public AuthenticateService(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<RestReponse<LoginResponse>> LoginAsync(LoginViewModel loginViewModel)
        {
            RestRequest<LoginViewModel> reqeust = new RestRequest<LoginViewModel>("Account/Login", RestMethodType.Post, loginViewModel,"", true);
            var response = await _restClient.ExecutePostAsync<LoginViewModel, LoginResponse>(reqeust);
            return response;
        }
        public async Task<RestReponse<LoginResponse>> LoginLdapAsync(LoginViewModel loginViewModel)
        {
            RestRequest<LoginViewModel> reqeust = new RestRequest<LoginViewModel>("Account/LoginLdap", RestMethodType.Post, loginViewModel,"", true);
            var response = await _restClient.ExecutePostAsync<LoginViewModel, LoginResponse>(reqeust);
            return response;
        }
        public async Task<AccountResponseViewModel> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            RestRequest<ChangePasswordViewModel> reqeust = new RestRequest<ChangePasswordViewModel>("Account/ChangePassword", RestMethodType.Post, model, "", true);
            var response = await _restClient.ExecutePostAsync<ChangePasswordViewModel, AccountResponseViewModel>(reqeust);
            return response.Data;
        }
        public async Task<RestReponse<AccountResponseViewModel>> ForgotPasswordAsync(string Email)
        {
            RestRequest<string> reqeust = new RestRequest<string>("Account/ForgotPassword?Email="+Email+"", RestMethodType.Post, Email, "", true);
            var response = await _restClient.ExecutePostAsync<string, AccountResponseViewModel>(reqeust);
            return response;
        }
        public async Task<RestReponse<LoginResponse>> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            RestRequest<ResetPasswordViewModel> reqeust = new RestRequest<ResetPasswordViewModel>("Account/ResetPassword", RestMethodType.Post, model, "", true);
            var response = await _restClient.ExecutePostAsync<ResetPasswordViewModel, LoginResponse>(reqeust);
            return response;
        }
        public async Task<RestReponse<string>> IsUserEmailAvailable(string userName, long? Id)
        {
            RestRequest<string> reqeust = new RestRequest<string>($"Account/IsUserEmailAvailable?UserName={userName}&Id={Id}", RestMethodType.Get, "");
            var response = await _restClient.ExecuteGetAsync<string>(reqeust);
            return response;
        }

        public async Task<RestReponse<LoginResponse>> SSOAuthenticationAsync(SSORequest model)
        {
            RestRequest<SSORequest> reqeust = new RestRequest<SSORequest>("Account/CreateSSOAuthentication", RestMethodType.Post, model, "", true);
            var response = await _restClient.ExecutePostAsync<SSORequest, LoginResponse>(reqeust);
            return response;
        }
    }
}
