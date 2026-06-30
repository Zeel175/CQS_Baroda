using CQSAirborne.Model.Dashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class DocumentListModel : BaseModel
    {
        public DocumentListModel()
        {
            Plants = new List<PlantDocumentListModel>();
        }
        public int Id { get; set; }
        public string Code { get; set; }
        public string DocumentTypeCode { get; set; }
        public string DocumentTypeName { get; set; }
        public int DocumentTypeId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime UploadDate { get; set; }
        public string RevisionNumber { get; set; }
        public string Alias { get; set; }
        public string ProcessOwner { get; set; }
        public string Source { get; set; }
        public List<PlantDocumentListModel> Plants { get; set; }

        public DateTime? InActiveDate { get; set; }
        public DateTime? ActualInActiveDate { get; set; }
        public bool IsActive { get; set; }
        public string ReasonForChange { get; set; }
        public string CPRNumber { get; set; }
        public long? CPRMasterId { get; set; }
        public string Remarks { get; set; }
        public int? PlantTableId { get; set; }
        public bool CanDownload { get; set; }
        public string RevisionDate
        {
            get { return UploadDate.ToString("dd-MMM-yyyy"); }
        }
        public string InActiveDt
        {
            get {
                if (InActiveDate != null && InActiveDate.Value != DateTime.MinValue)
                {
                    return InActiveDate.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    return "";
                }
            }
        }

        public string ActualInActiveDt
        {
            get
            {
                if (ActualInActiveDate != null && ActualInActiveDate.Value != DateTime.MinValue)
                {
                    return ActualInActiveDate.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    return "";
                }
            }
        }
        public string DocTypePrefix
        {
            get
            {
                if (DocumentNumber != null)
                {
                    string[] str = DocumentNumber.Split(" ");
                    return str[0] != null ? str[0].ToUpper().ToString() : string.Empty;
                }
                return "";
            }
        }
    }
}
