using CQSAirborne.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class UploadedDocumentListModel : BaseModel
    {
        public long PictureId { get; set; }
        public string Extension { get; set; }
        public string Plants { get; set; }
        public string DisplayName { get; set; }
    }

    public class AddEditDocumentPictureModel : BaseModel
    {
        public AddEditDocumentPictureModel()
        {
            Plants = new List<SelectListModel>();
        }
        public long PictureId { get; set; }
        public List<SelectListModel> Plants { get; set; }
        public string Path { get; set; }
        public string DisplayName { get; set; }
    }
}
