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
using CQSAirborne.Web.Infrastructure.Models;
using CQSAirborne.Web.Infrastructure.Contracts;

namespace CQSAirborne.Services.Implementation
{
    public class CustomerPortalService : ICustomerPortalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;
        private readonly string BASEURL;
        private readonly IRestClient _restClient;

        public CustomerPortalService(ICustomerRepository customerRepository
            , IConfiguration configuration, IRestClient restClient)
        {
            _customerRepository = customerRepository;
            _configuration = configuration;
            _restClient = restClient;
            BASEURL = _configuration.GetSection("CustomerWebPath").Value;
        }

        public Task<bool> UploadFile(CustomerFileModel model, MemoryStream memory)
        {
            //RestRequest<IFormFile> reqeust = new RestRequest<IFormFile>($"{BASEURL}Home/UploadFile?Path={model.Path}", RestMethodType.Post, model.File);
            //return _restClient.ExecutePostFileAsync<dynamic>(reqeust);

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(BASEURL);
                    IFormFile file = model.File;
                    byte[] data;


                    //using (var br = new BinaryReader(file.OpenReadStream()))
                    //    data = br.ReadBytes((int)file.OpenReadStream().Length);

                    data = ReadToEnd(memory);

                    ByteArrayContent bytes = new ByteArrayContent(data);


                    MultipartFormDataContent multiContent = new MultipartFormDataContent();

                    multiContent.Add(bytes, "file", file.FileName);

                    var result = client.PostAsync($"Home/UploadFile?Path={model.Path}", multiContent).Result;


                    return Task.Run(() => true);

                }
                catch (Exception ex)
                {
                    return Task.Run(() => false); // 500 is generic server error
                }
            }


        }
        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

    }
}
