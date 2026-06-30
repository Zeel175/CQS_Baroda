using CQSAirborne.Domain;
using CQSAirborne.Model;
using CQSAirborne.Model.Employee;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using CQSAirborne.Services.Implementation.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Implementation
{
    public class UserService : IUserService
    {
        public const string ENCRYPTION_KEY = "D94A1DC39B9F452F4CEC5B9A47D9C";
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataMapper _dataMapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly EmailHelper _emailHelper;
        private readonly IConfiguration _configuration;
        private readonly string SuspendMinutes;
        private readonly string MaxHitCount;

        public UserService(IUserRepository userRepository
            , IUnitOfWork unitOfWork
            , IDataMapper dataMapper
            , IEmployeeRepository employeeRepository
            , EmailHelper emailHelper
            , IConfiguration configuration)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _dataMapper = dataMapper;
            _employeeRepository = employeeRepository;
            _emailHelper = emailHelper;
            _configuration = configuration;
            SuspendMinutes = _configuration.GetSection("SuspendMinutes").Value;
            MaxHitCount = _configuration.GetSection("MaxHitCount").Value;
        }

        public LoginResult LoginUserAsync(LoginViewModel loginViewModel)
        {
            var result = new LoginResult();
            var userData = _employeeRepository.GetAll().Where(m => m.UserName == loginViewModel.UserName && m.Password == loginViewModel.Password && m.IsActive == true).FirstOrDefault();
            //var adminUsersData = _employeeRepository.GetAll().Where(m => m.OrgRole == "Admin");
            if (userData != null)
            {
                result.IsSuccess = true;
                result.Claims.Add("userName", userData.UserName);
                result.Claims.Add("name", userData.EmployeeName);
                result.Claims.Add("OrgRole", userData.OrgRole);
                result.Claims.Add("UserId", userData.Id.ToString());
                result.OrgRole = userData.OrgRole;
                result.Id = userData.Id;
                result.EmpName = userData.EmployeeName;
                userData.HitCount = 0;
                userData.LastHitDate = DateTime.Now;
                userData.SuspendDateTime = null;
                var updEmp = Task.Run(() => _employeeRepository.Update(userData));
                _unitOfWork.Commit();

                return result;
            }
            else
            {
                var userData1 = _employeeRepository.GetAll().Where(m => m.UserName == loginViewModel.UserName && m.IsActive == true).FirstOrDefault();
                if (userData1 != null)
                {
                    int susMin = SuspendMinutes != null ? Convert.ToInt32(SuspendMinutes) : 0;
                    int maxHitCount = MaxHitCount != null ? Convert.ToInt32(MaxHitCount) : 0;
                    userData1.HitCount = userData1.HitCount != null ? userData1.HitCount + 1 : 0;
                    userData1.LastHitDate = DateTime.Now;

                    if (userData1.HitCount > maxHitCount)
                    {
                        if (userData1.SuspendDateTime == null || userData1.SuspendDateTime < DateTime.Now)
                        {
                            userData1.SuspendDateTime = DateTime.Now.AddMinutes(susMin);
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Error = $"Your account has been locked for {SuspendMinutes} Minutes due to {MaxHitCount} failed login attempts.";
                        }
                    }

                    _employeeRepository.Update(userData1);
                    _unitOfWork.Commit();
                }
            }
            if (result.Error == "")
            {
                result.Error = "Credentials are invalid";
            }
            return result;
        }
        public bool Insert(AddEditUserViewModel model)
        {
            var entity = _dataMapper.Map<AddEditUserViewModel, UserEntity>(model);
            entity.CreatedOn = DateTime.Now;
            var user = _userRepository.GetAll().Where(m => m.UserName == model.UserName).FirstOrDefault();
            entity.EmployeeId = user == null ? "" : user.Id.ToString();
            _userRepository.Insert(entity);
            return _unitOfWork.Commit() > 0;
        }
        public async Task<bool> Update(AddEditUserViewModel model)
        {
            var entity = _dataMapper.Map<AddEditUserViewModel, UserEntity>(model);
            _userRepository.Delete(entity);
            _unitOfWork.Commit();
            var user = await _employeeRepository.FirstOrDefaultAsync(m => m.UserName == model.UserName);
            entity.EmployeeId = user == null ? "" : user.Id.ToString();
            entity.CreatedOn = DateTime.Now;
            await _userRepository.InsertAsync(entity);
            _unitOfWork.Commit();
            return true;
        }

        public AccountResponseViewModel ForgotPassword(string Email)
        {
            var user = _employeeRepository.GetAll().Where(w => w.OfficalEmpEmailID == Email).FirstOrDefault();
            if (user != null)
            {
                string rowToken = $"{user.UserName};{DateTime.Now.AddDays(1)};{DateTime.Now};";
                var token = EncryptData(rowToken);
                string resetPasswordUrl = $"{_configuration.GetSection("DomainUrl").Value}Account/ResetPassword?Token={token}";
                string body = $"Please follow following url to reset password, this url is valid for 24 hour.</br><a href='{resetPasswordUrl}'>{resetPasswordUrl}</a><br/><br/> Please Ignore if you did not requested this.";
                _emailHelper.SendEmail(user.OfficalEmpEmailID, "Reset Password for your QMS account", body);
            }
            var response = new AccountResponseViewModel()
            {
                Message = "Success",
                IsFailed = false
            };

            return response;
        }
        public string EncryptData(string textData)
        {
            RijndaelManaged objrij = new RijndaelManaged();
            //set the mode for operation of the algorithm
            objrij.Mode = CipherMode.CBC;
            //set the padding mode used in the algorithm.
            objrij.Padding = PaddingMode.PKCS7;
            //set the size, in bits, for the secret key.
            objrij.KeySize = 0x80;
            //set the block size in bits for the cryptographic operation.
            objrij.BlockSize = 0x80;
            //set the symmetric key that is used for encryption & decryption.
            byte[] passBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            //set the initialization vector (IV) for the symmetric algorithm
            byte[] EncryptionkeyBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int len = passBytes.Length;
            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, EncryptionkeyBytes, len);
            objrij.Key = EncryptionkeyBytes;
            objrij.IV = EncryptionkeyBytes;
            //Creates symmetric AES object with the current key and initialization vector IV.
            ICryptoTransform objtransform = objrij.CreateEncryptor();
            byte[] textDataByte = Encoding.UTF8.GetBytes(textData);
            //Final transform the test string.
            return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
        }
        public string DecryptData(string EncryptedText)
        {
            RijndaelManaged objrij = new RijndaelManaged();
            objrij.Mode = CipherMode.CBC;
            objrij.Padding = PaddingMode.PKCS7;
            objrij.KeySize = 0x80;
            objrij.BlockSize = 0x80;
            byte[] encryptedTextByte = Convert.FromBase64String(EncryptedText);
            byte[] passBytes = Encoding.UTF8.GetBytes(ENCRYPTION_KEY);
            byte[] EncryptionkeyBytes = new byte[0x10];
            int len = passBytes.Length;
            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, EncryptionkeyBytes, len);
            objrij.Key = EncryptionkeyBytes;
            objrij.IV = EncryptionkeyBytes;
            byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
            return Encoding.UTF8.GetString(TextByte);  //it will return readable string
        }
        public bool IsUserEmailAvailable(string userName, long? Id)
        {
            var user = _employeeRepository.GetAll().Where(m => m.UserName == userName);
            if (Id != null)
                user = user.Where(m => m.Id != Id);
            var selecteduser = user.FirstOrDefault();
            if (selecteduser == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}
