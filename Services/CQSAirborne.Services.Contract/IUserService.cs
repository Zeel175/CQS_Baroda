using CQSAirborne.Model;
using CQSAirborne.Model.Employee;
using System;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IUserService
    {
        LoginResult LoginUserAsync(LoginViewModel loginViewModel);
        bool Insert(AddEditUserViewModel model);
        Task<bool> Update(AddEditUserViewModel model);
        string DecryptData(string EncryptedText);
        AccountResponseViewModel ForgotPassword(string Email);
        bool IsUserEmailAvailable(string userName, long? Id);
    }
}
