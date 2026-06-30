using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Core
{
    public class SelectListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsCPREnable { get; set; } = false;
    }

    public class PlantSelectListModel : SelectListModel
    {
        public int DisplayOrder { get; set; }
    }

    public class CategorySelectListModel : SelectListModel
    {
        public string PrimaryCategoryName { get; set; }
    }
    public class StructureRoleSelectListModel : SelectListModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
