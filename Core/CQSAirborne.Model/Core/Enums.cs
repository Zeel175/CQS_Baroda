using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Core
{
    public enum UploadType : int
    {
        Document = 1,
        TemporaryImages = 2
    }

    public enum DocumentOperationType
    {
        Document = 1,
        History = 2,
        Common = 3
    }

    public enum PermissionScreenType : int
    {
        Role,
        User
    }
    public static class Role
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public static class ScreenCode
    {
        public const string Role = "PER_ROLE";
        public const string Category = "PER_CATEGORY";
        public const string CompanyCredentials = "PER_COMPANYCREDENTIALS";
        public const string Customer = "PER_CUSTOMER";
        public const string Document = "PER_DOCUMENT";
        public const string EmailDocuments = "PER_EMAILDOCUMENTS";
        public const string Employee = "PER_EMPLOYEE";
        public const string ExternalLink = "PER_EXTERNALLINK";
        public const string MasterPCFTracker = "PER_MASTERPCFTRACKER";
        public const string Plant = "PER_PLANT";
        public const string QualityAlerts = "PER_QUALITYALERTS";
        public const string Standards = "PER_STANDARDS";
        public const string TASLPCFCount = "PER_TASLPCFCOUNT";
        public const string TraningandSupport = "PER_TRANINGANDSUPPORT";
        public const string CPRMaster = "PER_CPRMASTER";
        public const string CPRAddFormMaster = "PER_CPRMASTERADDFORM";
    }

}
