using CQSAirborne.Data.Contract;
using CQSAirborne.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Chroma.Integration.Service.Services
{
    public class ADUserSyncService : IADUserSyncService
    {
        private readonly IDataContext _context;
        private readonly IConfiguration _configuration;

        public ADUserSyncService(IDataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SyncNewUsersAsync()
        {
            try
            {
                var existingAdIds = _context.GetNoTracking<EmployeeEntity>()
                    .Where(x => x.IsActive && x.ADID != null && x.ADID != string.Empty)
                    .Select(x => x.ADID.ToLower().Trim())
                    .ToHashSet();

                var employeesToCreate = new List<EmployeeEntity>();
                var ldapPaths = GetLdapPaths();

                foreach (var ldapPath in ldapPaths)
                {
                    using (var entry = new DirectoryEntry(ldapPath))
                    using (var searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = "(&(objectCategory=person)(objectClass=user))";
                        searcher.PageSize = 1000;

                        AddPropertiesToLoad(searcher);

                        foreach (SearchResult result in searcher.FindAll())
                        {
                            try
                            {
                                var userName = GetProperty(result, "samaccountname").Trim().ToLower();
                                var accountControl = GetProperty(result, "userAccountControl");

                                if (string.IsNullOrWhiteSpace(userName) || IsDisabledAccount(accountControl) || existingAdIds.Contains(userName))
                                {
                                    continue;
                                }

                                var employee = new EmployeeEntity
                                {
                                    EmployeeName = GetProperty(result, "name"),
                                    GroupId = GetProperty(result, "description"),
                                    EmpId = GetProperty(result, "employeeID"),
                                    OfficalEmpEmailID = GetProperty(result, "mail"),
                                    ADID = userName,
                                    UserName = userName,
                                    Password = string.Empty,
                                    Department = NormalizeDepartment(GetProperty(result, "department")),
                                    Designation = GetProperty(result, "title"),
                                    Plant = ExtractOU(ldapPath),
                                    PlantId = ResolvePlantId(ldapPath),
                                    OrgRole = GetDefaultOrgRole(),
                                    IsActive = true,
                                    IsManual = false,
                                    CreatedOn = DateTime.Now,
                                    CreatedBy = GetDefaultCreatedBy()
                                };

                                employeesToCreate.Add(employee);
                                existingAdIds.Add(userName);

                                LogUser($"Queued AD user '{userName}' for DB creation.");
                            }
                            catch (Exception innerException)
                            {
                                LogError(innerException.ToString());
                            }
                        }
                    }
                }

                if (employeesToCreate.Any())
                {
                    foreach (var employee in employeesToCreate)
                    {
                        _context.Insert(employee);
                    }

                    await _context.SaveChangesAsync();
                    LogUser($"Inserted {employeesToCreate.Count} AD users successfully.");
                }
                else
                {
                    LogUser("No new AD users found.");
                }
            }
            catch (Exception exception)
            {
                LogError(exception.ToString());
            }
        }

        private static void AddPropertiesToLoad(DirectorySearcher searcher)
        {
            searcher.PropertiesToLoad.Add("name");
            searcher.PropertiesToLoad.Add("mail");
            searcher.PropertiesToLoad.Add("samaccountname");
            searcher.PropertiesToLoad.Add("userAccountControl");
            searcher.PropertiesToLoad.Add("department");
            searcher.PropertiesToLoad.Add("title");
            searcher.PropertiesToLoad.Add("employeeID");
            searcher.PropertiesToLoad.Add("description");
        }

        private List<string> GetLdapPaths()
        {
            var paths = _configuration.GetSection("ADUserSync:LdapPaths").Get<List<string>>();
            return paths?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
        }

        private static string GetProperty(SearchResult result, string propertyName)
        {
            return result.Properties[propertyName].Count > 0 ? result.Properties[propertyName][0]?.ToString()?.Trim() ?? string.Empty : string.Empty;
        }

        private static bool IsDisabledAccount(string userAccountControl)
        {
            return int.TryParse(userAccountControl, out var flags) && (flags & 0x2) == 0x2;
        }

        private int? ResolvePlantId(string ldapPath)
        {
            var ouName = ExtractOU(ldapPath);
            var plant = _context.GetNoTracking<PlantEntity>()
                .FirstOrDefault(x => x.IsActive && x.Name.ToLower().Trim() == ouName.ToLower().Trim());

            return plant?.Id ?? _configuration.GetValue<int?>("ADUserSync:DefaultPlantId");
        }

        private string NormalizeDepartment(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                return string.Empty;
            }

            var normalizedDepartment = department.Trim();
            if (normalizedDepartment.Contains("-"))
            {
                normalizedDepartment = normalizedDepartment.Split('-').Last().Trim();
            }

            var mappings = _configuration.GetSection("ADUserSync:DepartmentMappings").Get<Dictionary<string, string>>()
                ?? new Dictionary<string, string>();

            return mappings.TryGetValue(normalizedDepartment.ToLower(), out var mappedDepartment) ? mappedDepartment : normalizedDepartment;
        }

        private string ExtractOU(string ldapPath)
        {
            var parts = ldapPath.Split(',');

            foreach (var part in parts)
            {
                if (part.StartsWith("OU=", StringComparison.OrdinalIgnoreCase) &&
                    !part.Contains("Users", StringComparison.OrdinalIgnoreCase) &&
                    !part.Contains("Permanent", StringComparison.OrdinalIgnoreCase))
                {
                    return part.Replace("OU=", string.Empty).Trim();
                }
            }

            return string.Empty;
        }

        private string GetDefaultOrgRole()
        {
            return _configuration.GetValue<string>("ADUserSync:DefaultOrgRole") ?? "User";
        }

        private int GetDefaultCreatedBy()
        {
            return _configuration.GetValue<int?>("ADUserSync:DefaultCreatedBy") ?? 1;
        }

        private void LogError(string message)
        {
            WriteLog("ad_user_sync_error.txt", message);
        }

        private void LogUser(string message)
        {
            WriteLog("ad_user_sync_users.txt", message);
        }

        private void WriteLog(string fileName, string message)
        {
            var configuredPath = _configuration.GetValue<string>("ADUserSync:LogPath");
            var logFolder = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                : configuredPath;

            Directory.CreateDirectory(logFolder);
            File.AppendAllText(Path.Combine(logFolder, fileName), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
        }
    }
}
