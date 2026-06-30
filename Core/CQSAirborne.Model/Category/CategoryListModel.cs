using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CQSAirborne.Model.Category
{
    public class CategoryListModel : BaseModel
    {
        public int Id { get; set; }

        [DisplayName("Category Code")]
        public string Code { get; set; }

        [DisplayName("Category Name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryType { get; set; }
        public string PrimaryCategory { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
