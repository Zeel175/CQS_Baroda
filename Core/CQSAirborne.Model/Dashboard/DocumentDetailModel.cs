using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Dashboard
{
    public class DocumentDetailModel : BaseModel
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public bool CanDownload { get; set; }
        public string DisplayFileName { get; set; }
    }
}
