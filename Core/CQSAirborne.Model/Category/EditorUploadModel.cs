using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Category
{
    public class EditorUploadModel : BaseModel
    {
        public long TempUploadId { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
