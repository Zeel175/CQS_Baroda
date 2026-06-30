using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Core
{
    public struct Constants
    {
        public struct ModuleNames
        {
            public const string CategoryType = "CategoryType";
            public const string DocumentType = "DocumentType";
            public const string CPRStatusType = "CPRStatusType";
            public const string CPRStage = "CPRStage";
        }
        public struct CPRStatusType
        {
            public const int Open = 9;
            public const int Awaiting_Approval = 10;
            public const int Approved = 11;
            public const int Rejected = 12;
            public const int Closed = 16;
        }
        public struct CPRStage
        {
            public const int Generate = 13;
            public const int Level_I = 14;
            public const int Level_II = 15;
        }

        public struct LoginScheme
        {
            public const string scheme = "saml2";
            public const string cookies = "saml2.cookies";
        }

        public struct SSOSecretKey
        {
            public const string key = "skyward@#2018@TATA!Uk";
        }


        public struct CategoryType
        {
            public const string Primary = "CAT_PR";
            public const string Secondary = "CAT_SC";
        }

        public struct CodeModule
        {
            public const string Category = "CAT";
            public const string Document = "DOC";
        }

        public struct DocumentType
        {
            public const string PlantSpecific = "DOC_PLNT";
            public const string Common = "DOC_COM";
            public const string Global = "DOC_GLBL";
        }

        public struct QuickSearchDbColumn
        {
            public const string CategoryName = "CategoryName";
            public const string DocumentName = "DocumentName";
            public const string DocumentCode = "DocumentCode";
            public const string DocumentUniqueCode = "DocumentUniqueCode";
            public const string RevisionNumber = "RevisionNumber";
            public const string ALL = "All";
        }

        public struct PermissionTypeConstant
        {
            public const int Add = 6;
            public const int Edit = 7;
            public const int List = 8;
            public const int Delete = 9;
        }
    }
}
