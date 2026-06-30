using CQSAirborne.Chroma.Integration.Service.Models;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Chroma.Integration.Service.Helpers
{
    public class EmployeeCsvService
    {
        private readonly IConfiguration _configuration;
        public EmployeeCsvService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void PushEmployeeData()
        {
            string basePath = _configuration.GetValue<string>("PathToCsv");
            string latestCsvFile = Directory.GetFiles(basePath).Where(w => w.Contains("Employee_Data"))
                    .Select(s =>
                    {
                        string fileName = Path.GetFileNameWithoutExtension(s);
                        string dateTimePortion = fileName.Replace("Employee_Data", string.Empty).Replace("_", string.Empty);
                        return new
                        {
                            FileName = s,
                            FileDate = DateTime.ParseExact(dateTimePortion, "ddMMyyyyhhmmss", CultureInfo.InvariantCulture)
                        };
                    }).OrderByDescending(w => w.FileDate)
                    .Select(w => w.FileName).DefaultIfEmpty().FirstOrDefault();

            if (string.IsNullOrEmpty(latestCsvFile))
                throw new Exception("No file found to read");

            string logFile = Path.Combine(_configuration.GetValue<string>("CsvReadLogPath"), Path.GetFileNameWithoutExtension(latestCsvFile)) + ".log";
            using (StreamWriter w = File.AppendText(logFile))
            {

                using (var reader = new StreamReader(latestCsvFile))
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.Delimiter = ";";
                    csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();
                    csv.Configuration.RegisterClassMap<AddEditEmployeeViewModelMap>();


                    try
                    {
                        var records = csv.GetRecords<AddEditEmployeeViewModel>().ToList();
                        if (!IsUniqueEmployeeId(records))
                        {
                            w.WriteLine("Duplicate employee entries found so not able to process");
                            return;
                        }

                        string baseApiPath = _configuration.GetValue<string>("BaseApiPath");
                        HttpClient _httpClient = new HttpClient
                        {
                            BaseAddress = new Uri(baseApiPath)
                        };
                        _httpClient.PostAsync(baseApiPath + "Employee/BulkUpdate", new StringContent(JsonConvert.SerializeObject(records), Encoding.UTF8, "application/json")).Wait();
                        w.WriteLine($"{records.Count} employee uploaded successfully!");

                    }
                    catch (HeaderValidationException ex)
                    {
                        string missingHeaders = string.Join(',', ex.HeaderNames);
                        w.WriteLine($"Following headers are missing {missingHeaders}");
                        return;
                    }
                }
            }
        }

        private bool IsUniqueEmployeeId(List<AddEditEmployeeViewModel> records)
        {
            return records.GroupBy(w => w.EmpId).All(w => w.Count() == 1);
        }
    }
}
