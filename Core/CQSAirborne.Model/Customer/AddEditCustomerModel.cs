using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CQSAirborne.Model.Customer
{
    public class CustomExcelColumn : Attribute
    {
        public int ColumnIndex { get; }
        public CustomExcelColumn(int index)
        {
            ColumnIndex = index;
        }
    }
    public class AddEditCustomerModel : BaseValidateModel
    {
        public long Id { get; set; }
        [Required(ErrorMessage ="Customer Name is Required")]
        public string CustomerName { get; set; }
        //[Required(ErrorMessage = "Contact Number is Required")]
        public string ContactNumber { get; set; }
        [Required(ErrorMessage = "Email is Required")]
        //[EmailAddress(ErrorMessage = "Please enter valid email")]
        //[RegularExpression(@"^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*[,]{0,1}\s*)+$", ErrorMessage = "Please enter valid email")]
        //[RegularExpression(@"^(|([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*$", ErrorMessage = "Please enter a valid e-mail adress")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }

        public DateTime CreatedOn { get; set; }

        [DisplayName("Document")]
        public int DocumentId { get; set; }
        [DisplayName("Attachment")]
        public long PictureId { get; set; }
        public string Path { get; set; }
        public string GeneratedPath { get; set; }
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }
        [DisplayName("End Date")]
        public DateTime EndDate { get; set; }
        public bool IsSendEmail { get; set; }

        public List<CustomerDocumentMappingModel> CustomerDocumentDetail { get; set; }
    }
}
