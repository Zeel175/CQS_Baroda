using CQSAirborne.Model.Document;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Chroma.Integration.Service.Helpers
{
    public class HangFIreService
    {
        private readonly IConfiguration _configuration;
        public HangFIreService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> EmailDocument(SendEmailModel sem)
        {
            string baseApiPath = _configuration.GetValue<string>("BaseApiPath");
            HttpClient _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseApiPath)
            };
            var response = await _httpClient.PostAsync(baseApiPath + "Document/EmailDocument", new StringContent(JsonConvert.SerializeObject(sem), Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var customerJsonString = await response.Content.ReadAsStringAsync();
                return customerJsonString;
            }
            else
            {
                var error = string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                return error;
            }
            //w.WriteLine($"{records.Count} employee uploaded successfully!");
        }
    }
}
