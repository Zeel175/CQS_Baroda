using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class SendEmailModel
    {
        public int id { get; set; }
        public string PlantName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string DocumentType { get; set; }
        public string SpecificPersonEmail { get; set; }
        public bool? IsSendLater { get; set; }
    }
}
