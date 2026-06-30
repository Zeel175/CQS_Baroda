using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class AddEditDocumentModel : BaseValidateModel
    {
        public AddEditDocumentModel()
        {
            Plants = new List<int>();
            SessionId = Guid.NewGuid();
            Uploads = new List<AddEditDocumentPictureModel>();
            //this.InActiveDate = DateTime.Now;
        }
        public int Id { get; set; }
        public int OldRevId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        [DisplayName("Document Number")]
        public string DocumentNumber { get; set; }

        [DisplayName("Revision Date")]
        public DateTime UploadDate { get; set; }

        [DisplayName("Revision Number")]
        public string RevisionNumber { get; set; }
        public string Alias { get; set; }

        [DisplayName("Process Owner")]
        public string ProcessOwner { get; set; }
        public long DocumentId { get; set; }

        [DisplayName("Primary Category")]
        public string PrimaryCategory { get; set; }

        [DisplayName("Document Type")]
        public int DocumentTypeId { get; set; }
        public List<int> Plants { get; set; }
        public Guid SessionId { get; set; }
        [DisplayName("Tags")]
        public string Tags { get; set; } = "";
        public DateTime InActiveDate { get; set; }
        [DisplayName("Reason for change")]
        public string ReasonForChange { get; set; }
        [DisplayName("CPR Number")]
        public string CPRNumber { get; set; }
        [DisplayName("CPR Number")]
        public long? CPRMasterId { get; set; }
        public string Remarks { get; set; }
        public List<AddEditDocumentPictureModel> Uploads { get; set; }
    }
}
