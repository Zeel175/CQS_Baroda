using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.ExternalLink
{
    public class ExternalLinkListModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Url { get; set; }
        public int Priority { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
    }
}
