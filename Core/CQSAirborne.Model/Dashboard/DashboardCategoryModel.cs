using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Dashboard
{
    public class DashboardCategoryModel : BaseModel
    {
        public DashboardCategoryModel()
        {
            Plants = new List<DashboardDocumentPlantModel>();
        }
        public int ParentCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string CategoryType { get; set; }
        public string HtmlTemplate { get; set; }
        public bool HasChildCategory { get; set; }
        public bool HasSubCategory { get; set; }
        public bool HasDocument { get; set; }
        public List<DashboardDocumentPlantModel> Plants { get; set; }
    }

    public class DashboardDocumentPlantModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
    }
}
