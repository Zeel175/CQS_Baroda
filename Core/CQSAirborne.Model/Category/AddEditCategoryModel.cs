using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CQSAirborne.Model.Category
{
    public class AddEditCategoryModel : BaseValidateModel
    {
        public int Id { get; set; }

        [DisplayName("Category Code")]
        public string Code { get; set; }

        [DisplayName("Category Name")]
        public string Name { get; set; }

        [DisplayName("Detailed Process Owner")]
        public string Description { get; set; }

        [DisplayName("Category Type")]
        public int CategoryTypeId { get; set; }

        [DisplayName("Primary Category")]
        public int? PrimaryCategoryId { get; set; }
        public string Remark { get; set; }
        public bool IsAvailableForDownload { get; set; }

        [DisplayName("Display Order")]
        public int DisplayOrder { get; set; }
    }
}
