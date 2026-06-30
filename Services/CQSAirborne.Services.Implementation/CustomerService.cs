using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Employee;
using CQSAirborne.Model.Customer;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using CQSAirborne.Model.Core;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.Security.Cryptography;
using System.Threading;
using CQSAirborne.Services.Implementation.Utils;

namespace CQSAirborne.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        public const string ENCRYPTION_KEY = "D94A1DC39B9F452F4CEC5B9A47D9C";
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerDocumentMappingRepository _customerDocumentMappingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataMapper _dataMapper;
        private readonly IUserService _userService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentPlantRepository _documentPlantRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ICustomerPortalService _customerPortalService;
        private readonly string OtherPortalURL;
        private readonly string CC1;
        private readonly string CC2;
        private readonly EmailHelper _emailHelper;

        public CustomerService(ICustomerRepository customerRepository
            , IUnitOfWork unitOfWork
            , IDataMapper dataMapper
            , IUserService userService, ICustomerDocumentMappingRepository customerDocumentMappingRepository,
            IDocumentRepository documentRepository, IDocumentPlantRepository documentPlantRepository,
            IHostingEnvironment hostingEnvironment, IConfiguration configuration, ICustomerPortalService customerPortalService, EmailHelper emailHelper
            )
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
            _dataMapper = dataMapper;
            _userService = userService;
            _customerDocumentMappingRepository = customerDocumentMappingRepository;
            _documentRepository = documentRepository;
            _documentPlantRepository = documentPlantRepository;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _customerPortalService = customerPortalService;
            OtherPortalURL = _configuration.GetSection("CustomerWebPath").Value;
            CC1 = _configuration.GetSection("CC1").Value;
            CC2 = _configuration.GetSection("CC2").Value;
            _emailHelper = emailHelper;
        }

        public IQueryable<AddEditCustomerModel> GetAll()
        {
            return _dataMapper.Project<CustomerEntity, AddEditCustomerModel>(
                _customerRepository.GetAllNoTracking().Where(a => a.IsActive == true));
        }

        public async Task<AddEditCustomerModel> GetCustomerById(long id)
        {
            var custEntity = await Task.Run(() => _customerRepository.GetById(id));
            if (custEntity == null)
                return null;

            var detail = custEntity.CustomerDocumentMappings.Where(a => a.IsActive == true).ToList();
            custEntity.CustomerDocumentMappings.Clear();
            custEntity.CustomerDocumentMappings = detail;
            var model = _dataMapper.Map<CustomerEntity, AddEditCustomerModel>(custEntity);

            return model;
        }
        public bool CreateEdit(AddEditCustomerModel model, int userId)
        {
            bool ismailSend = false;
            ismailSend = model.IsSendEmail != null ? model.IsSendEmail : false;
            var entity = _dataMapper.Map<AddEditCustomerModel, CustomerEntity>(model);
            if (model.Id == null || model.Id == 0)
            {
                entity.IsActive = true;
                entity.CreatedOn = DateTime.Now;
                entity.ModifiedOn = DateTime.Now;
                entity.CreatedBy = userId;
                entity.ModifiedBy = userId;
                _customerRepository.Insert(entity);
            }
            else
            {
                string emailAll = model.Email;
                var getEntity = _customerRepository.GetById(model.Id);
                if (getEntity != null)
                {
                    var oldEntity = getEntity;
                    model.CreatedOn = getEntity.CreatedOn;
                    entity = _dataMapper.Map<AddEditCustomerModel, CustomerEntity>(model, getEntity);
                    entity.CreatedBy = oldEntity.CreatedBy;
                }
                var mappingData = entity.CustomerDocumentMappings.Where(a => a.IsActive == true).ToList();

                entity.ModifiedOn = DateTime.Now;
                entity.CreatedBy = userId;
                entity.ModifiedBy = userId;
                _customerRepository.Update(entity);

                if (ismailSend)
                {
                    string Subject = $"COSMOS || Requested Documents";
                    string basePath = _hostingEnvironment.ContentRootPath;
                    var filePath = "EmailTemplate/CustomerEmail.template";
                    string path = Path.Combine(basePath, filePath);
                    var template = File.ReadAllText(path).ToString();

                    string tableBody = "";
                    int count = 1;
                    foreach (var item in mappingData)
                    {
                        string str = "";
                        str = "<tr>";
                        str += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + count.ToString() + "</td>";
                        str += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + item.Documents.DocumentNumber.ToString() + "</td>";
                        str += "<td style='border:1px solid;text-decoration:none !important;'>" + item.Picture.DisplayName.ToString() + "</td>";
                        str += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + (item.StartDate != null && item.StartDate != DateTime.MinValue ? item.StartDate.Value.ToString("dd-MMM-yyyy") : "") + "</td>";
                        str += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'>" + (item.EndDate != null && item.EndDate != DateTime.MinValue ? item.EndDate.Value.ToString("dd-MMM-yyyy") : "") + "</td>";
                        str += "<td style='border:1px solid;text-align:center;text-decoration:none !important;'><a target='_blank' href='" + item.CreatedLink.ToString() + "'>View</a></td>";
                        str += "</tr>";
                        tableBody = tableBody + str;
                        count++;
                    }

                    template = template.Replace("###TableBody", tableBody);
                    template = template.Replace("###Password", entity.Password);

                    var cc = new List<string>();
                    if (CC1 != null && CC1 != "")
                    {
                        cc.Add(CC1);
                    }
                    if (CC2 != null && CC2 != "")
                    {
                        cc.Add(CC2);
                    }

                    //cc.Add("srpurohit@tasl.aero");
                    //cc.Add("vvishwambharan@tasl.aero");
                    try
                    {
                        if (emailAll != "")
                        {
                            _emailHelper.SendEmail(emailAll, Subject, template, null, cc);
                            //string[] str = emailAll.Split(",");
                            //if (str.Count() > 0)
                            //{
                            //    foreach (var item in str)
                            //    {
                            //        //            var bcc = new List<string>();
                            //        //bcc = emailAll.Split(",").ToList();
                            //        _emailHelper.SendEmail(item, Subject, template, null, cc);
                            //    }
                            //}

                        }
                    }
                    catch
                    {

                    }

                }

            }
            return _unitOfWork.Commit() > 0;
        }


        public async Task<string> CreateEditCustomerDocumentAsync(CustomerDocumentMappingModel model, int userId)
        {
            string encData = "";
            var documentEntity = _documentRepository.GetById(model.DocumentId);
            if (documentEntity.DocumentType.Code == Constants.DocumentType.Global)
            {
                model.Path = documentEntity.CommonPicture.Path;
            }
            else
            {
                var doc = _documentPlantRepository.GetByDocumentId(model.DocumentId).GroupBy(w => new { w.PictureId, w.Picture.Path }).Where(d => d.Key.PictureId == model.PictureId).Select(s => new
                {
                    path = s.Key.Path
                });
                model.Path = doc.FirstOrDefault().path;
            }
            //string[] filen;
            string basePath = _hostingEnvironment.ContentRootPath;
            string filePath = model.Path.Substring(2, model.Path.Length - 2); //data.Path.Replace("~/", string.Empty);
            string path = Path.Combine(basePath, filePath);
            var memory = new MemoryStream();
            IFormFile file2 = null;
            if (File.Exists(path))
            {

                //Thread.Sleep(1000);
                //Task.Run(() =>
                //{
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                {
                    await stream.CopyToAsync(memory);
                }
                //});
                //Thread.Sleep(1000);

                file2 = new FormFile(memory, 0, memory.Length, Path.GetFileName(path), Path.GetFileName(path));

                string FilePath = $"Uploads/{model.CustomerId.ToString()}/{model.DocumentId.ToString()}/{file2.FileName}";
                encData = EncryptData(FilePath);
                model.GeneratedPath = FilePath;
                CustomerFileModel fileModel = new CustomerFileModel();
                fileModel.File = file2;
                fileModel.Path = FilePath;
                await Task.Run(() => _customerPortalService.UploadFile(fileModel, memory));
            }

            var entity = _dataMapper.Map<CustomerDocumentMappingModel, CustomerDocumentMappingEntity>(model);
            if (encData != string.Empty)
            {
                encData = OtherPortalURL + "Home/DownloadDoc?data=" + encData;
            }

            entity.CreatedLink = encData;
            entity.IsActive = true;
            entity.CreatedOn = DateTime.Now;
            entity.CreatedBy = userId;
            entity.ModifiedBy = userId;
            entity.ModifiedOn = DateTime.Now;

            _customerDocumentMappingRepository.Insert(entity);
            _unitOfWork.Commit();

            return encData;
        }

        public bool DeleteCustomerDocument(long id, int UserId)
        {
            bool result = false;
            var entity = _customerDocumentMappingRepository.GetAll().Where(a => a.Id == id).FirstOrDefault();
            //entity.IsActive = false;
            //entity.ModifiedOn = DateTime.Now;
            //entity.ModifiedBy = UserId;
            //_customerDocumentMappingRepository.Update(entity);

            _customerDocumentMappingRepository.Delete(entity);
            result = true;
            _unitOfWork.Commit();
            return result;
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

        public bool DeleteCustomer(long id, int UserId)
        {
            bool result = false;
            var entity = _customerRepository.GetAll().Where(a => a.Id == id).FirstOrDefault();
            entity.IsActive = false;
            entity.ModifiedOn = DateTime.Now;
            entity.ModifiedBy = UserId;
            _customerRepository.Update(entity);

            var entity2 = _customerDocumentMappingRepository.GetAll().Where(a => a.CustomerId == id && a.IsActive == true);
            if (entity2 != null && entity2.Count() > 0)
            {
                foreach (var item in entity2)
                {
                    item.IsActive = false;
                    item.ModifiedOn = DateTime.Now;
                    item.ModifiedBy = UserId;
                    _customerDocumentMappingRepository.Update(item);
                }
            }
            result = true;
            _unitOfWork.Commit();
            return result;
        }

    }
}
