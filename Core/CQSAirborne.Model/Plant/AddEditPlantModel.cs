using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CQSAirborne.Model.Plant
{
    public class AddEditPlantModel : BaseValidateModel
    {
        public int Id { get; set; }

        [DisplayName("Plant Name")]
        public string Name { get; set; }
        [DisplayName("Site Code")]
        [Required(ErrorMessage ="Site code is required.")]
        public string Alias { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PinCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }

        [DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }

        [DisplayName("Email Address")]
        public string? EmailAddress { get; set; }
        public string? WebSite { get; set; }
        public long? LogoId { get; set; }
        public int DisplayOrder { get; set; }
        public string DMRUserIds { get; set; }
        public string MRUserIds { get; set; }
        public string FinalReleaseToEmails { get; set; }
        public string FinalReleaseCcEmails { get; set; }
        public bool IsCPREnable { get; set; } = false;
    }
}
