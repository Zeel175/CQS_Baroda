using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Plant
{
    public class PlantListModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
