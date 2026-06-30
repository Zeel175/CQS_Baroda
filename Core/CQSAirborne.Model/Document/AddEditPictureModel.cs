using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Document
{
    public class AddEditPictureModel : BaseModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string DisplayName { get; set; }
    }
}
