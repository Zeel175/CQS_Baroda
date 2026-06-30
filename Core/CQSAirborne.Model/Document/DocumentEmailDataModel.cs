using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class DocumentEmailDataModel
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string SpecificPersonEmail { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentName { get; set; }
        public string DocumentNumber { get; set; }
        public string RevisionNumber { get; set; }
        public string ProcessOwner { get; set; }
        public int DocumentTypeId { get; set; }
        public bool IsMailSend { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public string PlantName { get; set; }
        public string RevisionDate { get {
                string date = "";
                if(ModifiedOn != null)
                {
                    date = ModifiedOn.Value.ToString("dd-MMM-yyyy");
                }
                return date;
            } }
    }

    public class EmployeewiseDocumentMailModel
    {
        public EmployeewiseDocumentMailModel()
        {
            this.DocumentModel = new List<DocumentMailModel>();
        }
        public string EmployeeEmail { get; set; }
        public string DocIds { get; set; }
        public List<string> EmailIds { get; set; }
        public List<DocumentMailModel> DocumentModel { get; set; }
    }

    public class DocumentMailModel
    {
        public string DocumentName { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string RevisionNo { get; set; }
        public string Location { get; set; }
    }

    public class EmailPreviewModel
    {
        public string Subject { get; set; }
        public List<string> Recipients { get; set; }
        public List<DocumentMailModel> Documents { get; set; }
    }
    public class EmailPreviewRequest
    {
        public string Ids { get; set; }     // "1,2,3"
        public string Title { get; set; }   // optional subject extension
    }

    public class SpecialPlantEmailConfig
    {
        public List<string> PlantAliases { get; set; } = new List<string>();
        public string Emails { get; set; } = string.Empty;
    }
}
